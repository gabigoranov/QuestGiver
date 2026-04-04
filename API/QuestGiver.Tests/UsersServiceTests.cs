using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestGiver.Data;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using QuestGiver.Models.Common;
using QuestGiver.Models.Send;
using QuestGiver.Services.Users;
using Xunit;

namespace QuestGiver.Tests
{
    /// <summary>
    /// Unit tests for <see cref="UsersService"/> using an in-memory database.
    ///
    /// These tests cover:
    /// - retrieving a user by ID
    /// - increasing user XP without levelling up
    /// - increasing user XP that triggers a level-up
    /// - multiple consecutive level-ups from a large XP gain
    /// - error handling for missing users
    /// </summary>
    public class UsersServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly UsersService _service;

        /// <summary>
        /// Initializes the test fixture with an isolated in-memory database,
        /// a repository, and AutoMapper configured with the application's profile.
        /// </summary>
        public UsersServiceTests()
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

            _service = new UsersService(_repo, _mapper);
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
        /// Creates and persists a fully valid <see cref="User"/> entity.
        /// </summary>
        private async Task<User> SeedUserAsync(
            Guid? id = null,
            int level = 1,
            int experiencePoints = 0,
            int nextLevelExperience = 100)
        {
            var user = new User
            {
                Id = id ?? Guid.NewGuid(),
                Email = "test@test.com",
                Username = "testuser",
                BirthDate = new DateTime(2000, 1, 1),
                Description = "desc",
                PasswordHash = "hashed-password",
                Level = level,
                ExperiencePoints = experiencePoints,
                NextLevelExperience = nextLevelExperience
            };

            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();

            return user;
        }

        #endregion

        #region GetByIdAsync

        /// <summary>
        /// Verifies that requesting an existing user by ID returns a correctly
        /// mapped <see cref="UserDTO"/>.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsUserDTO()
        {
            var user = await SeedUserAsync();

            var result = await _service.GetByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.Level, result.Level);
            Assert.Equal(user.ExperiencePoints, result.ExperiencePoints);
        }

        /// <summary>
        /// Verifies that requesting a non-existent user ID throws
        /// <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.GetByIdAsync(Guid.NewGuid()));
        }

        #endregion

        #region IncreaseUserXP

        /// <summary>
        /// Verifies that increasing a user's XP by an amount that does not
        /// reach the next level threshold only updates ExperiencePoints.
        /// </summary>
        [Fact]
        public async Task IncreaseUserXP_WithoutLevelUp_IncreasesExperienceOnly()
        {
            var user = await SeedUserAsync(experiencePoints: 10, nextLevelExperience: 100);

            await _service.IncreaseUserXP(user.Id, 30);

            var updated = await _context.Set<User>().FirstAsync(u => u.Id == user.Id);

            Assert.Equal(40, updated.ExperiencePoints);
            Assert.Equal(1, updated.Level);
            Assert.Equal(100, updated.NextLevelExperience);
        }

        /// <summary>
        /// Verifies that when XP reaches exactly the next level threshold,
        /// the user levels up, XP is reduced, and NextLevelExperience scales by 1.25.
        /// </summary>
        [Fact]
        public async Task IncreaseUserXP_ExactThreshold_TriggersLevelUp()
        {
            var user = await SeedUserAsync(experiencePoints: 80, nextLevelExperience: 100);

            await _service.IncreaseUserXP(user.Id, 20);

            var updated = await _context.Set<User>().FirstAsync(u => u.Id == user.Id);

            Assert.Equal(2, updated.Level);
            Assert.Equal(0, updated.ExperiencePoints);      // 80 + 20 = 100, then 100 - 100 = 0
            Assert.Equal(125, updated.NextLevelExperience); // 100 * 1.25
        }

        /// <summary>
        /// Verifies that when XP exceeds the next level threshold, the user levels up
        /// and the overflow carries into the new XP pool.
        /// </summary>
        [Fact]
        public async Task IncreaseUserXP_ExceedsThreshold_TriggersLevelUpWithOverflow()
        {
            var user = await SeedUserAsync(experiencePoints: 80, nextLevelExperience: 100);

            await _service.IncreaseUserXP(user.Id, 50);

            var updated = await _context.Set<User>().FirstAsync(u => u.Id == user.Id);

            Assert.Equal(2, updated.Level);
            Assert.Equal(30, updated.ExperiencePoints);      // 80 + 50 = 130, then 130 - 100 = 30
            Assert.Equal(125, updated.NextLevelExperience);
        }

        /// <summary>
        /// Verifies that a large XP gain can trigger multiple consecutive level-ups
        /// (each level recalculates the threshold).
        /// </summary>
        [Fact]
        public async Task IncreaseUserXP_LargeGain_TriggersMultipleLevelUps()
        {
            var user = await SeedUserAsync(experiencePoints: 90, nextLevelExperience: 100);

            await _service.IncreaseUserXP(user.Id, 500);

            var updated = await _context.Set<User>().FirstAsync(u => u.Id == user.Id);

            // Level 1 → 2: 90 + 500 = 590, 590 >= 100 → level=2, xp=490, next=125
            // Level 2 → 3: 490 >= 125 → level=3, xp=365, next=156 (125*1.25=156.25 → 156)
            // Level 3 → 4: 365 >= 156 → level=4, xp=209, next=195 (156*1.25=195)
            // Level 4 → 5: 209 >= 195 → level=5, xp=14,  next=243 (195*1.25=243.75 → 243)
            Assert.Equal(5, updated.Level);
            Assert.Equal(14, updated.ExperiencePoints);
            Assert.Equal(243, updated.NextLevelExperience);
        }

        /// <summary>
        /// Verifies that increasing XP for a non-existent user throws
        /// <see cref="KeyNotFoundException"/>.
        /// </summary>
        [Fact]
        public async Task IncreaseUserXP_NonExistentUser_ThrowsKeyNotFoundException()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.IncreaseUserXP(Guid.NewGuid(), 50));
        }

        /// <summary>
        /// Verifies that adding zero XP does not change the user's state.
        /// </summary>
        [Fact]
        public async Task IncreaseUserXP_ZeroXp_DoesNotChangeState()
        {
            var user = await SeedUserAsync(experiencePoints: 50, nextLevelExperience: 100);

            await _service.IncreaseUserXP(user.Id, 0);

            var updated = await _context.Set<User>().FirstAsync(u => u.Id == user.Id);

            Assert.Equal(50, updated.ExperiencePoints);
            Assert.Equal(1, updated.Level);
            Assert.Equal(100, updated.NextLevelExperience);
        }

        #endregion
    }
}
