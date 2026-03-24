using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using QuestGiver.Services.Tokens;

namespace QuestGiver.Services.Users
{
    /// <inheritdoc />
    public class AuthService : IAuthService
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<string> _passwordHasher;
        private readonly ITokensService _tokensService;

        /// <summary>
        /// Initializes a new instance of the AuthService class using the specified repository.
        /// </summary>
        /// <param name="repo">The repository used for data access operations required by the authentication service. Cannot be null.</param>
        /// <param name="mapper">The AutoMapper.</param>
        /// <param name="passwordHasher">Password hasher used to securely store passwords.</param>
        /// <param name="tokensService">The token service used for handling JWT.</param>
        public AuthService(IRepository repo, IMapper mapper, IPasswordHasher<string> passwordHasher, ITokensService tokensService)
        {
            _repo = repo;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _tokensService = tokensService;
        }

        /// <inheritdoc />
        public async Task<AuthResponse> CreateUserAsync(CreateUserDTO model)
        {
            if (_repo.AllReadonly<User>().Any(x => x.Email == model.Email)) 
            {
                // User with the same email already exists
                throw new ArgumentException("A user with the same email already exists.");
            }

            User user = _mapper.Map<User>(model);
            user.PasswordHash = _passwordHasher.HashPassword(user.Email, model.Password); // Hash the password using the email as the salt

            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();

            UserDTO userMap = _mapper.Map<UserDTO>(user);
            TokenDTO token = await _tokensService.CreateTokenAsync(user.Id);

            return new AuthResponse(userMap, token);
        }

        /// <inheritdoc />
        public async Task<AuthResponse> VerifyLoginAsync(LoginDTO model)
        {
            User? user = await _repo.AllReadonly<User>().SingleOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
                throw new ArgumentException("No user with such email exists.");

            var result = _passwordHasher.VerifyHashedPassword(user.Email, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Supplied password does not match user profile.");

            UserDTO userMap = _mapper.Map<UserDTO>(user);
            TokenDTO token = await _tokensService.CreateTokenAsync(user.Id);

            return new AuthResponse(userMap, token);
        }
    }
}
