using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestGiver.Data.Common;
using QuestGiver.Data.Constants;
using QuestGiver.Data.Models;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using QuestGiver.Services.Quests;
using QuestGiver.Services.Votes;

namespace QuestGiver.Services.Groups
{
    /// <inheritdoc />
    public class GroupsService : IGroupsService
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IQuestsService _questsService;
        private readonly IVotesService _votesService;

        /// <summary>
        /// DI constructor for GroupsService.
        /// </summary>
        /// <param name="repo">Repository for accessing the db.</param>
        /// <param name="mapper">Automapper.</param>
        /// <param name="questsService">The quests service.</param>
        /// <param name="votesService">Votes service</param>
        public GroupsService(IRepository repo, IMapper mapper, IQuestsService questsService, IVotesService votesService)
        {
            _repo = repo;
            _mapper = mapper;
            _questsService = questsService;
            _votesService = votesService;
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

            // Add the new user to the group's most recent active vote
            Vote? activeVote = await _repo.AllReadonly<Vote>().OrderByDescending(x => x.DateCreated).FirstOrDefaultAsync(x => x.Quest.FriendGroupId == groupId && x.Decision == null);
            if(activeVote != null)
            {
                await _votesService.CreateUserVoteAsync(activeVote.Id, userId);
            }

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

        /// <inheritdoc />
        public async Task<List<UserDTO>> GetGroupMembersAsync(Guid groupId, Guid userId)
        {
            List<User> groupMembers = await _repo.AllReadonly<User>()
                .Include(x => x.UserFriendGroups)
                .Where(x => x.UserFriendGroups.Any(fg => fg.FriendGroupId == groupId))
                .ToListAsync();

            if (!groupMembers.Any(x => x.Id == userId))
                throw new KeyNotFoundException("No group with specified id was found"); // hide the existence

            return _mapper.Map<List<UserDTO>>(groupMembers);
        }

        /// <inheritdoc/>
        public async Task<List<GroupDTO>> GetGroupsForUserAsync(Guid userId)
        {
            List<FriendGroup> groups = await _repo.All<FriendGroup>()
                .Include(g => g.UserFriendGroups)
                .Include(g => g.Quests)
                .Where(g => g.UserFriendGroups.Any(ufg => ufg.UserId == userId)).ToListAsync();

            if (groups.Count == 0)
                return [];

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

            // Remove the user from the groups active vote
            Vote? activeVote = await _repo.AllReadonly<Vote>().OrderByDescending(x => x.DateCreated).FirstOrDefaultAsync(x => x.Quest.FriendGroupId == groupId && x.Decision == null);
            if (activeVote != null)
            {
                await _votesService.DeleteUserVoteAsync(activeVote.Id, userId);
            }
        }

        
    }
}
