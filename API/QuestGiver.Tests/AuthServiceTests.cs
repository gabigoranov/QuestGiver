using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuestGiver.Data;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using QuestGiver.Services.Tokens;
using QuestGiver.Services.Users;
using Xunit;

namespace QuestGiver.Tests
{
    /// <summary>
    /// Unit tests for <see cref="AuthService"/> using an in-memory database.
    /// 
    /// These tests cover the main authentication flows:
    /// - user creation
    /// - duplicate email prevention
    /// - login verification
    /// - incorrect password handling
    /// - missing user handling
    /// 
    /// The token service is mocked so the tests focus only on the behavior of
    /// <see cref="AuthService"/> itself.
    /// </summary>
    public class AuthServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<string> _passwordHasher;
        private readonly Mock<ITokensService> _tokensServiceMock;
        private readonly AuthService _authService;

        /// <summary>
        /// Initializes the test fixture with:
        /// - an isolated in-memory database
        /// - repository abstraction over the test context
        /// - AutoMapper configured with the application's mapping profile
        /// - a real password hasher for verifying password logic
        /// - a mocked token service so token generation does not affect these tests
        /// </summary>
        public AuthServiceTests()
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
            _passwordHasher = new PasswordHasher<string>();

            _tokensServiceMock = new Mock<ITokensService>();

            // Default token response used by all successful auth flows.
            _tokensServiceMock
                .Setup(x => x.CreateTokenAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid userId) => new TokenDTO
                {
                    Id = 1,
                    RefreshToken = Guid.NewGuid().ToString(),
                    AccessToken = "test-access-token",
                    ExpirationDateTime = DateTime.UtcNow.AddDays(7)
                });

            _authService = new AuthService(
                _repo,
                _mapper,
                _passwordHasher,
                _tokensServiceMock.Object);
        }

        /// <summary>
        /// Cleans up the in-memory database after each test to ensure isolation.
        /// </summary>
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Helpers

        /// <summary>
        /// Creates and persists a fully valid <see cref="User"/> entity.
        /// 
        /// The <see cref="User.PasswordHash"/> is always populated to satisfy EF Core
        /// required-property validation and to allow login verification tests.
        /// </summary>
        private async Task<User> SeedUserAsync(string email = "test@test.com", string password = "123456")
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = "testuser",
                BirthDate = new DateTime(2000, 1, 1),
                Description = "desc"
            };

            // PasswordHash must be set before saving because the entity requires it.
            user.PasswordHash = _passwordHasher.HashPassword(user.Email, password);

            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Builds a valid <see cref="CreateUserDTO"/> used in user creation tests.
        /// </summary>
        private CreateUserDTO CreateValidCreateDto(string email = "new@test.com")
        {
            return new CreateUserDTO
            {
                Email = email,
                Password = "123456",
                Username = "newuser",
                BirthDate = new DateTime(2000, 1, 1),
                Description = "desc"
            };
        }

        /// <summary>
        /// Builds a valid <see cref="LoginDTO"/> used in login verification tests.
        /// </summary>
        private LoginDTO CreateValidLoginDto(string email = "test@test.com", string password = "123456")
        {
            return new LoginDTO
            {
                Email = email,
                Password = password
            };
        }

        #endregion

        #region CreateUserAsync

        /// <summary>
        /// Verifies that creating a user with valid input:
        /// - persists the user to the database
        /// - returns an <see cref="AuthResponse"/>
        /// - includes both user and token information in the response
        /// - calls the token service exactly once
        /// </summary>
        [Fact]
        public async Task CreateUserAsync_WithValidData_CreatesUserAndReturnsAuthResponse()
        {
            var dto = CreateValidCreateDto();

            var result = await _authService.CreateUserAsync(dto);

            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.NotNull(result.Token);

            var userInDb = await _context.Set<User>().FirstAsync();

            Assert.Equal(dto.Email, userInDb.Email);
            Assert.NotEqual(dto.Password, userInDb.PasswordHash); // hashed

            _tokensServiceMock.Verify(x => x.CreateTokenAsync(userInDb.Id), Times.Once);
        }

        /// <summary>
        /// Verifies that attempting to create a user with an email that already exists
        /// throws <see cref="ArgumentException"/>.
        /// </summary>
        [Fact]
        public async Task CreateUserAsync_WithDuplicateEmail_ThrowsArgumentException()
        {
            await SeedUserAsync("duplicate@test.com");

            var dto = CreateValidCreateDto("duplicate@test.com");

            await Assert.ThrowsAsync<ArgumentException>(
                () => _authService.CreateUserAsync(dto));
        }

        #endregion

        #region VerifyLoginAsync

        /// <summary>
        /// Verifies that logging in with valid credentials:
        /// - returns an <see cref="AuthResponse"/>
        /// - maps the correct user back into the response
        /// - triggers token generation exactly once
        /// </summary>
        [Fact]
        public async Task VerifyLoginAsync_WithValidCredentials_ReturnsAuthResponse()
        {
            var user = await SeedUserAsync();

            var dto = CreateValidLoginDto(user.Email);

            var result = await _authService.VerifyLoginAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(user.Email, result.User.Email);

            _tokensServiceMock.Verify(x => x.CreateTokenAsync(user.Id), Times.Once);
        }

        /// <summary>
        /// Verifies that logging in with the correct email but wrong password
        /// throws <see cref="UnauthorizedAccessException"/>.
        /// </summary>
        [Fact]
        public async Task VerifyLoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
        {
            var user = await SeedUserAsync();

            var dto = CreateValidLoginDto(user.Email, "wrong-password");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.VerifyLoginAsync(dto));
        }

        /// <summary>
        /// Verifies that attempting to log in with an email that does not exist
        /// throws <see cref="ArgumentException"/>.
        /// </summary>
        [Fact]
        public async Task VerifyLoginAsync_WithNonExistingUser_ThrowsArgumentException()
        {
            var dto = CreateValidLoginDto("missing@test.com");

            await Assert.ThrowsAsync<ArgumentException>(
                () => _authService.VerifyLoginAsync(dto));
        }

        #endregion
    }
}