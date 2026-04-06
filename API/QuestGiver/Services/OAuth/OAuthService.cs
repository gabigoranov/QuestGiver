using AutoMapper;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using QuestGiver.Models.Send;
using QuestGiver.Services.Tokens;
using System;

namespace QuestGiver.Services.OAuth
{
    /// <inheritdoc />
    public class OAuthService : IOAuthService
    {
        private readonly IRepository _repo;
        private readonly ITokensService _tokensService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public OAuthService(IRepository repo, ITokensService tokensService, IConfiguration config, IMapper mapper)
        {
            _repo = repo;
            _tokensService = tokensService;
            _config = config;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task<AuthResponse> LoginWithGoogleAsync(string idToken)
        {
            // 1. Validate the Google ID token
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["OAuthSettings:ClientId"] }
            });

            // 2. Lookup user in your database
            var user = await _repo.All<User>().FirstOrDefaultAsync(u => u.Email == payload.Email);
            if (user == null)
            {
                // TODO: EXTRACT INTO AUTH SERVICE

                // 3. Create a new user if it doesn't exist
                user = new User
                {
                    Email = payload.Email,
                    Username = payload.Name,
                    Provider = AuthProviderType.Google,
                    AvatarUrl = payload.Picture
                };
                await _repo.AddAsync<User>(user);
                await _repo.SaveChangesAsync();
            }

            // 4. Issue your own JWT for your app
            var token = await _tokensService.CreateTokenAsync(user.Id);

            // If user profile has a description, then they have filled out their interests data
            return new AuthResponse(
                _mapper.Map<UserDTO>(user), 
                _mapper.Map<TokenDTO>(token), 
                user.Description != null);
        }
    }
}
