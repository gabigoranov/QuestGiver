using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using QuestGiver.Models.Send;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QuestGiver.Services.Tokens
{
    /// <inheritdoc />
    public class TokensService : ITokensService
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// DI constructor for TokensService, which takes in the application configuration to access JWT settings such as the secret key, issuer, audience, and token expiration times. This allows the service to generate and manage tokens based on the configured parameters.
        /// </summary>
        /// <param name="configuration">The appsettings.json configuration representative.</param>
        /// <param name="repo">Repository for accessing the db.</param>
        /// <param name="mapper">Mapper</param>
        public TokensService(IConfiguration configuration, IRepository repo, IMapper mapper)
        {
            _configuration = configuration;
            _repo = repo;
            _mapper = mapper;
        }


        /// <inheritdoc />
        public async Task<TokenDTO> CreateTokenAsync(Guid userId)
        {
            // Generate the access token and refresh token
            string accessToken = GenerateAccessToken(userId);
            string refreshToken = GenerateRefreshToken();

            // Load JWT settings from configuration
            var jwtSettings = _configuration.GetSection("Jwt");

            if (jwtSettings == null ||
                string.IsNullOrEmpty(jwtSettings["RefreshTokenExpirationDays"]) ||
                !int.TryParse(jwtSettings["RefreshTokenExpirationDays"], out int refreshDays))
            {
                throw new ArgumentException("Invalid JWT configuration: RefreshTokenExpirationDays is missing or invalid.");
            }

            Token res = new Token()
            {
                UserId = userId,
                RefreshToken = refreshToken,
                ExpirationDateTime = DateTime.UtcNow.AddDays(refreshDays), // Set the expiration time for the refresh token based on configuration
                AccessToken = accessToken
            };

            Token? old = await _repo.All<Token>().FirstOrDefaultAsync(t => t.UserId == userId);
            if (old != null) await _repo.DeleteAsync<Token>(old.Id);

            await _repo.AddAsync<Token>(res);
            await _repo.SaveChangesAsync();

            return _mapper.Map<TokenDTO>(res);

        }

        /// <inheritdoc />
        public string GenerateAccessToken(Guid userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // TODO: Add more claims as needed, e.g., roles, permissions, etc.
            };

            var jwtSettings = _configuration.GetSection("Jwt");

            if (jwtSettings == null ||
                string.IsNullOrEmpty(jwtSettings["Key"]) ||
                string.IsNullOrEmpty(jwtSettings["Issuer"]) ||
                string.IsNullOrEmpty(jwtSettings["Audience"]) ||
                string.IsNullOrEmpty(jwtSettings["AccessTokenExpirationMinutes"]) ||
                !int.TryParse(jwtSettings["AccessTokenExpirationMinutes"], out int accessMinutes))
            {
                throw new ArgumentException("Invalid JWT configuration: One or more required fields are missing or invalid.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <inheritdoc />
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <inheritdoc />
        public async Task InvalidateTokenAsync(string refreshToken) => await _repo.ExecuteDeleteAsync<Token>(t => t.RefreshToken == refreshToken);

        /// <inheritdoc />
        public async Task<TokenDTO> RefreshTokenAsync(string refreshToken)
        {
            Token? token = await _repo.All<Token>().SingleOrDefaultAsync(x => x.RefreshToken == refreshToken);

            if(token == null || token.IsExpired)
            {
                // Token does not exist or is expired, deny auth
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            token.RefreshToken = GenerateRefreshToken();
            token.AccessToken = GenerateAccessToken(token.UserId);
            
            _repo.Update(token);
            await _repo.SaveChangesAsync();

            return _mapper.Map<TokenDTO>(token);
        }
    }
}