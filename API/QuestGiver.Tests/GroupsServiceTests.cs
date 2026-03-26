using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuestGiver.Data;
using QuestGiver.Data.Common;
using QuestGiver.Data.Constants;
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

        /// <summary>
        /// Initializes a new instance of <see cref="GroupsServiceTests"/> with fresh in-memory DB.
        /// Each test gets a unique database to ensure isolation.
        /// </summary>
        public GroupsServiceTests()
        {
            // Use a unique in-memory database per test to ensure test isolation
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

        #region CreateGroupAsync Tests

        /// <summary>
        /// Tests that CreateGroupAsync successfully creates a group, adds the creator to it,
        /// and triggers the initial quest creation.
        /// </summary>
        [Fact]
        public async Task CreateGroupAsync_ShouldCreateGroupAndReturnGroupDTO()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createGroupDto = new CreateGroupDTO
            {
                Title = "Test Group",
                Description = "Test Description"
            };

            // Setup mock to return a DTO when CreateQuestAsync is called
            _mockQuestsService.Setup(x => x.CreateQuestAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CreateQuestDTO>()))
                .ReturnsAsync((Guid gid, Guid uid, CreateQuestDTO dto) =>
                    new QuestDTO { Id = Guid.NewGuid(), Title = dto.Title });

            // Act
            var result = await _groupsService.CreateGroupAsync(createGroupDto, userId);

            // Assert - Verify group was created in database
            var groupInDb = await _context.FriendGroups.FirstOrDefaultAsync();
            Assert.NotNull(groupInDb);
            Assert.Equal(createGroupDto.Title, groupInDb.Title);
            Assert.Equal(createGroupDto.Description, groupInDb.Description);
            Assert.Equal(userId, groupInDb.UserFriendGroups.Single().UserId);

            // Assert - Verify creator was added to the group (UserFriendGroup join table)
            var userGroup = await _context.UserFriendGroups.FirstOrDefaultAsync();
            Assert.NotNull(userGroup);
            Assert.Equal(userId, userGroup.UserId);
            Assert.Equal(groupInDb.Id, userGroup.FriendGroupId);

            // Assert - Verify returned DTO matches input
            Assert.Equal(createGroupDto.Title, result.Title);
            Assert.Equal(createGroupDto.Description, result.Description);

            // Assert - Verify initial quest was created for the group
            _mockQuestsService.Verify(x => x.CreateQuestAsync(groupInDb.Id, userId, It.IsAny<CreateQuestDTO>()), Times.Once);
        }

        /// <summary>
        /// Tests that CreateGroupAsync correctly maps all properties from DTO to entity.
        /// </summary>
        [Fact]
        public async Task CreateGroupAsync_WithValidData_ShouldMapAllPropertiesCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createGroupDto = new CreateGroupDTO
            {
                Title = "Maximum Length Title Exactly 30",
                Description = "A longer description that tests the mapping of the description field properly."
            };

            _mockQuestsService.Setup(x => x.CreateQuestAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CreateQuestDTO>()))
                .ReturnsAsync(new QuestDTO { Id = Guid.NewGuid(), Title = "Quest" });

            // Act
            var result = await _groupsService.CreateGroupAsync(createGroupDto, userId);

            // Assert
            var groupInDb = await _context.FriendGroups.FirstOrDefaultAsync();
            Assert.NotNull(groupInDb);
            Assert.Equal(createGroupDto.Title, groupInDb.Title);
            Assert.Equal(createGroupDto.Description, groupInDb.Description);
            Assert.Equal(createGroupDto.Title, result.Title);
            Assert.Equal(createGroupDto.Description, result.Description);
        }

        /// <summary>
        /// Tests that CreateGroupAsync sets the DateCreated property on the group entity.
        /// </summary>
        [Fact]
        public async Task CreateGroupAsync_ShouldSetDateCreated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createGroupDto = new CreateGroupDTO
            {
                Title = "Test Group",
                Description = "Test Description"
            };

            _mockQuestsService.Setup(x => x.CreateQuestAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CreateQuestDTO>()))
                .ReturnsAsync(new QuestDTO { Id = Guid.NewGuid(), Title = "Quest" });

            var beforeCreate = DateTime.UtcNow;

            // Act
            await _groupsService.CreateGroupAsync(createGroupDto, userId);

            var afterCreate = DateTime.UtcNow;

            // Assert
            var groupInDb = await _context.FriendGroups.FirstOrDefaultAsync();
            Assert.NotNull(groupInDb);
            Assert.InRange(groupInDb.DateCreated, beforeCreate, afterCreate);
        }

        /// <summary>
        /// Tests that CreateGroupAsync passes correct InitialAddFriendsQuest data to the quests service.
        /// </summary>
        [Fact]
        public async Task CreateGroupAsync_ShouldCreateInitialQuestWithCorrectData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var createGroupDto = new CreateGroupDTO
            {
                Title = "Test Group",
                Description = "Test Description"
            };

            // Capture the CreateQuestDTO passed to the mock
            CreateQuestDTO capturedDto = null;
            Guid capturedGroupId = Guid.Empty;
            Guid capturedUserId = Guid.Empty;

            _mockQuestsService.Setup(x => x.CreateQuestAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CreateQuestDTO>()))
                .Callback<Guid, Guid, CreateQuestDTO>((gid, uid, dto) =>
                {
                    capturedGroupId = gid;
                    capturedUserId = uid;
                    capturedDto = dto;
                })
                .ReturnsAsync(new QuestDTO { Id = Guid.NewGuid(), Title = "Quest" });

            // Act
            var result = await _groupsService.CreateGroupAsync(createGroupDto, userId);

            // Assert
            Assert.NotNull(capturedDto);
            Assert.Equal(result.Id, capturedGroupId);
            Assert.Equal(userId, capturedUserId);
            Assert.Equal("Add Friends", capturedDto.Title);
            Assert.Equal(100, capturedDto.RewardPoints);
        }

        /// <summary>
        /// Tests that CreateGroupAsync handles multiple group creations correctly.
        /// </summary>
        [Fact]
        public async Task CreateGroupAsync_WithMultipleGroups_ShouldCreateAllGroups()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var group1Dto = new CreateGroupDTO { Title = "Group 1", Description = "Description 1" };
            var group2Dto = new CreateGroupDTO { Title = "Group 2", Description = "Description 2" };

            _mockQuestsService.Setup(x => x.CreateQuestAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CreateQuestDTO>()))
                .ReturnsAsync(new QuestDTO { Id = Guid.NewGuid(), Title = "Quest" });

            // Act
            var result1 = await _groupsService.CreateGroupAsync(group1Dto, userId);
            var result2 = await _groupsService.CreateGroupAsync(group2Dto, userId);

            // Assert
            var groupsInDb = await _context.FriendGroups.ToListAsync();
            Assert.Equal(2, groupsInDb.Count);
            Assert.Contains(groupsInDb, g => g.Title == "Group 1");
            Assert.Contains(groupsInDb, g => g.Title == "Group 2");
            Assert.NotEqual(result1.Id, result2.Id);
        }

        #endregion

        #region AddUserToGroupAsync Tests

        /// <summary>
        /// Tests that AddUserToGroupAsync successfully adds a user to an existing group.
        /// </summary>
        [Fact]
        public async Task AddUserToGroupAsync_ShouldAddUserToGroup()
        {
            // Arrange
            var group = new FriendGroup { Title = "Group1", Description = "Test Description" };
            await _context.FriendGroups.AddAsync(group);
            await _context.SaveChangesAsync();

            var userId = Guid.NewGuid();

            // Act
            await _groupsService.AddUserToGroupAsync(group.Id, userId);

            // Assert
            var userGroup = await _context.UserFriendGroups.FirstOrDefaultAsync();
            Assert.NotNull(userGroup);
            Assert.Equal(userId, userGroup.UserId);
            Assert.Equal(group.Id, userGroup.FriendGroupId);
        }

        /// <summary>
        /// Tests that AddUserToGroupAsync can add multiple users to the same group.
        /// </summary>
        [Fact]
        public async Task AddUserToGroupAsync_WithMultipleUsers_ShouldAddAllUsers()
        {
            // Arrange
            var group = new FriendGroup { Title = "Group1", Description = "Test Description" };
            await _context.FriendGroups.AddAsync(group);
            await _context.SaveChangesAsync();

            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var userId3 = Guid.NewGuid();

            // Act
            await _groupsService.AddUserToGroupAsync(group.Id, userId1);
            await _groupsService.AddUserToGroupAsync(group.Id, userId2);
            await _groupsService.AddUserToGroupAsync(group.Id, userId3);

            // Assert
            var userGroups = await _context.UserFriendGroups.ToListAsync();
            Assert.Equal(3, userGroups.Count);
            Assert.Contains(userGroups, ug => ug.UserId == userId1);
            Assert.Contains(userGroups, ug => ug.UserId == userId2);
            Assert.Contains(userGroups, ug => ug.UserId == userId3);
        }

        /// <summary>
        /// Tests that AddUserToGroupAsync can add the same user to multiple groups.
        /// </summary>
        [Fact]
        public async Task AddUserToGroupAsync_WithMultipleGroups_ShouldAddUserToAllGroups()
        {
            // Arrange
            var group1 = new FriendGroup { Title = "Group1", Description = "Test Description" };
            var group2 = new FriendGroup { Title = "Group2", Description = "Test Description" };
            await _context.FriendGroups.AddRangeAsync(group1, group2);
            await _context.SaveChangesAsync();

            var userId = Guid.NewGuid();

            // Act
            await _groupsService.AddUserToGroupAsync(group1.Id, userId);
            await _groupsService.AddUserToGroupAsync(group2.Id, userId);

            // Assert
            var userGroups = await _context.UserFriendGroups.ToListAsync();
            Assert.Equal(2, userGroups.Count);
            Assert.Contains(userGroups, ug => ug.FriendGroupId == group1.Id);
            Assert.Contains(userGroups, ug => ug.FriendGroupId == group2.Id);
        }

        /// <summary>
        /// Tests that AddUserToGroupAsync allows adding a user who is already in another group
        /// (users can be in multiple groups).
        /// </summary>
        [Fact]
        public async Task AddUserToGroupAsync_WithUserAlreadyInAnotherGroup_ShouldSucceed()
        {
            // Arrange
            var group1 = new FriendGroup { Title = "Group1", Description = "Test Description" };
            var group2 = new FriendGroup { Title = "Group2", Description = "Test Description" };
            await _context.FriendGroups.AddRangeAsync(group1, group2);
            await _context.SaveChangesAsync();

            var userId = Guid.NewGuid();
            await _groupsService.AddUserToGroupAsync(group1.Id, userId);

            // Act & Assert - Should not throw, user can be in multiple groups
            await _groupsService.AddUserToGroupAsync(group2.Id, userId);

            var userGroups = await _context.UserFriendGroups.ToListAsync();
            Assert.Equal(2, userGroups.Count);
        }

        #endregion

        #region GetGroupsForUserAsync Tests

        /// <summary>
        /// Tests that GetGroupsForUserAsync returns the correct group for a user.
        /// </summary>
        [Fact]
        public async Task GetGroupsForUserAsync_ShouldReturnGroupsForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var group = new FriendGroup { Title = "Group1", Description = "Test Description" };
            await _context.FriendGroups.AddAsync(group);
            await _context.SaveChangesAsync();

            await _context.UserFriendGroups.AddAsync(new UserFriendGroup
            {
                UserId = userId,
                FriendGroupId = group.Id
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _groupsService.GetGroupsForUserAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(group.Id, result.Id);
            Assert.Equal(group.Title, result.Title);
            Assert.Equal(group.Description, result.Description);
        }

        /// <summary>
        /// Tests that GetGroupsForUserAsync throws ArgumentException when user has no groups.
        /// </summary>
        [Fact]
        public async Task GetGroupsForUserAsync_ShouldThrow_WhenUserHasNoGroups()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _groupsService.GetGroupsForUserAsync(userId));
            Assert.Equal("Invalid userId or no groups found for the user.", ex.Message);
        }

        /// <summary>
        /// Tests that GetGroupsForUserAsync throws when the user exists in DB but has no group memberships.
        /// </summary>
        [Fact]
        public async Task GetGroupsForUserAsync_ShouldThrow_WhenUserExistsButHasNoGroups()
        {
            // Arrange
            var userId = Guid.NewGuid();
            
            // Create a user in the database (but no group memberships)
            await _context.Users.AddAsync(new User 
            { 
                Id = userId, 
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Description = "Test user"
            });
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _groupsService.GetGroupsForUserAsync(userId));
            Assert.Equal("Invalid userId or no groups found for the user.", ex.Message);
        }

        /// <summary>
        /// Tests that GetGroupsForUserAsync returns only the groups for the specified user,
        /// not groups belonging to other users.
        /// </summary>
        [Fact]
        public async Task GetGroupsForUserAsync_WithMultipleUsers_ShouldReturnOnlyCorrectUsersGroups()
        {
            // Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            
            var group1 = new FriendGroup { Title = "Group1", Description = "Test Description" };
            var group2 = new FriendGroup { Title = "Group2", Description = "Test Description" };
            await _context.FriendGroups.AddRangeAsync(group1, group2);
            await _context.SaveChangesAsync();

            await _context.UserFriendGroups.AddAsync(new UserFriendGroup
            {
                UserId = userId1,
                FriendGroupId = group1.Id
            });
            await _context.UserFriendGroups.AddAsync(new UserFriendGroup
            {
                UserId = userId2,
                FriendGroupId = group2.Id
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _groupsService.GetGroupsForUserAsync(userId1);

            // Assert - Should only return group1, not group2
            Assert.NotNull(result);
            Assert.Equal(group1.Id, result.Id);
            Assert.Equal("Group1", result.Title);
        }

        /// <summary>
        /// Tests that GetGroupsForUserAsync handles a user with multiple groups correctly.
        /// Note: Current implementation returns the first group found.
        /// </summary>
        [Fact]
        public async Task GetGroupsForUserAsync_WithUserInMultipleGroups_ShouldReturnFirstGroup()
        {
            // Arrange
            var userId = Guid.NewGuid();
            
            var group1 = new FriendGroup { Title = "Group1", Description = "Test Description" };
            var group2 = new FriendGroup { Title = "Group2", Description = "Test Description" };
            await _context.FriendGroups.AddRangeAsync(group1, group2);
            await _context.SaveChangesAsync();

            await _context.UserFriendGroups.AddAsync(new UserFriendGroup
            {
                UserId = userId,
                FriendGroupId = group1.Id
            });
            await _context.UserFriendGroups.AddAsync(new UserFriendGroup
            {
                UserId = userId,
                FriendGroupId = group2.Id
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _groupsService.GetGroupsForUserAsync(userId);

            // Assert - Should return one of the groups (implementation returns first found)
            Assert.NotNull(result);
            Assert.True(result.Id == group1.Id || result.Id == group2.Id);
        }

        #endregion

        #region RemoveUserFromGroupAsync Tests

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync successfully removes a user from a group.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_ShouldRemoveUserFromGroup()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var group = new FriendGroup { Title = "Group1", Description = "Test Description" };
            await _context.FriendGroups.AddAsync(group);
            await _context.SaveChangesAsync();

            var userGroup = new UserFriendGroup
            {
                UserId = userId,
                FriendGroupId = group.Id
            };
            await _context.UserFriendGroups.AddAsync(userGroup);
            await _context.SaveChangesAsync();

            // Act
            await _groupsService.RemoveUserFromGroupAsync(group.Id, userId);

            // Assert
            var removed = await _context.UserFriendGroups.FirstOrDefaultAsync();
            Assert.Null(removed);
        }

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync throws ArgumentException when user is not in the group.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_ShouldThrow_WhenUserNotInGroup()
        {
            // Arrange
            var group = new FriendGroup { Title = "Group1", Description = "Test Description" };
            await _context.FriendGroups.AddAsync(group);
            await _context.SaveChangesAsync();

            var userId = Guid.NewGuid();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _groupsService.RemoveUserFromGroupAsync(group.Id, userId));
            Assert.Equal("Invalid userId or friend groupId", ex.Message);
        }

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync throws ArgumentException when group does not exist.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_ShouldThrow_WhenGroupDoesNotExist()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _groupsService.RemoveUserFromGroupAsync(groupId, userId));
            Assert.Equal("Invalid userId or friend groupId", ex.Message);
        }

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync only removes the specified user and leaves other users intact.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_WithMultipleUsers_ShouldRemoveOnlySpecifiedUser()
        {
            // Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var group = new FriendGroup { Title = "Group1", Description = "Test Description" };
            await _context.FriendGroups.AddAsync(group);
            await _context.SaveChangesAsync();

            await _context.UserFriendGroups.AddAsync(new UserFriendGroup
            {
                UserId = userId1,
                FriendGroupId = group.Id
            });
            await _context.UserFriendGroups.AddAsync(new UserFriendGroup
            {
                UserId = userId2,
                FriendGroupId = group.Id
            });
            await _context.SaveChangesAsync();

            // Act
            await _groupsService.RemoveUserFromGroupAsync(group.Id, userId1);

            // Assert - userId1 should be removed, userId2 should remain
            var remainingUserGroups = await _context.UserFriendGroups.ToListAsync();
            Assert.Single(remainingUserGroups);
            Assert.Equal(userId2, remainingUserGroups[0].UserId);
        }

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync allows removing a user from one group
        /// while they remain in other groups.
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_WithUserInMultipleGroups_ShouldRemoveFromOnlyOneGroup()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var group1 = new FriendGroup { Title = "Group1", Description = "Test Description" };
            var group2 = new FriendGroup { Title = "Group2", Description = "Test Description" };
            await _context.FriendGroups.AddRangeAsync(group1, group2);
            await _context.SaveChangesAsync();

            await _context.UserFriendGroups.AddAsync(new UserFriendGroup
            {
                UserId = userId,
                FriendGroupId = group1.Id
            });
            await _context.UserFriendGroups.AddAsync(new UserFriendGroup
            {
                UserId = userId,
                FriendGroupId = group2.Id
            });
            await _context.SaveChangesAsync();

            // Act
            await _groupsService.RemoveUserFromGroupAsync(group1.Id, userId);

            // Assert - User should be removed from group1 but remain in group2
            var remainingUserGroups = await _context.UserFriendGroups.ToListAsync();
            Assert.Single(remainingUserGroups);
            Assert.Equal(group2.Id, remainingUserGroups[0].FriendGroupId);
        }

        /// <summary>
        /// Tests that RemoveUserFromGroupAsync succeeds when removing the last user from a group
        /// (group can exist without members).
        /// </summary>
        [Fact]
        public async Task RemoveUserFromGroupAsync_WithLastUserInGroup_ShouldSucceed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var group = new FriendGroup { Title = "Group1", Description = "Test Description" };
            await _context.FriendGroups.AddAsync(group);
            await _context.SaveChangesAsync();

            await _context.UserFriendGroups.AddAsync(new UserFriendGroup
            {
                UserId = userId,
                FriendGroupId = group.Id
            });
            await _context.SaveChangesAsync();

            // Act & Assert - Should not throw even though this is the last user
            await _groupsService.RemoveUserFromGroupAsync(group.Id, userId);

            var remainingUserGroups = await _context.UserFriendGroups.ToListAsync();
            Assert.Empty(remainingUserGroups);

            // Group should still exist
            var groupInDb = await _context.FriendGroups.FirstOrDefaultAsync();
            Assert.NotNull(groupInDb);
        }

        #endregion
    }
}
