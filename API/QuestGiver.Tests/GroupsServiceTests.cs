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

            _groupsService = new GroupsService(_repo, _mapper, _mockQuestsService.Object);
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
        /// throws <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task GetGroupsForUserAsync_WithInvalidUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var invalidUserId = Guid.NewGuid(); // not seeded, no memberships

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _groupsService.GetGroupsForUserAsync(invalidUserId));
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
    }
}