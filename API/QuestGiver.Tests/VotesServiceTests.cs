using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuestGiver.Data;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using QuestGiver.Exceptions;
using QuestGiver.Models.Common;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using QuestGiver.Services.Quests;
using QuestGiver.Services.Votes;
using Xunit;

namespace QuestGiver.Tests
{
    /// <summary>
    /// Unit tests for <see cref="VotesService"/> using an in-memory database.
    ///
    /// These tests cover all public methods:
    /// - GetLatestQuestVoteAsync
    /// - CreateVoteAsync (completion & skip votes)
    /// - SubmitIndividualVoteAsync
    /// - CreateUserVoteAsync
    /// - DeleteUserVoteAsync
    ///
    /// The <see cref="IQuestsService"/> is mocked so tests focus on vote logic
    /// rather than quest completion/skip side-effects.
    /// </summary>
    public class VotesServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly Mock<IQuestsService> _questsServiceMock;
        private readonly VotesService _service;

        /// <summary>
        /// Initializes the test fixture with an isolated in-memory database,
        /// AutoMapper configured with the application's profile, and a mocked
        /// <see cref="IQuestsService"/>.
        /// </summary>
        public VotesServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repo = new Repository(_context);

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile<QuestGiver.Models.Common.AutoMapper>();
            });
            _mapper = mapperConfig.CreateMapper();

            _questsServiceMock = new Mock<IQuestsService>();

            // Default mock: complete/skip calls succeed silently
            _questsServiceMock
                .Setup(x => x.CompleteQuestAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new QuestDTO { Id = Guid.NewGuid() });
            _questsServiceMock
                .Setup(x => x.SkipQuestAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new QuestDTO { Id = Guid.NewGuid() });

            _service = new VotesService(_repo, _mapper, _questsServiceMock.Object);
        }

        /// <summary>
        /// Cleans up the in-memory database after each test.
        /// </summary>
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Helpers

        /// <summary>
        /// Creates and persists a fully valid <see cref="User"/>.
        /// </summary>
        private async Task<User> SeedUserAsync(string email = "test@test.com")
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = "testuser",
                BirthDate = new DateTime(2000, 1, 1),
                Description = "desc",
                PasswordHash = "hashed"
            };
            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Creates and persists a <see cref="FriendGroup"/>.
        /// </summary>
        private async Task<FriendGroup> SeedGroupAsync()
        {
            var group = new FriendGroup
            {
                Id = Guid.NewGuid(),
                Title = "Test Group",
                Description = "A test group"
            };
            await _repo.AddAsync(group);
            await _repo.SaveChangesAsync();
            return group;
        }

        /// <summary>
        /// Links a user to a group via <see cref="UserFriendGroup"/>.
        /// </summary>
        private async Task LinkUserToGroupAsync(Guid userId, Guid groupId)
        {
            await _repo.AddAsync(new UserFriendGroup
            {
                UserId = userId,
                FriendGroupId = groupId
            });
            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds a quest with a specific assigned user and friend group.
        /// </summary>
        private async Task<Quest> SeedQuestAsync(Guid groupId, Guid userId, DateTime? scheduledDate = null)
        {
            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                FriendGroupId = groupId,
                UserId = userId,
                ScheduledDate = scheduledDate ?? DateTime.UtcNow.Date,
                Title = "Test Quest",
                Description = "A test quest",
                RewardPoints = 10
            };
            await _repo.AddAsync(quest);
            await _repo.SaveChangesAsync();
            return quest;
        }

        /// <summary>
        /// Sets up a complete voting scenario:
        /// - A group with the given number of members
        /// - A quest assigned to the first member
        /// Returns (group, members[], quest, creator).
        /// </summary>
        private async Task<(FriendGroup Group, User[] Members, Quest Quest, User Creator)>
            SeedVoteScenarioAsync(int memberCount = 3)
        {
            var group = await SeedGroupAsync();
            var members = new User[memberCount];

            for (int i = 0; i < memberCount; i++)
            {
                members[i] = await SeedUserAsync($"user{i}@test.com");
                await LinkUserToGroupAsync(members[i].Id, group.Id);
            }

            var quest = await SeedQuestAsync(group.Id, members[0].Id);

            return (group, members, quest, members[0]);
        }

        #endregion

        #region GetLatestQuestVoteAsync

        /// <summary>
        /// Verifies that when an active (undecided) vote exists for a quest
        /// and the user is a voter, the vote DTO is returned.
        /// </summary>
        [Fact]
        public async Task GetLatestQuestVoteAsync_WithActiveVote_ReturnsVoteDTO()
        {
            var (_, members, quest, creator) = await SeedVoteScenarioAsync(3);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Did I complete it?",
                Discriminator = VoteType.CompletionVote
            };

            await _service.CreateVoteAsync(createDto, creator.Id);

            var result = await _service.GetLatestQuestVoteAsync(quest.Id, members[1].Id);

            Assert.NotNull(result);
            Assert.Equal(quest.Id, result.QuestId);
            Assert.Equal(VoteType.CompletionVote, result.Discriminator);
            Assert.Null(result.Decision); // still undecided
        }

        /// <summary>
        /// Verifies that when no active vote exists for the quest, null is returned.
        /// </summary>
        [Fact]
        public async Task GetLatestQuestVoteAsync_NoActiveVote_ReturnsNull()
        {
            var (_, _, quest, _) = await SeedVoteScenarioAsync();

            var result = await _service.GetLatestQuestVoteAsync(quest.Id, quest.UserId);

            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that when a vote exists but the user is not part of the
        /// voter list, <see cref="KeyNotFoundException"/> is thrown to hide
        /// the vote's existence.
        /// </summary>
        [Fact]
        public async Task GetLatestQuestVoteAsync_UserNotInVote_ThrowsKeyNotFoundException()
        {
            var (_, members, quest, creator) = await SeedVoteScenarioAsync(3);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Vote",
                Discriminator = VoteType.CompletionVote
            };
            await _service.CreateVoteAsync(createDto, creator.Id);

            // A user not in the group
            var outsider = await SeedUserAsync("outsider@test.com");

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.GetLatestQuestVoteAsync(quest.Id, outsider.Id));
        }

        #endregion

        #region CreateVoteAsync

        /// <summary>
        /// Verifies that the assigned quest user can create a completion vote,
        /// all group members get UserVote entries, and the creator auto-votes yes.
        /// </summary>
        [Fact]
        public async Task CreateVoteAsync_CreationVote_CreatesVoteWithUserVotes()
        {
            var (_, members, quest, creator) = await SeedVoteScenarioAsync(3);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Did I complete it?",
                Discriminator = VoteType.CompletionVote
            };

            var result = await _service.CreateVoteAsync(createDto, creator.Id);

            Assert.NotNull(result);
            Assert.Equal(quest.Id, result.QuestId);
            Assert.Equal(VoteType.CompletionVote, result.Discriminator);
            Assert.Equal(3, result.UserVotes.Count);

            // Creator auto-votes yes
            var creatorVote = result.UserVotes.First(v => v.UserId == creator.Id);
            Assert.True(creatorVote.Decision);

            // Others are undecided
            foreach (var member in members.Skip(1))
            {
                var memberVote = result.UserVotes.First(v => v.UserId == member.Id);
                Assert.Null(memberVote.Decision);
            }
        }

        /// <summary>
        /// Verifies that a completion vote with unanimous yes (creator is sole member)
        /// triggers CompleteQuestAsync.
        /// </summary>
        [Fact]
        public async Task CreateVoteAsync_SoleMember_CallsCompleteQuestAsync()
        {
            var (group, members, quest, creator) = await SeedVoteScenarioAsync(1);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Done",
                Discriminator = VoteType.CompletionVote
            };

            await _service.CreateVoteAsync(createDto, creator.Id);

            _questsServiceMock.Verify(
                x => x.CompleteQuestAsync(quest.Id, creator.Id),
                Times.Once);
        }

        /// <summary>
        /// Verifies that a skip vote with unanimous yes (sole member)
        /// triggers SkipQuestAsync.
        /// </summary>
        [Fact]
        public async Task CreateVoteAsync_SoleMemberSkip_CallsSkipQuestAsync()
        {
            var (_, _, quest, creator) = await SeedVoteScenarioAsync(1);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Skip this",
                Discriminator = VoteType.SkipVote
            };

            await _service.CreateVoteAsync(createDto, creator.Id);

            _questsServiceMock.Verify(
                x => x.SkipQuestAsync(quest.Id, creator.Id),
                Times.Once);
        }

        /// <summary>
        /// Verifies that a user who is NOT the assigned quest user
        /// receives <see cref="ForbiddenException"/>.
        /// </summary>
        [Fact]
        public async Task CreateVoteAsync_NonAssignedUser_ThrowsForbiddenException()
        {
            var (_, members, quest, _) = await SeedVoteScenarioAsync(3);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Vote",
                Discriminator = VoteType.CompletionVote
            };

            // members[1] is not the assigned user
            await Assert.ThrowsAsync<ForbiddenException>(
                () => _service.CreateVoteAsync(createDto, members[1].Id));
        }

        /// <summary>
        /// Verifies that creating a vote for a non-existent quest
        /// throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task CreateVoteAsync_NonExistentQuest_ThrowsKeyNotFoundException()
        {
            var (_, members, _, creator) = await SeedVoteScenarioAsync();

            var createDto = new CreateVoteDTO
            {
                QuestId = Guid.NewGuid(),
                Description = "Vote",
                Discriminator = VoteType.CompletionVote
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.CreateVoteAsync(createDto, creator.Id));
        }

        /// <summary>
        /// Verifies that when a decided vote already exists for the quest,
        /// creating a new one throws <see cref="ConflictException"/>.
        /// </summary>
        [Fact]
        public async Task CreateVoteAsync_ExistingDecidedVote_ThrowsConflictException()
        {
            var (_, _, quest, creator) = await SeedVoteScenarioAsync(1);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "First vote",
                Discriminator = VoteType.CompletionVote
            };
            await _service.CreateVoteAsync(createDto, creator.Id);

            var createDto2 = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Second vote",
                Discriminator = VoteType.CompletionVote
            };

            await Assert.ThrowsAsync<ConflictException>(
                () => _service.CreateVoteAsync(createDto2, creator.Id));
        }

        #endregion

        #region SubmitIndividualVoteAsync

        /// <summary>
        /// Verifies that a group member can submit their vote decision,
        /// and when majority is reached the vote is decided.
        /// </summary>
        [Fact]
        public async Task SubmitIndividualVoteAsync_MajorityYes_DecidesVoteAndCallsComplete()
        {
            var (_, members, quest, creator) = await SeedVoteScenarioAsync(3);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Did I complete it?",
                Discriminator = VoteType.CompletionVote
            };
            var voteResult = await _service.CreateVoteAsync(createDto, creator.Id);

            // Member 2 votes yes → majority (2/3)
            await _service.SubmitIndividualVoteAsync(voteResult.Id, members[1].Id, true);

            var updatedVote = await _context.Set<Vote>()
                .Include(v => v.UserVotes)
                .FirstAsync(v => v.Id == voteResult.Id);

            Assert.True(updatedVote.Decision);
            _questsServiceMock.Verify(
                x => x.CompleteQuestAsync(quest.Id, creator.Id),
                Times.Once);
        }

        /// <summary>
        /// Verifies that a majority no vote decides the vote as false
        /// and does NOT call CompleteQuestAsync or SkipQuestAsync.
        /// </summary>
        [Fact]
        public async Task SubmitIndividualVoteAsync_MajorityNo_DecidesVoteAsFalse()
        {
            var (_, members, quest, creator) = await SeedVoteScenarioAsync(3);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Did I complete it?",
                Discriminator = VoteType.CompletionVote
            };
            var voteResult = await _service.CreateVoteAsync(createDto, creator.Id);

            // Both other members vote no → majority says no
            await _service.SubmitIndividualVoteAsync(voteResult.Id, members[1].Id, false);
            await _service.SubmitIndividualVoteAsync(voteResult.Id, members[2].Id, false);

            var updatedVote = await _context.Set<Vote>()
                .Include(v => v.UserVotes)
                .FirstAsync(v => v.Id == voteResult.Id);

            Assert.False(updatedVote.Decision);

            // CompleteQuestAsync should NOT be called for a rejected vote
            _questsServiceMock.Verify(
                x => x.CompleteQuestAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
                Times.Never);
        }

        /// <summary>
        /// Verifies that submitting a vote on a vote that has already been
        /// decided throws <see cref="ConflictException"/>.
        /// </summary>
        [Fact]
        public async Task SubmitIndividualVoteAsync_VoteAlreadyDecided_ThrowsConflictException()
        {
            var (_, members, quest, creator) = await SeedVoteScenarioAsync(1);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Done",
                Discriminator = VoteType.CompletionVote
            };
            var voteResult = await _service.CreateVoteAsync(createDto, creator.Id);
            // Sole member already decided it as yes

            await Assert.ThrowsAsync<ConflictException>(
                () => _service.SubmitIndividualVoteAsync(voteResult.Id, creator.Id, false));
        }

        /// <summary>
        /// Verifies that a non-existent vote ID throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task SubmitIndividualVoteAsync_NonExistentVote_ThrowsKeyNotFoundException()
        {
            var (_, members, _, _) = await SeedVoteScenarioAsync();

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.SubmitIndividualVoteAsync(Guid.NewGuid(), members[0].Id, true));
        }

        /// <summary>
        /// Verifies that a user who is not in the voter list for the vote
        /// receives <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task SubmitIndividualVoteAsync_UserNotInVote_ThrowsKeyNotFoundException()
        {
            var (_, members, quest, creator) = await SeedVoteScenarioAsync(3);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Vote",
                Discriminator = VoteType.CompletionVote
            };
            var voteResult = await _service.CreateVoteAsync(createDto, creator.Id);

            var outsider = await SeedUserAsync("outsider@test.com");

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.SubmitIndividualVoteAsync(voteResult.Id, outsider.Id, true));
        }

        /// <summary>
        /// Verifies that a skip vote with majority yes triggers SkipQuestAsync.
        /// </summary>
        [Fact]
        public async Task SubmitIndividualVoteAsync_SkipVoteMajorityYes_CallsSkipQuestAsync()
        {
            var (_, members, quest, creator) = await SeedVoteScenarioAsync(3);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Should we skip?",
                Discriminator = VoteType.SkipVote
            };
            var voteResult = await _service.CreateVoteAsync(createDto, creator.Id);

            // Member 2 votes yes → majority (2/3)
            await _service.SubmitIndividualVoteAsync(voteResult.Id, members[1].Id, true);

            _questsServiceMock.Verify(
                x => x.SkipQuestAsync(quest.Id, creator.Id),
                Times.Once);
        }

        #endregion

        #region CreateUserVoteAsync

        /// <summary>
        /// Verifies that adding a new user to an active vote creates a
        /// <see cref="UserVote"/> entry with undecided status.
        /// </summary>
        [Fact]
        public async Task CreateUserVoteAsync_WithValidData_AddsUserVote()
        {
            var (_, members, quest, creator) = await SeedVoteScenarioAsync(3);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Vote",
                Discriminator = VoteType.CompletionVote
            };
            var voteResult = await _service.CreateVoteAsync(createDto, creator.Id);

            // Create a new user and add them to the vote
            var newUser = await SeedUserAsync("newbie@test.com");
            await LinkUserToGroupAsync(newUser.Id, quest.FriendGroupId);

            await _service.CreateUserVoteAsync(voteResult.Id, newUser.Id);

            var userVote = await _context.Set<UserVote>()
                .FirstOrDefaultAsync(uv => uv.UserId == newUser.Id && uv.VoteId == voteResult.Id);

            Assert.NotNull(userVote);
            Assert.Null(userVote.Decision);
        }

        /// <summary>
        /// Verifies that adding a user vote for a non-existent vote
        /// throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task CreateUserVoteAsync_NonExistentVote_ThrowsKeyNotFoundException()
        {
            var user = await SeedUserAsync();

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.CreateUserVoteAsync(Guid.NewGuid(), user.Id));
        }

        /// <summary>
        /// Verifies that adding a user vote for a non-existent user
        /// throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task CreateUserVoteAsync_NonExistentUser_ThrowsKeyNotFoundException()
        {
            var (_, _, quest, creator) = await SeedVoteScenarioAsync();

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Vote",
                Discriminator = VoteType.CompletionVote
            };
            var voteResult = await _service.CreateVoteAsync(createDto, creator.Id);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.CreateUserVoteAsync(voteResult.Id, Guid.NewGuid()));
        }

        /// <summary>
        /// Verifies that if a vote has already been decided, CreateUserVoteAsync
        /// does nothing (returns without adding a UserVote).
        /// </summary>
        [Fact]
        public async Task CreateUserVoteAsync_AlreadyDecidedVote_DoesNothing()
        {
            var (_, members, quest, creator) = await SeedVoteScenarioAsync(1);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Done",
                Discriminator = VoteType.CompletionVote
            };
            var voteResult = await _service.CreateVoteAsync(createDto, creator.Id);

            var newUser = await SeedUserAsync("late@test.com");
            await LinkUserToGroupAsync(newUser.Id, quest.FriendGroupId);

            // Should not throw and should not create a UserVote
            await _service.CreateUserVoteAsync(voteResult.Id, newUser.Id);

            var userVote = await _context.Set<UserVote>()
                .FirstOrDefaultAsync(uv => uv.UserId == newUser.Id && uv.VoteId == voteResult.Id);

            Assert.Null(userVote);
        }

        #endregion

        #region DeleteUserVoteAsync

        /// <summary>
        /// Verifies that deleting a user's vote removes the <see cref="UserVote"/>
        /// entry and recalculates the vote decision.
        /// </summary>
        [Fact]
        public async Task DeleteUserVoteAsync_WithValidData_RemovesUserVoteAndRecalculates()
        {
            var (_, members, quest, creator) = await SeedVoteScenarioAsync(3);

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Vote",
                Discriminator = VoteType.CompletionVote
            };
            var voteResult = await _service.CreateVoteAsync(createDto, creator.Id);

            // Member 1 votes yes (now 2/3 yes, decided)
            await _service.SubmitIndividualVoteAsync(voteResult.Id, members[1].Id, true);

            var beforeVote = await _context.Set<Vote>()
                .Include(v => v.UserVotes)
                .FirstAsync(v => v.Id == voteResult.Id);
            Assert.True(beforeVote.Decision);

            // Member 2 leaves — delete their vote
            await _service.DeleteUserVoteAsync(voteResult.Id, members[2].Id);

            var afterVote = await _context.Set<Vote>()
                .Include(v => v.UserVotes)
                .FirstAsync(v => v.Id == voteResult.Id);

            // Member 2's vote should be gone
            Assert.DoesNotContain(afterVote.UserVotes, uv => uv.UserId == members[2].Id);
            // With only 2 voters remaining (creator=yes, member1=yes), still majority
            // But recalculated with memberCount=3 (group size), so need 2 for majority
            // 2 yes votes >= ceil(3/2)=2 → still decided
            Assert.True(afterVote.Decision);
        }

        /// <summary>
        /// Verifies that deleting a non-existent UserVote throws
        /// <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task DeleteUserVoteAsync_NonExistentUserVote_ThrowsKeyNotFoundException()
        {
            var (_, members, quest, creator) = await SeedVoteScenarioAsync();

            var createDto = new CreateVoteDTO
            {
                QuestId = quest.Id,
                Description = "Vote",
                Discriminator = VoteType.CompletionVote
            };
            var voteResult = await _service.CreateVoteAsync(createDto, creator.Id);

            // An outsider user who doesn't have a UserVote
            var outsider = await SeedUserAsync("outsider@test.com");

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeleteUserVoteAsync(voteResult.Id, outsider.Id));
        }

        #endregion
    }
}
