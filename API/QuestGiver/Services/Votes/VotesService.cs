using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using QuestGiver.Exceptions;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;

namespace QuestGiver.Services.Votes
{
    /// <inheritdoc />
    public class VotesService : IVotesService
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;

        /// <summary>
        /// Injects services
        /// </summary>
        /// <param name="repo">The db repository</param>
        /// <param name="mapper">Automapper</param>
        public VotesService(IRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task CreateUserVoteAsync(Guid voteId, Guid userId)
        {
            Vote? vote = await _repo.AllReadonly<Vote>().FirstOrDefaultAsync(x => x.Id == voteId);

            if (vote == null)
                throw new KeyNotFoundException("No vote with specified id was found");

            User? user = await _repo.AllReadonly<User>().FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new KeyNotFoundException("No user with specified id was found");

            // If a vote has already been decided, do not add a new uservote to not mess with history
            if (vote.Decision != null)
                return;

            UserVote res = new UserVote() { 
                UserId = userId,
                VoteId = voteId
            };

            await _repo.AddAsync(res);
            await _repo.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<VoteDTO> CreateVoteAsync(CreateVoteDTO model, Guid userId)
        {
            // we need to verify that there are no other active votes before creating a new one
            var activeVote = await _repo.AllReadonly<Vote>()
                .OrderByDescending(x => x.DateCreated)
                .FirstOrDefaultAsync(x => x.QuestId == model.QuestId && x.Decision != null);

            if (activeVote != null)
                throw new ConflictException("There is an active vote, so a new one can not be created");

            // Verify that the user has access to this vote ( he is chosen for the quest )
            Quest? voteQuest = await _repo.AllReadonly<Quest>()
                .Include(x => x.FriendGroup)
                .ThenInclude(x => x.UserFriendGroups)
                .FirstOrDefaultAsync(x => x.Id == model.QuestId);
            
            if (voteQuest == null)
                throw new KeyNotFoundException("No quest with specified id was found");

            if (voteQuest.UserId != userId)
                throw new ForbiddenException("Only the chosen user for a quest can start a vote");

            // Use a factory to decide whether we need a completion or a skip vote
            Vote res = VoteFactory.Create(model, _mapper); 
            await _repo.AddAsync(res);

            // create UserVotes
            foreach(UserFriendGroup userInGroup in voteQuest.FriendGroup.UserFriendGroups)
            {
                res.UserVotes.Add(new UserVote
                {
                    UserId = userInGroup.UserId,
                    Decision = userInGroup.UserId == userId ? true : null
                });
            }

            res.RecalculateDecision(res.UserVotes.Count);

            await _repo.SaveChangesAsync();

            return _mapper.Map<VoteDTO>(res);
        }

        /// <inheritdoc />
        public async Task DeleteUserVoteAsync(Guid voteId, Guid userId)
        {
            UserVote? userVote = await _repo.All<UserVote>()
                .Include(x => x.Vote)
                    .ThenInclude(v => v.Quest)
                        .ThenInclude(q => q.FriendGroup)
                            .ThenInclude(fg => fg.UserFriendGroups)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.VoteId == voteId);

            if (userVote == null)
                throw new KeyNotFoundException("No UserVote with specified id was found");

            // Remove via tracked entity
            _repo.Delete(userVote);

            // Recalculate after deletion
            int memberCount = userVote.Vote.Quest.FriendGroup.UserFriendGroups.Count;
            userVote.Vote.RecalculateDecision(memberCount);

            await _repo.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<VoteDTO> GetLatestQuestVoteAsync(Guid questId, Guid userId)
        {
            // Order by descending date created, even though at most there can be only 1 active quest
            // however there can be old history votes
            var activeVote = await _repo.AllReadonly<Vote>().Include(x => x.UserVotes).OrderByDescending(x => x.DateCreated).FirstOrDefaultAsync(x => x.QuestId == questId);

            if (activeVote == null) 
                throw new KeyNotFoundException("No vote with specified id was found");

            if (!activeVote.UserVotes.Any(x => x.UserId == userId))
                throw new KeyNotFoundException("No vote with the specified id was found"); // Hide the existence from the user

            // TODO: Right now there is no difference between the completion and skip vote, but in the future we need to handle the differences
            return _mapper.Map<VoteDTO>(activeVote);
        }

        /// <inheritdoc />
        public async Task SubmitIndividualVoteAsync(Guid voteId, Guid userId, bool decision)
        {
            Vote? vote = await _repo.All<Vote>().Include(x => x.UserVotes).SingleOrDefaultAsync(v => v.Id == voteId);

            if (vote == null)
                throw new KeyNotFoundException("No vote with specified id was found");

            // if the user has a decision already, then deny
            if (vote.Decision != null)
                throw new ConflictException("The vote has already been decided");

            // Verify that the user has access to this vote
            UserVote? userVote = vote.UserVotes.FirstOrDefault(x => x.UserId == userId);

            if (userVote == null)
                throw new KeyNotFoundException("No vote with the specified id was found");

            userVote.Decision = decision;

            _repo.Update(userVote);
            vote.RecalculateDecision(vote.UserVotes.Count);

            await _repo.SaveChangesAsync();
        }
    }
}
