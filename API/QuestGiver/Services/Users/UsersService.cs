using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using QuestGiver.Models.Send;

namespace QuestGiver.Services.Users
{
    /// <inheritdoc />
    public class UsersService : IUsersService
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the UsersService class with the specified repository and object mapper.
        /// </summary>
        /// <param name="repo">The repository used for data access operations related to users. Cannot be null.</param>
        /// <param name="mapper">The object mapper used to map between data entities and domain models. Cannot be null.</param>
        public UsersService(IRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task<UserDTO> GetByIdAsync(Guid userId)
        {
            User? user = await _repo.AllReadonly<User>().FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("No user with such id found");

            return _mapper.Map<UserDTO>(user);
        }

        /// <inheritdoc />
        public async Task IncreaseUserXP(Guid userId, int xp)
        {
            // Attribute the reward points to the user
            User? user = await _repo.All<User>()
                .FirstOrDefaultAsync(u => u.Id == userId); // We assume the user exists because we find the quest with his user id

            if (user == null)
                throw new KeyNotFoundException("No user with specified id was found");

            user.ExperiencePoints += xp;
            while (user.ExperiencePoints >= user.NextLevelExperience)
            {
                user.Level++;
                user.ExperiencePoints -= user.NextLevelExperience;
                user.NextLevelExperience = (int)(user.NextLevelExperience * 1.25);
            }

            _repo.Update(user);
            await _repo.SaveChangesAsync();
        }
    }
}
