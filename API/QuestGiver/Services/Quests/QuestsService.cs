using AutoMapper;
using QuestGiver.Data.Common;
using QuestGiver.Models.Send;

namespace QuestGiver.Services.Quests
{
    /// <inheritdoc />
    public class QuestsService : IQuestsService
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;

        /// <summary>
        /// Handles DI.
        /// </summary>
        /// <param name="repo">the db repo.</param>
        /// <param name="mapper">Automapper</param>
        public QuestsService(IRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        /// <summary>
        /// Calculates whether the friend group is running out of quests and how many they need to generate to fill the queeue up to the desired level.
        /// </summary>
        /// <param name="groupId">The id of the group.</param>
        /// <returns>The count of needed quests</returns>
        private Task<int> CalculateNeededQuestsCount(Guid groupId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles generating neededCount quests for the friend group and saving them to the database.
        /// </summary>
        /// <param name="groupId">The id of the friend group.</param>
        /// <param name="neededCount">How many new quests should be generated.</param>
        /// <returns>Nothing.</returns>
        private Task GenerateQuestsForGroupAsync(Guid groupId, int neededCount)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<QuestDTO> GetCurrentQuestForGroupAsync(Guid groupId, Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
