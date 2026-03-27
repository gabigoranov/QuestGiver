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
    public class AuthServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<string> _passwordHasher;
        private readonly Mock<ITokensService> _tokensServiceMock;
        private readonly AuthService _authService;

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

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Helpers

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

            user.PasswordHash = _passwordHasher.HashPassword(user.Email, password);

            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();

            return user;
        }

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

        [Fact]
        public async Task VerifyLoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
        {
            var user = await SeedUserAsync();

            var dto = CreateValidLoginDto(user.Email, "wrong-password");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.VerifyLoginAsync(dto));
        }

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