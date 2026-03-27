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
    /// 
    /// This test suite validates the full lifecycle of token handling including:
    /// - Access token generation (JWT correctness and claims)
    /// - Refresh token generation (uniqueness and validity)
    /// - Token persistence in the database
    /// - Token invalidation (revocation behavior)
    /// - Token refresh logic (rotation, expiration, and security constraints)
    /// 
    /// The tests ensure that all critical authentication and authorization edge cases
    /// are handled correctly, including multi-token scenarios and replay attack prevention.
    /// </summary>
    public class TokensServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly TokensService _tokensService;

        /// <summary>
        /// Initializes a new instance of the test class by configuring:
        /// - In-memory database for isolation
        /// - Repository abstraction
        /// - AutoMapper profile
        /// - JWT configuration settings
        /// - Instance of <see cref="TokensService"/>
        /// </summary>
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

        /// <summary>
        /// Cleans up the in-memory database after each test execution
        /// to ensure full isolation between test cases.
        /// </summary>
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Helpers

        /// <summary>
        /// Creates and persists a fully valid <see cref="User"/> entity,
        /// satisfying all required constraints (including PasswordHash).
        /// 
        /// This method ensures test stability by avoiding EF Core nullability violations.
        /// </summary>
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

        /// <summary>
        /// Creates and persists a <see cref="Token"/> entity with configurable state.
        /// 
        /// Parameters allow simulation of:
        /// - Revoked tokens
        /// - Expired tokens
        /// - Custom revocation timestamps
        /// 
        /// This helper is essential for testing edge cases in refresh and invalidation flows.
        /// </summary>
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

        /// <summary>
        /// Verifies that creating a token for a valid user:
        /// - Returns a valid <see cref="TokenDTO"/>
        /// - Persists a corresponding <see cref="Token"/> in the database
        /// - Initializes fields such as RefreshToken, CreatedAt, and ExpirationDateTime correctly
        /// </summary>
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

        /// <summary>
        /// Verifies that only fields exposed in <see cref="TokenDTO"/> are persisted and mapped correctly,
        /// ensuring no unintended data leakage or mismatch between entity and DTO.
        /// </summary>
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

        /// <summary>
        /// Verifies that a valid JWT access token:
        /// - Is generated successfully
        /// - Contains correct issuer and audience
        /// - Includes a NameIdentifier claim with the correct user ID
        /// - Has a valid expiration timestamp in the future
        /// </summary>
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

        /// <summary>
        /// Verifies that missing critical JWT configuration values
        /// results in an <see cref="ArgumentException"/> during token generation.
        /// </summary>
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

        /// <summary>
        /// Verifies that invalid configuration values (non-numeric expiration)
        /// throw an <see cref="ArgumentException"/> during service construction.
        /// </summary>
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

        /// <summary>
        /// Verifies that generating a refresh token returns a non-empty string.
        /// </summary>
        [Fact]
        public void GenerateRefreshToken_ReturnsNonEmptyString()
        {
            var token = _tokensService.GenerateRefreshToken();

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        /// <summary>
        /// Verifies that consecutive calls to generate refresh tokens produce unique values,
        /// preventing token collisions.
        /// </summary>
        [Fact]
        public void GenerateRefreshToken_ReturnsDifferentValuesOnConsecutiveCalls()
        {
            var first = _tokensService.GenerateRefreshToken();
            var second = _tokensService.GenerateRefreshToken();

            Assert.NotEqual(first, second);
        }

        #endregion

        #region InvalidateTokenAsync

        /// <summary>
        /// Verifies that a valid refresh token is properly revoked:
        /// - IsRevoked is set to true
        /// - RevokedAt timestamp is assigned
        /// </summary>
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

        /// <summary>
        /// Verifies that attempting to invalidate a non-existing token
        /// does not affect any existing tokens.
        /// </summary>
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

        /// <summary>
        /// Verifies that refreshing a valid token:
        /// - Rotates the refresh token
        /// - Extends expiration
        /// - Updates database state accordingly
        /// </summary>
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

        /// <summary>
        /// Verifies that attempting to refresh an expired token
        /// throws <see cref="UnauthorizedAccessException"/>.
        /// </summary>
        [Fact]
        public async Task RefreshTokenAsync_WithExpiredToken_ThrowsUnauthorizedAccessException()
        {
            var user = await CreateUserAsync();
            var token = await CreateTokenAsync(user.Id, expired: true);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _tokensService.RefreshTokenAsync(token.RefreshToken));
        }

        /// <summary>
        /// Verifies that refreshing a revoked token:
        /// - Revokes all related tokens for the same user
        /// - Throws <see cref="UnauthorizedAccessException"/>
        /// 
        /// This protects against reuse of compromised tokens.
        /// </summary>
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

        /// <summary>
        /// Verifies that providing an invalid (non-existing) refresh token
        /// results in <see cref="UnauthorizedAccessException"/>.
        /// </summary>
        [Fact]
        public async Task RefreshTokenAsync_WithInvalidToken_ThrowsUnauthorizedAccessException()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _tokensService.RefreshTokenAsync("invalid-token"));
        }

        #endregion

        /// <summary>
        /// Verifies that when refreshing a revoked token:
        /// - Already revoked tokens remain revoked
        /// - Active tokens become revoked
        /// - No token is unintentionally reactivated
        /// </summary>
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

        /// <summary>
        /// Verifies that when multiple active tokens exist for a user:
        /// - Only the targeted token is rotated during refresh
        /// - Other tokens remain unchanged and valid
        /// 
        /// This ensures proper isolation between sessions/devices.
        /// </summary>
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

        /// <summary>
        /// Verifies that once a refresh token has been used:
        /// - It becomes invalid (cannot be reused)
        /// - Any subsequent attempt to use it throws <see cref="UnauthorizedAccessException"/>
        /// 
        /// This prevents replay attacks and enforces one-time token usage.
        /// </summary>
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