using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestGiver.Data.Common;
using QuestGiver.Data.Constants;
using QuestGiver.Data.Models;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using QuestGiver.Services.Quests;

namespace QuestGiver.Services.Groups
{
    /// <inheritdoc />
    public class GroupsService : IGroupsService
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IQuestsService _questsService;

        /// <summary>
        /// DI constructor for GroupsService.
        /// </summary>
        /// <param name="repo">Repository for accessing the db.</param>
        /// <param name="mapper">Automapper.</param>
        /// <param name="questsService">The quests service.</param>
        public GroupsService(IRepository repo, IMapper mapper, IQuestsService questsService)
        {
            _repo = repo;
            _mapper = mapper;
            _questsService = questsService;
        }

        /// <inheritdoc />
        public async Task AddUserToGroupAsync(Guid groupId, Guid userId)
        {
            if(await _repo.AllReadonly<User>().FirstOrDefaultAsync(u => u.Id == userId) == null)
                throw new KeyNotFoundException("Invalid userId");

            if(await _repo.AllReadonly<FriendGroup>().FirstOrDefaultAsync(g => g.Id == groupId) == null)
                throw new KeyNotFoundException("Invalid groupId");

            if (await _repo.AllReadonly<UserFriendGroup>().AnyAsync(ufg => ufg.UserId == userId && ufg.FriendGroupId == groupId))
                throw new InvalidOperationException("User is already in the group.");

            UserFriendGroup userFriendGroup = new UserFriendGroup
            {
                FriendGroupId = groupId,
                UserId = userId
            };

            await _repo.AddAsync<UserFriendGroup>(userFriendGroup); // Register the new many-to-many relationship in the database
            await _repo.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<GroupDTO> CreateGroupAsync(CreateGroupDTO model, Guid userId)
        {
            if (await _repo.AllReadonly<User>().FirstOrDefaultAsync(u => u.Id == userId) == null)
                throw new KeyNotFoundException("Invalid userId");

            FriendGroup group = _mapper.Map<FriendGroup>(model);

            group.UserFriendGroups = new List<UserFriendGroup>
            {
                new UserFriendGroup
                {
                    UserId = userId
                }
            };

            await _repo.AddAsync(group); // Add the creator to the group

            // assign the initial quest to the group (add friends quest)
            await _questsService.CreateQuestAsync(group.Id, userId, 
                _mapper.Map<CreateQuestDTO>(new InitialAddFriendsQuest(userId, group.Id)));

            await _repo.SaveChangesAsync();

            return _mapper.Map<GroupDTO>(group);
        }

        /// <inheritdoc />
        public async Task<GroupDTO> GetGroupByIdAsync(Guid groupId, Guid userId)
        {
            FriendGroup? group = await _repo.All<FriendGroup>()
                .Include(g => g.UserFriendGroups)
                .Include(g => g.Quests)
                .FirstOrDefaultAsync(g => g.UserFriendGroups.Any(ufg => ufg.UserId == userId));

            if (group == null)
                throw new KeyNotFoundException("Invalid userId or no groups found for the user.");

            return _mapper.Map<GroupDTO>(group);
        }

        /// <inheritdoc/>
        public async Task<List<GroupDTO>> GetGroupsForUserAsync(Guid userId)
        {
            List<FriendGroup> groups = await _repo.All<FriendGroup>()
                .Include(g => g.UserFriendGroups)
                .Include(g => g.Quests)
                .Where(g => g.UserFriendGroups.Any(ufg => ufg.UserId == userId)).ToListAsync();

            if(groups.Count == 0)
                throw new KeyNotFoundException("Invalid userId or no groups found for the user.");

            return _mapper.Map<List<GroupDTO>>(groups);
        }

        /// <inheritdoc />
        public async Task RemoveUserFromGroupAsync(Guid groupId, Guid userId)
        {
            UserFriendGroup? relationship = await _repo.All<UserFriendGroup>().
                FirstOrDefaultAsync(x => x.UserId == userId && x.FriendGroupId == groupId);

            if (relationship == null)
                throw new KeyNotFoundException("Invalid userId or friend groupId");

            _repo.Delete<UserFriendGroup>(relationship);
            await _repo.SaveChangesAsync();
        }
    }
}
