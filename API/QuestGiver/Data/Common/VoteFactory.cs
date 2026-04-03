using AutoMapper;
using QuestGiver.Data.Models;
using QuestGiver.Models.Receive;

namespace QuestGiver.Data.Common
{
    public static class VoteFactory
    {
        public static Vote Create(CreateVoteDTO dto, IMapper mapper)
        {
            return dto.Discriminator switch
            {
                VoteType.SkipVote => mapper.Map<SkipVote>(dto),
                VoteType.CompletionVote => mapper.Map<CompletionVote>(dto),
                _ => throw new ArgumentException("Invalid vote type")
            };
        }
    }
}
