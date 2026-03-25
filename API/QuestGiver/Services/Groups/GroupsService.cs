using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;

namespace QuestGiver.Services.Groups
{
    /// <inheritdoc />
    public class GroupsService : IGroupsService
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;

        /// <summary>
        /// DI constructor for GroupsService.
        /// </summary>
        /// <param name="repo">Repository for accessing the db.</param>
        /// <param name="mapper">Mapper</param>
        public GroupsService(IRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task AddUserToGroupAsync(Guid groupId, Guid userId)
        {
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
            FriendGroup group = _mapper.Map<FriendGroup>(model);

            await _repo.AddAsync<FriendGroup>(group);
            await AddUserToGroupAsync(group.Id, userId); // Add the creator to the group

            await _repo.SaveChangesAsync();

            return _mapper.Map<GroupDTO>(group);
        }

        /// <inheritdoc/>
        public async Task<GroupDTO> GetGroupsForUserAsync(Guid userId)
        {
            FriendGroup? group = await _repo.All<FriendGroup>().
                Include(g => g.UserFriendGroups).
                FirstOrDefaultAsync(g => g.UserFriendGroups.Any(ufg => ufg.UserId == userId));

            if(group == null)
                throw new ArgumentException("Invalid userId or no groups found for the user.");

            return _mapper.Map<GroupDTO>(group);
        }

        /// <inheritdoc />
        public async Task RemoveUserFromGroupAsync(Guid groupId, Guid userId)
        {
            UserFriendGroup? relationship = await _repo.All<UserFriendGroup>().
                FirstOrDefaultAsync(x => x.UserId == userId && x.FriendGroupId == groupId);

            if (relationship == null)
                throw new ArgumentException("Invalid userId or friend groupId");

            await _repo.DeleteAsync<UserFriendGroup>(relationship);
            await _repo.SaveChangesAsync();
        }
    }
}
