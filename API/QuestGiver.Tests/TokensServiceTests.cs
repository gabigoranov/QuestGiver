using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuestGiver.Data;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using QuestGiver.Models.Common;
using QuestGiver.Models.Send;
using QuestGiver.Services.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace QuestGiver.Tests
{
    /// <summary>
    /// Unit tests for <see cref="TokensService"/> using an in-memory database.
    /// Covers creation, refresh, invalidation, token generation and edge cases.
    /// </summary>
    public class TokensServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly TokensService _tokensService;

        public TokensServiceTests()
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

            var configData = new Dictionary<string, string?>
            {
                { "Jwt:Key", "THIS_IS_A_SUPER_SECRET_KEY_123456789" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:AccessTokenExpirationMinutes", "60" },
                { "Jwt:RefreshTokenExpirationDays", "7" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            _tokensService = new TokensService(_configuration, _repo, _mapper);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Helpers

        private async Task<User> CreateUserAsync()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                BirthDate = new DateTime(1995, 1, 1),
                Description = "Test user description",
                Email = "testuser@example.com",
                PasswordHash = "hashedpassword123"
            };

            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();
            return user;
        }

        private async Task<Token> CreateTokenAsync(Guid userId, bool revoked = false, bool expired = false, DateTime? revokedAt = null)
        {
            var token = new Token
            {
                UserId = userId,
                RefreshToken = Guid.NewGuid().ToString(),
                AccessToken = "old-access-token",
                IsRevoked = revoked,
                RevokedAt = revokedAt,
                ExpirationDateTime = expired
                    ? DateTime.UtcNow.AddDays(-1)
                    : DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            };

            await _repo.AddAsync(token);
            await _repo.SaveChangesAsync();
            return token;
        }

        #endregion Helpers

        #region CreateTokenAsync

        [Fact]
        public async Task CreateTokenAsync_WithValidData_CreatesTokenAndReturnsTokenDTO()
        {
            var user = await CreateUserAsync();

            var before = DateTime.UtcNow;
            var result = await _tokensService.CreateTokenAsync(user.Id);
            var after = DateTime.UtcNow;

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
            Assert.InRange(result.ExpirationDateTime, before.AddDays(6), after.AddDays(8));

            var tokenInDb = await _context.Set<Token>().FirstOrDefaultAsync(t => t.Id == result.Id);
            Assert.NotNull(tokenInDb);
            Assert.Equal(user.Id, tokenInDb!.UserId);
            Assert.Equal(result.RefreshToken, tokenInDb.RefreshToken);
            Assert.False(tokenInDb.IsRevoked);
            Assert.InRange(tokenInDb.CreatedAt, before.AddMinutes(-1), after.AddMinutes(1));
        }

        [Fact]
        public async Task CreateTokenAsync_WithValidData_PersistsOnlyDTOFieldsToDatabase()
        {
            var user = await CreateUserAsync();

            var result = await _tokensService.CreateTokenAsync(user.Id);

            var tokenInDb = await _context.Set<Token>().FirstAsync(t => t.Id == result.Id);

            Assert.Equal(result.Id, tokenInDb.Id);
            Assert.Equal(result.RefreshToken, tokenInDb.RefreshToken);
            Assert.Equal(result.ExpirationDateTime, tokenInDb.ExpirationDateTime);
        }

        #endregion

        #region GenerateAccessToken

        [Fact]
        public void GenerateAccessToken_WithValidConfig_ReturnsJwtContainingUserIdClaim()
        {
            var userId = Guid.NewGuid();

            var token = _tokensService.GenerateAccessToken(userId);

            Assert.False(string.IsNullOrWhiteSpace(token));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal("TestIssuer", jwt.Issuer);
            Assert.Contains("TestAudience", jwt.Audiences);

            Assert.Contains(jwt.Claims, c =>
                c.Type == ClaimTypes.NameIdentifier &&
                c.Value == userId.ToString());

            Assert.True(jwt.ValidTo > DateTime.UtcNow);
        }

        [Fact]
        public void GenerateAccessToken_WithMissingJwtConfig_ThrowsArgumentException()
        {
            var badConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:RefreshTokenExpirationDays", "7" }
                })
                .Build();

            var service = new TokensService(badConfig, _repo, _mapper);

            Assert.Throws<ArgumentException>(() =>
                service.GenerateAccessToken(Guid.NewGuid()));
        }

        [Fact]
        public void Constructor_WithInvalidRefreshTokenExpirationDays_ThrowsArgumentException()
        {
            var badConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "THIS_IS_A_SUPER_SECRET_KEY_123456789" },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:AccessTokenExpirationMinutes", "60" },
                    { "Jwt:RefreshTokenExpirationDays", "not-a-number" }
                })
                .Build();

            Assert.Throws<ArgumentException>(() => new TokensService(badConfig, _repo, _mapper));
        }

        #endregion

        #region GenerateRefreshToken

        [Fact]
        public void GenerateRefreshToken_ReturnsNonEmptyString()
        {
            var token = _tokensService.GenerateRefreshToken();

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void GenerateRefreshToken_ReturnsDifferentValuesOnConsecutiveCalls()
        {
            var first = _tokensService.GenerateRefreshToken();
            var second = _tokensService.GenerateRefreshToken();

            Assert.NotEqual(first, second);
        }

        #endregion

        #region InvalidateTokenAsync

        [Fact]
        public async Task InvalidateTokenAsync_WithValidToken_RevokesToken()
        {
            var user = await CreateUserAsync();
            var token = await CreateTokenAsync(user.Id);

            await _tokensService.InvalidateTokenAsync(token.RefreshToken);

            var updated = await _context.Set<Token>().FirstAsync(t => t.Id == token.Id);

            Assert.True(updated.IsRevoked);
            Assert.NotNull(updated.RevokedAt);
        }

        [Fact]
        public async Task InvalidateTokenAsync_WithMissingToken_DoesNothing()
        {
            var user = await CreateUserAsync();
            var existingToken = await CreateTokenAsync(user.Id);

            await _tokensService.InvalidateTokenAsync("missing-token");

            var tokenInDb = await _context.Set<Token>().FirstAsync(t => t.Id == existingToken.Id);

            Assert.False(tokenInDb.IsRevoked);
            Assert.Null(tokenInDb.RevokedAt);
        }

        #endregion

        #region RefreshTokenAsync

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsUpdatedTokenDTO()
        {
            var user = await CreateUserAsync();
            var token = await CreateTokenAsync(user.Id);

            var beforeRefresh = token.RefreshToken;
            var beforeExpiry = token.ExpirationDateTime;

            var result = await _tokensService.RefreshTokenAsync(token.RefreshToken);

            Assert.NotNull(result);
            Assert.Equal(token.Id, result.Id);
            Assert.NotEqual(beforeRefresh, result.RefreshToken);
            Assert.True(result.ExpirationDateTime > beforeExpiry);

            var updated = await _context.Set<Token>().FirstAsync(t => t.Id == token.Id);
            Assert.Equal(result.RefreshToken, updated.RefreshToken);
            Assert.Equal(result.ExpirationDateTime, updated.ExpirationDateTime);
            Assert.False(updated.IsRevoked);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithExpiredToken_ThrowsUnauthorizedAccessException()
        {
            var user = await CreateUserAsync();
            var token = await CreateTokenAsync(user.Id, expired: true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _tokensService.RefreshTokenAsync(token.RefreshToken));
        }

        [Fact]
        public async Task RefreshTokenAsync_WithRevokedToken_RevokesAllRelatedTokensAndThrows()
        {
            var user = await CreateUserAsync();

            var revokedToken = await CreateTokenAsync(
                user.Id,
                revoked: true,
                revokedAt: DateTime.UtcNow.AddMinutes(-10));

            var activeToken1 = await CreateTokenAsync(user.Id);
            var activeToken2 = await CreateTokenAsync(user.Id);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _tokensService.RefreshTokenAsync(revokedToken.RefreshToken));

            var tokens = await _context.Set<Token>()
                .Where(t => t.UserId == user.Id)
                .ToListAsync();

            Assert.All(tokens, t => Assert.True(t.IsRevoked));

            var newlyRevokedTokens = tokens.Where(t => t.Id != revokedToken.Id).ToList();
            Assert.All(newlyRevokedTokens, t => Assert.NotNull(t.RevokedAt));
        }

        [Fact]
        public async Task RefreshTokenAsync_WithInvalidToken_ThrowsUnauthorizedAccessException()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _tokensService.RefreshTokenAsync("invalid-token"));
        }

        #endregion

        [Fact]
        public async Task RefreshTokenAsync_WithRevokedToken_RevokesActiveRelatedTokens_AndKeepsAlreadyRevokedTokensRevoked()
        {
            var user = await CreateUserAsync();

            var revokedToken = await CreateTokenAsync(
                user.Id,
                revoked: true,
                revokedAt: DateTime.UtcNow.AddMinutes(-20));

            var alreadyRevokedRelatedToken = await CreateTokenAsync(
                user.Id,
                revoked: true,
                revokedAt: DateTime.UtcNow.AddMinutes(-30));

            var activeRelatedToken = await CreateTokenAsync(user.Id);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _tokensService.RefreshTokenAsync(revokedToken.RefreshToken));

            var tokens = await _context.Set<Token>()
                .Where(t => t.UserId == user.Id)
                .ToListAsync();

            Assert.All(tokens, t => Assert.True(t.IsRevoked));
            Assert.All(tokens, t => Assert.NotNull(t.RevokedAt));

            var revivedToken = await _context.Set<Token>().FirstAsync(t => t.Id == alreadyRevokedRelatedToken.Id);
            var activeToken = await _context.Set<Token>().FirstAsync(t => t.Id == activeRelatedToken.Id);

            Assert.True(revivedToken.IsRevoked);
            Assert.True(activeToken.IsRevoked);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenUserHasMultipleActiveTokens_OnlyTargetTokenRotates()
        {
            var user = await CreateUserAsync();

            var targetToken = await CreateTokenAsync(user.Id);
            var siblingToken = await CreateTokenAsync(user.Id);

            var targetRefreshBefore = targetToken.RefreshToken;
            var siblingRefreshBefore = siblingToken.RefreshToken;
            var siblingAccessBefore = siblingToken.AccessToken;
            var siblingExpiryBefore = siblingToken.ExpirationDateTime;

            var result = await _tokensService.RefreshTokenAsync(targetRefreshBefore);

            var updatedTarget = await _context.Set<Token>().FirstAsync(t => t.Id == targetToken.Id);
            var updatedSibling = await _context.Set<Token>().FirstAsync(t => t.Id == siblingToken.Id);

            Assert.Equal(targetToken.Id, result.Id);
            Assert.NotEqual(targetRefreshBefore, result.RefreshToken);

            Assert.Equal(result.RefreshToken, updatedTarget.RefreshToken);
            Assert.Equal(result.AccessToken, updatedTarget.AccessToken);
            Assert.Equal(result.ExpirationDateTime, updatedTarget.ExpirationDateTime);

            Assert.Equal(siblingRefreshBefore, updatedSibling.RefreshToken);
            Assert.Equal(siblingAccessBefore, updatedSibling.AccessToken);
            Assert.Equal(siblingExpiryBefore, updatedSibling.ExpirationDateTime);
            Assert.False(updatedSibling.IsRevoked);
        }

        [Fact]
        public async Task RefreshTokenAsync_OldRefreshTokenBecomesUnusableAfterRotation()
        {
            var user = await CreateUserAsync();
            var token = await CreateTokenAsync(user.Id);

            var oldRefreshToken = token.RefreshToken;

            var refreshed = await _tokensService.RefreshTokenAsync(oldRefreshToken);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _tokensService.RefreshTokenAsync(oldRefreshToken));

            var updated = await _context.Set<Token>().FirstAsync(t => t.Id == token.Id);

            Assert.Equal(refreshed.RefreshToken, updated.RefreshToken);
            Assert.Equal(refreshed.AccessToken, updated.AccessToken);
            Assert.Equal(refreshed.ExpirationDateTime, updated.ExpirationDateTime);
        }
    }
}