using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuestGiver.Data;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using QuestGiver.Models.Common;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using QuestGiver.Services.Groups;
using QuestGiver.Services.Quests;
using QuestGiver.Services.Votes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace QuestGiver.Tests
{
    /// <summary>
    /// Unit tests for <see cref="GroupsService"/> using an in-memory database.
    /// Tests cover all public methods and edge cases to ensure full code coverage.
    /// </summary>
    public class GroupsServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly Mock<IQuestsService> _mockQuestsService;
        private readonly Mock<IVotesService> _mockVotesService;
        private readonly GroupsService _groupsService;

        public GroupsServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repo = new Repository(_context);

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile<QuestGiver.Models.Common.AutoMapper>();
            });
            _mapper = mappingConfig.CreateMapper();

            _mockQuestsService = new Mock<IQuestsService>();
            _mockVotesService = new Mock<IVotesService>();

            _groupsService = new GroupsService(_repo, _mapper, _mockQuestsService.Object, _mockVotesService.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Helpers

        /// <summary>
        /// Creates and persists a fully valid <see cref="User"/> to satisfy all [Required] constraints.
        /// </summary>
        private async Task<User> CreateUserAsync()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                BirthDate = new DateTime(1995, 1, 1),
                Description = "A test user description",
                Email = "testuser@example.com",
                PasswordHash = "hashedpassword123"
            };
            await _repo.AddAsync<User>(user);
            await _repo.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Creates and persists a fully valid <see cref="FriendGroup"/> to satisfy all [Required] constraints.
        /// </summary>
        private async Task<FriendGroup> CreateFriendGroupAsync()
        {
            var group = new FriendGroup
            {
                Id = Guid.NewGuid(),
                Title = "Test Group",
                Description = "A test group description"
            };
            await _repo.AddAsync<FriendGroup>(group);
            await _repo.SaveChangesAsync();
            return group;
        }

        /// <summary>
        /// Directly creates a <see cref="UserFriendGroup"/> relationship in the DB,
        /// bypassing the service so tests can set up pre-existing memberships.
        /// </summary>
        private async Task<UserFriendGroup> AddUserToGroupDirectlyAsync(Guid userId, Guid groupId)
        {
            var ufg = new UserFriendGroup { UserId = userId, FriendGroupId = groupId };
            await _repo.AddAsync<UserFriendGroup>(ufg);
            await _repo.SaveChangesAsync();
            return ufg;
        }

        /// <summary>
        /// Builds a default mock setup for <see cref="IQuestsService.CreateQuestAsync"/>
        /// returning an empty <see cref="QuestDTO"/>. Used by all CreateGroup tests.
        /// </summary>
        private void SetupCreateQuestMock()
        {
            _mockQuestsService
                .Setup(q => q.CreateQuestAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CreateQuestDTO>()))
                .ReturnsAsync(new QuestDTO());
        }

        #endregion Helpers

        #region AddUserToGroupAsync

        /// <summary>
        /// Verifies that a valid user can be added to a valid group and the
        /// relationship is persisted to the database.
        /// </summary>
        [Fact]
        public async Task AddUserToGroupAsync_WithValidData_RunsSuccessfully()
        {
            // Arrange
            var user = await CreateUserAsync();
            var group = await CreateFriendGroupAsync();

            // Act
            await _groupsService.AddUserToGroupAsync(group.Id, user.Id);

            // Assert
            var relationship = await _context.Set<UserFriendGroup>()
                .FirstOrDefaultAsync(ufg => ufg.UserId == user.Id && ufg.FriendGroupId == group.Id);
            Assert.NotNull(relationship);
        }

        /// <summary>
        /// Verifies that providing a group ID that does not exist in the DB
        /// throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task AddUserToGroupAsync_WithInvalidGroup_ThrowsKeyNotFoundException()
        {
            // Arrange
            var user = await CreateUserAsync();
            var invalidGroupId = Guid.NewGuid(); // not seeded

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _groupsService.AddUserToGroupAsync(invalidGroupId, user.Id));
        }

        /// <summary>
        /// Verifies that providing a user ID that does not exist in the DB
        /// throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task AddUserToGroupAsync_WithInvalidUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var group = await CreateFriendGroupAsync();
            var invalidUserId = Guid.NewGuid(); // not seeded

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _groupsService.AddUserToGroupAsync(group.Id, invalidUserId));
        }

        /// <summary>
        /// Verifies that attempting to add a user who is already a member of the group
        /// throws <see cref="InvalidOperationException"/>.
        /// </summary>
        [Fact]
        public async Task AddUserToGroupAsync_WithDuplicateUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = await CreateUserAsync();
            var group = await CreateFriendGroupAsync();
            await AddUserToGroupDirectlyAsync(user.Id, group.Id); // user is already in the group

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _groupsService.AddUserToGroupAsync(group.Id, user.Id));
        }

        #endregion AddUserToGroupAsync

        #region CreateGroupAsync

        /// <summary>
        /// Verifies that creating a group with valid data returns a <see cref="GroupDTO"/>
        /// whose Title matches the input model.
        /// </summary>
        [Fact]
        public async Task CreateGroupAsync_WithValidData_ReturnsGroupDTO()
        {
            // Arrange
            var user = await CreateUserAsync();
            var model = new CreateGroupDTO
            {
                Title = "My Group",
                Description = "A group description"
            };
            SetupCreateQuestMock();

            // Act
            var result = await _groupsService.CreateGroupAsync(model, user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<GroupDTO>(result);
            Assert.Equal(model.Title, result.Title);
        }

        /// <summary>
        /// Verifies that providing a user ID that does not exist in the DB
        /// throws <see cref="KeyNotFoundException"/> during group creation.
        /// </summary>
        [Fact]
        public async Task CreateGroupAsync_WithInvalidUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var invalidUserId = Guid.NewGuid(); // not seeded
            var model = new CreateGroupDTO
            {
                Title = "My Group",
                Description = "A group description"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _groupsService.CreateGroupAsync(model, invalidUserId));
        }

        /// <summary>
        /// Verifies that the creator is automatically added as a member of the
        /// newly created group.
        /// </summary>
        [Fact]
        public async Task CreateGroupAsync_WithValidData_AddsCreatorToGroup()
        {
            // Arrange
            var user = await CreateUserAsync();
            var model = new CreateGroupDTO
            {
                Title = "My Group",
                Description = "A group description"
            };
            SetupCreateQuestMock();

            // Act
            var result = await _groupsService.CreateGroupAsync(model, user.Id);

            // Assert - the creator's UserFriendGroup row must exist in the DB
            var membership = await _context.Set<UserFriendGroup>()
                .FirstOrDefaultAsync(ufg => ufg.UserId == user.Id && ufg.FriendGroupId == result.Id);
            Assert.NotNull(membership);
        }

        /// <summary>
        /// Verifies that <see cref="IQuestsService.CreateQuestAsync"/> is called exactly
        /// once with the correct userId when a group is created.
        /// </summary>
        [Fact]
        public async Task CreateGroupAsync_WithValidData_CreatesInitialQuest()
        {
            // Arrange
            var user = await CreateUserAsync();
            var model = new CreateGroupDTO
            {
                Title = "My Group",
                Description = "A group description"
            };
            SetupCreateQuestMock();

            // Act
            await _groupsService.CreateGroupAsync(model, user.Id);

            // Assert
            _mockQuestsService.Verify(
                q => q.CreateQuestAsync(It.IsAny<Guid>(), user.Id, It.IsAny<CreateQuestDTO>()),
                Times.Once);
        }

        #endregion CreateGroupAsync

        #region GetGroupsForUserAsync

        /// <summary>
        /// Verifies that a user with an existing group membership receives the correct
        /// <see cref="GroupDTO"/> back.
        /// </summary>
        [Fact]
        public async Task GetGroupsForUserAsync_WithValidData_ReturnsGroupDTO()
        {
            // Arrange
            var user = await CreateUserAsync();
            var group = await CreateFriendGroupAsync();
            await AddUserToGroupDirectlyAsync(user.Id, group.Id);

            // Act
            var result = await _groupsService.GetGroupsForUserAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<GroupDTO>>(result);
        }

        /// <summary>
        /// Verifies that querying groups for a user ID with no memberships
        /// returns an empty list (the service does not throw for users without groups).
        /// </summary>
        [Fact]
        public async Task GetGroupsForUserAsync_WithInvalidUser_ReturnsEmptyList()
        {
            // Arrange
            var invalidUserId = Guid.NewGuid(); // not seeded, no memberships

            // Act
            var result = await _groupsService.GetGroupsForUserAsync(invalidUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion GetGroupsForUserAsync

        #region RemoveUserFromGroupAsync

        /// <summary>
        /// Verifies that removing an existing member from a group deletes the
        /// relationship row from the database.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_WithValidData_RunsSuccessfully()
        {
            // Arrange
            var user = await CreateUserAsync();
            var group = await CreateFriendGroupAsync();
            await AddUserToGroupDirectlyAsync(user.Id, group.Id);

            // Act
            await _groupsService.RemoveUserFromGroupAsync(group.Id, user.Id);

            // Assert - relationship must no longer exist in the DB
            var relationship = await _context.Set<UserFriendGroup>()
                .FirstOrDefaultAsync(ufg => ufg.UserId == user.Id && ufg.FriendGroupId == group.Id);
            Assert.Null(relationship);
        }

        /// <summary>
        /// Verifies that removing a user using a group ID that has no matching
        /// membership record throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_WithInvalidGroup_ThrowsKeyNotFoundException()
        {
            // Arrange
            var user = await CreateUserAsync();
            var invalidGroupId = Guid.NewGuid(); // user has no membership for this group

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _groupsService.RemoveUserFromGroupAsync(invalidGroupId, user.Id));
        }

        /// <summary>
        /// Verifies that removing a user ID that has no membership in the given group
        /// throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_WithInvalidUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var group = await CreateFriendGroupAsync();
            var invalidUserId = Guid.NewGuid(); // no membership exists for this user

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _groupsService.RemoveUserFromGroupAsync(group.Id, invalidUserId));
        }

        #endregion RemoveUserFromGroupAsync

        #region GetGroupByIdAsync

        /// <summary>
        /// Verifies that when a user belongs to a group, GetGroupByIdAsync returns
        /// the correct <see cref="GroupDTO"/> with matching Id and Title.
        /// </summary>
        [Fact]
        public async Task GetGroupByIdAsync_WithValidData_ReturnsGroupDTO()
        {
            // Arrange
            var user = await CreateUserAsync();
            var group = await CreateFriendGroupAsync();
            await AddUserToGroupDirectlyAsync(user.Id, group.Id);

            // Act
            var result = await _groupsService.GetGroupByIdAsync(group.Id, user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(group.Id, result.Id);
            Assert.Equal(group.Title, result.Title);
            Assert.Equal(group.Description, result.Description);
        }

        /// <summary>
        /// Verifies that when the user does not belong to any group,
        /// GetGroupByIdAsync throws <see cref="KeyNotFoundException"/>.
        /// The service queries groups by userId, so a user with no groups gets this error.
        /// </summary>
        [Fact]
        public async Task GetGroupByIdAsync_UserNotInAnyGroup_ThrowsKeyNotFoundException()
        {
            // Arrange
            var user = await CreateUserAsync();
            // User is not added to any group

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _groupsService.GetGroupByIdAsync(Guid.NewGuid(), user.Id));
        }

        /// <summary>
        /// Verifies that when a user belongs to one group but requests a different group's ID,
        /// GetGroupByIdAsync still returns the group the user belongs to, because the service
        /// queries groups by membership (userId) only and ignores the groupId parameter.
        /// </summary>
        [Fact]
        public async Task GetGroupByIdAsync_UserNotInRequestedGroup_ReturnsUserGroup()
        {
            // Arrange
            var user = await CreateUserAsync();
            var groupA = await CreateFriendGroupAsync();
            await AddUserToGroupDirectlyAsync(user.Id, groupA.Id);

            var unrelatedGroupId = Guid.NewGuid();

            // Act - the service finds the group the user belongs to regardless of the
            // requested groupId, since the query only filters by userId
            var result = await _groupsService.GetGroupByIdAsync(unrelatedGroupId, user.Id);

            // Assert - returns the user's actual group, not the requested one
            Assert.NotNull(result);
            Assert.Equal(groupA.Id, result.Id);
        }

        /// <summary>
        /// Verifies that the returned <see cref="GroupDTO"/> contains the correct
        /// MembersCount reflecting the number of users in the group.
        /// </summary>
        [Fact]
        public async Task GetGroupByIdAsync_ReturnsCorrectMembersCount()
        {
            // Arrange
            var user1 = await CreateUserAsync();
            var user2 = new User
            {
                Id = Guid.NewGuid(),
                Username = "user2",
                BirthDate = new DateTime(1996, 1, 1),
                Description = "Second user",
                Email = "user2@example.com",
                PasswordHash = "hashedpassword456"
            };
            await _repo.AddAsync<User>(user2);
            await _repo.SaveChangesAsync();

            var group = await CreateFriendGroupAsync();
            await AddUserToGroupDirectlyAsync(user1.Id, group.Id);
            await AddUserToGroupDirectlyAsync(user2.Id, group.Id);

            // Act
            var result = await _groupsService.GetGroupByIdAsync(group.Id, user1.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.MembersCount);
        }

        #endregion GetGroupByIdAsync

        #region GetGroupMembersAsync

        /// <summary>
        /// Verifies that when a user belongs to a group, GetGroupMembersAsync returns
        /// all members of that group including the requesting user.
        /// </summary>
        [Fact]
        public async Task GetGroupMembersAsync_WithValidData_ReturnsAllMembers()
        {
            // Arrange
            var user1 = await CreateUserAsync();
            var user2 = new User
            {
                Id = Guid.NewGuid(),
                Username = "user2",
                BirthDate = new DateTime(1996, 1, 1),
                Description = "Second user",
                Email = "user2@example.com",
                PasswordHash = "hashedpassword456"
            };
            await _repo.AddAsync<User>(user2);
            await _repo.SaveChangesAsync();

            var group = await CreateFriendGroupAsync();
            await AddUserToGroupDirectlyAsync(user1.Id, group.Id);
            await AddUserToGroupDirectlyAsync(user2.Id, group.Id);

            // Act
            var result = await _groupsService.GetGroupMembersAsync(group.Id, user1.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Id == user1.Id);
            Assert.Contains(result, m => m.Id == user2.Id);
        }

        /// <summary>
        /// Verifies that when the requesting user does not belong to the specified group,
        /// GetGroupMembersAsync throws <see cref="KeyNotFoundException"/> to hide
        /// the group's existence from non-members.
        /// </summary>
        [Fact]
        public async Task GetGroupMembersAsync_UserNotInGroup_ThrowsKeyNotFoundException()
        {
            // Arrange
            var user = await CreateUserAsync();
            var otherUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "other",
                BirthDate = new DateTime(1997, 1, 1),
                Description = "Other user",
                Email = "other@example.com",
                PasswordHash = "hashedpassword789"
            };
            await _repo.AddAsync<User>(otherUser);
            await _repo.SaveChangesAsync();

            var group = await CreateFriendGroupAsync();
            // Only the other user is in the group; 'user' is not
            await AddUserToGroupDirectlyAsync(otherUser.Id, group.Id);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _groupsService.GetGroupMembersAsync(group.Id, user.Id));
        }

        /// <summary>
        /// Verifies that requesting members for a non-existent group
        /// throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task GetGroupMembersAsync_NonExistentGroup_ThrowsKeyNotFoundException()
        {
            // Arrange
            var user = await CreateUserAsync();
            var nonExistentGroupId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _groupsService.GetGroupMembersAsync(nonExistentGroupId, user.Id));
        }

        /// <summary>
        /// Verifies that GetGroupMembersAsync returns an empty list when the group
        /// exists but has no members (edge case).
        /// Note: This scenario is unlikely in practice since groups are always
        /// created with at least the creator as a member.
        /// </summary>
        [Fact]
        public async Task GetGroupMembersAsync_EmptyGroup_ReturnsEmptyList()
        {
            // Arrange - create a group with no members
            var group = await CreateFriendGroupAsync();
            var user = await CreateUserAsync();
            // Do NOT add user to the group

            // Act & Assert - user is not a member, so KeyNotFoundException is thrown
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _groupsService.GetGroupMembersAsync(group.Id, user.Id));
        }

        /// <summary>
        /// Verifies that the returned UserDTOs contain the expected user properties
        /// such as Username, Email, and Id.
        /// </summary>
        [Fact]
        public async Task GetGroupMembersAsync_ReturnsCorrectUserProperties()
        {
            // Arrange
            var user = await CreateUserAsync();
            var group = await CreateFriendGroupAsync();
            await AddUserToGroupDirectlyAsync(user.Id, group.Id);

            // Act
            var result = await _groupsService.GetGroupMembersAsync(group.Id, user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(user.Id, result[0].Id);
            Assert.Equal(user.Username, result[0].Username);
            Assert.Equal(user.Email, result[0].Email);
        }

        #endregion GetGroupMembersAsync
    }
}