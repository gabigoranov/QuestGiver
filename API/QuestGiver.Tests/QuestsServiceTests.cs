using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using OpenAI;
using OpenAI.Chat;
using QuestGiver.Data;
using QuestGiver.Data.Common;
using QuestGiver.Data.Constants;
using QuestGiver.Data.Models;
using QuestGiver.Models.Receive;
using QuestGiver.Services.Quests;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;
using Xunit;

namespace QuestGiver.Tests
{
    /// <summary>
    /// Unit tests for <see cref="QuestsService"/> using an in-memory database.
    /// 
    /// This test suite covers the public quest workflow end to end:
    /// - retrieving the current quest for a group
    /// - creating a quest with validation
    /// - completing a quest and applying XP rewards
    /// - queue refill behavior and AI integration
    /// - error handling for missing quests and unauthorized access
    /// 
    /// The OpenAI client is mocked so the tests focus on service behavior rather than
    /// the external API itself.
    /// </summary>
    public class QuestsServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly Mock<ChatClient> _chatClientMock;
        private readonly Mock<OpenAIClient> _openAiMock;
        private readonly QuestsService _service;

        /// <summary>
        /// Initializes the test fixture with:
        /// - an isolated in-memory database
        /// - repository access to that database
        /// - AutoMapper configured with the application's mapping profile
        /// - an empty configuration object
        /// - mocked OpenAI client and chat client instances
        /// - the <see cref="QuestsService"/> under test
        /// </summary>
        public QuestsServiceTests()
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

            _config = new ConfigurationBuilder().Build();

            _chatClientMock = new Mock<ChatClient>(MockBehavior.Loose);
            _openAiMock = new Mock<OpenAIClient>();

            _openAiMock
                .Setup(x => x.GetChatClient(It.IsAny<string>()))
                .Returns(_chatClientMock.Object);

            _service = new QuestsService(_repo, _mapper, _config, _openAiMock.Object);
        }

        /// <summary>
        /// Cleans up the in-memory database after each test to keep the suite isolated.
        /// </summary>
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        /// <summary>
        /// Creates a valid friend group, a valid user, and a membership link between them.
        /// 
        /// This helper is used by tests that need a group with at least one member.
        /// The created user includes all required properties so EF Core validation succeeds.
        /// </summary>
        private async Task<(FriendGroup Group, User User)> SeedGroupWithUserAsync()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                Username = "testuser",
                BirthDate = new DateTime(2000, 1, 1),
                Description = "desc",
                PasswordHash = "this password hash won't be used",
                Level = 1,
                ExperiencePoints = 0,
                NextLevelExperience = 100,
            };

            var group = new FriendGroup
            {
                Id = Guid.NewGuid(),
                Description = "desc",
                Title = "group"
            };

            var link = new UserFriendGroup
            {
                UserId = user.Id,
                FriendGroupId = group.Id
            };

            await _repo.AddAsync(user);
            await _repo.AddAsync(group);
            await _repo.AddAsync(link);
            await _repo.SaveChangesAsync();

            return (group, user);
        }

        /// <summary>
        /// Seeds the quest queue for a given group with one or more quests.
        /// 
        /// The helper can optionally include a quest scheduled for today and then add
        /// additional quests on future days to simulate a queue that already contains
        /// upcoming work.
        /// </summary>
        private async Task SeedQuestQueueAsync(
            FriendGroup group,
            User user,
            int queuedQuestCount,
            bool includeTodayQuest = true)
        {
            if (includeTodayQuest)
            {
                await _repo.AddAsync(new Quest
                {
                    Id = Guid.NewGuid(),
                    FriendGroupId = group.Id,
                    UserId = user.Id,
                    ScheduledDate = DateTime.UtcNow.Date,
                    Title = "Today quest",
                    Description = "desc",
                    RewardPoints = 10
                });
            }

            for (int i = 1; i < queuedQuestCount; i++)
            {
                await _repo.AddAsync(new Quest
                {
                    Id = Guid.NewGuid(),
                    FriendGroupId = group.Id,
                    UserId = user.Id,
                    ScheduledDate = DateTime.UtcNow.Date.AddDays(i),
                    Title = $"Quest {i}",
                    Description = "desc",
                    RewardPoints = 10
                });
            }

            await _repo.SaveChangesAsync();
        }

        /// <summary>
        /// Builds a minimal ChatCompletion payload from a JSON string so the mocked
        /// OpenAI client can return a response shaped like the real API.
        /// </summary>
        private static ChatCompletion BuildChatCompletionFromJson(string jsonArrayText)
        {
            var payload = new
            {
                id = "chatcmpl-test",
                @object = "chat.completion",
                created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                model = "gpt-5-nano",
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        finish_reason = "stop",
                        message = new
                        {
                            role = "assistant",
                            // Wrap the JSON string in an array of ChatMessageContent
                            content = new[]
                            {
                                new { type = "text", text = jsonArrayText }
                            }
                        }
                    }
                }
            };

            return ModelReaderWriter.Read<ChatCompletion>(
                BinaryData.FromObjectAsJson(payload))!;
        }

        /// <summary>
        /// Configures the mocked chat client to return a valid OpenAI completion response
        /// containing the provided JSON string as the assistant output.
        /// </summary>
        private void SetupAiResponse(string jsonArrayText)
        {
            var completion = BuildChatCompletionFromJson(jsonArrayText);

            _chatClientMock
                .Setup(x => x.CompleteChatAsync(It.IsAny<ChatMessage[]>()))
                .Returns(Task.FromResult(
                    ClientResult.FromValue(
                        BuildChatCompletionFromJson(completion.Content[0].Text),
                        Mock.Of<PipelineResponse>()
                    )
                ));
        }

        #region GetCurrentQuestForGroupAsync

        /// <summary>
        /// Verifies that when a quest exists for the current date and the user belongs
        /// to the friend group, the service returns the expected quest DTO.
        /// </summary>
        [Fact]
        public async Task GetCurrentQuestForGroupAsync_WithValidData_ReturnsQuest()
        {
            var (group, user) = await SeedGroupWithUserAsync();

            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                FriendGroupId = group.Id,
                UserId = user.Id,
                ScheduledDate = DateTime.UtcNow.Date,
                Title = "Quest",
                Description = "Desc",
                RewardPoints = 10
            };

            await _repo.AddAsync(quest);
            await _repo.SaveChangesAsync();

            var result = await _service.GetCurrentQuestForGroupAsync(group.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(quest.Id, result.Id);
            Assert.Equal(quest.Title, result.Title);
        }

        /// <summary>
        /// Verifies that requesting the current quest when no quest is scheduled
        /// for today throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task GetCurrentQuestForGroupAsync_NoQuestToday_Throws()
        {
            var (group, user) = await SeedGroupWithUserAsync();

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.GetCurrentQuestForGroupAsync(group.Id, user.Id));
        }

        /// <summary>
        /// Verifies that a user who is not a member of the group cannot access
        /// the group's current quest and receives <see cref="UnauthorizedAccessException"/>.
        /// </summary>
        [Fact]
        public async Task GetCurrentQuestForGroupAsync_UserNotInGroup_Throws()
        {
            var (group, user) = await SeedGroupWithUserAsync();

            var outsider = new User
            {
                Id = Guid.NewGuid(),
                Email = "out@test.com",
                Username = "outsider",
                BirthDate = new DateTime(2000, 1, 1),
                PasswordHash = "this password hash won't be used",
                Description = "desc"
            };

            await _repo.AddAsync(outsider);

            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                FriendGroupId = group.Id,
                UserId = user.Id,
                ScheduledDate = DateTime.UtcNow.Date,
                Title = "Quest",
                Description = "Desc",
                RewardPoints = 10
            };

            await _repo.AddAsync(quest);
            await _repo.SaveChangesAsync();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.GetCurrentQuestForGroupAsync(group.Id, outsider.Id));
        }

        #endregion

        #region CreateQuestAsync

        /// <summary>
        /// Verifies that creating a quest with valid data persists the quest and returns
        /// the expected <see cref="QuestDTO"/>.
        /// </summary>
        [Fact]
        public async Task CreateQuestAsync_WithValidData_CreatesQuest()
        {
            var (group, user) = await SeedGroupWithUserAsync();

            var dto = new CreateQuestDTO
            {
                Title = "Quest",
                Description = "Desc",
                RewardPoints = 10,
                ScheduledDate = DateTime.UtcNow.Date.AddDays(1)
            };

            var result = await _service.CreateQuestAsync(group.Id, user.Id, dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Title, result.Title);

            var inDb = await _context.Set<Quest>().SingleAsync();
            Assert.Equal(group.Id, inDb.FriendGroupId);
            Assert.Equal(user.Id, inDb.UserId);
            Assert.Equal(dto.ScheduledDate.Date, inDb.ScheduledDate.Date);
        }

        /// <summary>
        /// Verifies that trying to create a second quest for the same group and date
        /// throws <see cref="ArgumentException"/>.
        /// </summary>
        [Fact]
        public async Task CreateQuestAsync_DuplicateDate_Throws()
        {
            var (group, user) = await SeedGroupWithUserAsync();

            var date = DateTime.UtcNow.Date.AddDays(1);

            await _repo.AddAsync(new Quest
            {
                Id = Guid.NewGuid(),
                FriendGroupId = group.Id,
                UserId = user.Id,
                ScheduledDate = date,
                Title = "Existing",
                Description = "Desc",
                RewardPoints = 10
            });

            await _repo.SaveChangesAsync();

            var dto = new CreateQuestDTO
            {
                Title = "New",
                Description = "Desc",
                RewardPoints = 10,
                ScheduledDate = date
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateQuestAsync(group.Id, user.Id, dto));
        }

        #endregion

        #region CompleteQuestAsync

        /// <summary>
        /// Verifies that completing the current quest:
        /// - marks the quest as completed
        /// - awards experience points to the user
        /// - does not call AI when the queue is already sufficiently filled
        /// </summary>
        [Fact]
        public async Task CompleteQuestAsync_WithEnoughQueuedQuests_CompletesQuest_AddsXp_AndDoesNotCallAi()
        {
            var (group, user) = await SeedGroupWithUserAsync();

            var desiredQueueSize = QuestQueeueConstants.DesiredQueeueSize;
            await SeedQuestQueueAsync(group, user, desiredQueueSize + 1, includeTodayQuest: true);

            var todayQuest = await _context.Set<Quest>()
                .FirstAsync(x => x.ScheduledDate == DateTime.UtcNow.Date && x.FriendGroupId == group.Id);

            todayQuest.UserId = user.Id;
            todayQuest.RewardPoints = 40;
            await _context.SaveChangesAsync();

            var result = await _service.CompleteQuestAsync(todayQuest.Id, user.Id);

            var updatedQuest = await _context.Set<Quest>().FirstAsync(x => x.Id == todayQuest.Id);
            var updatedUser = await _context.Set<User>().FirstAsync(x => x.Id == user.Id);

            Assert.NotNull(result);
            Assert.True(updatedQuest.DateCompleted.HasValue);
            Assert.Equal(40, updatedUser.ExperiencePoints);

            _chatClientMock.Verify(
                x => x.CompleteChatAsync(It.IsAny<ChatMessage[]>()),
                Times.Never);
        }

        /// <summary>
        /// Verifies that when the quest queue is too small, completing a quest triggers
        /// AI-based quest generation and adds replacement quests to the database.
        /// </summary>
        [Fact]
        public async Task CompleteQuestAsync_WhenQueueNeedsRefill_RetriesAi_AndAddsReplacementQuests()
        {
            var (group, user) = await SeedGroupWithUserAsync();

            await SeedQuestQueueAsync(group, user, 1, includeTodayQuest: true);

            var todayQuest = await _context.Set<Quest>()
                .FirstAsync(x => x.ScheduledDate == DateTime.UtcNow.Date && x.FriendGroupId == group.Id);

            todayQuest.UserId = user.Id;
            todayQuest.RewardPoints = 25;
            await _context.SaveChangesAsync();

            var generatedQuests = new[]
            {
                new GeneratedQuestDTO
                {
                    UserId = user.Id,
                    Title = "Generated quest",
                    Description = "Generated description",
                    RewardPoints = 15
                }
            };

            SetupAiResponse(JsonSerializer.Serialize(generatedQuests));

            var result = await _service.CompleteQuestAsync(todayQuest.Id, user.Id);

            Assert.NotNull(result);
            Assert.True((await _context.Set<Quest>().CountAsync()) >= 2);
            _chatClientMock.Verify(
                x => x.CompleteChatAsync(It.IsAny<ChatMessage[]>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that attempting to complete a quest that does not exist for the
        /// specified user throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task CompleteQuestAsync_UnknownQuest_Throws()
        {
            var (_, user) = await SeedGroupWithUserAsync();

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.CompleteQuestAsync(Guid.NewGuid(), user.Id));
        }

        #endregion
    }
}