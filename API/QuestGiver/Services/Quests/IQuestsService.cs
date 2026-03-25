using QuestGiver.Models.Send;

namespace QuestGiver.Services.Quests
{
    /// <summary>
    /// Defines the contract for quest-related operations within the application.
    /// </summary>
    public interface IQuestsService
    {
        /// <summary>
        /// Retrieves the active quest for a friend group and controls the update of the quest queeue for the group.
        /// </summary>
        /// <param name="groupId">The friend group id.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>The QuestDTO.</returns>
        public Task<QuestDTO> GetCurrentQuestForGroupAsync(Guid groupId, Guid userId);
    }
}
