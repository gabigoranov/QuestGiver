using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using OpenAI.Chat;
using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using System.Diagnostics;

namespace QuestGiver.Services.Quests
{
    /// <inheritdoc />
    public class QuestsService : IQuestsService
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ChatClient _chatClient;

        /// <summary>
        /// Handles DI.
        /// </summary>
        /// <param name="repo">the db repo.</param>
        /// <param name="mapper">Automapper</param>
        /// <param name="configuration">The app settings configuration.</param>
        /// <param name="aiClient">The open ai client</param>
        public QuestsService(IRepository repo, IMapper mapper, IConfiguration configuration, OpenAIClient aiClient)
        {
            _repo = repo;
            _mapper = mapper;
            _configuration = configuration;


            var apiKeysSection = _configuration.GetSection("APIKeys");
            _chatClient = aiClient.GetChatClient("gpt-5-nano");
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
        private async Task GenerateQuestsForGroupAsync(Guid groupId, int neededCount)
        {
            var response = await _chatClient.CompleteChatAsync("Explain JWT in one sentence.");
            Debug.WriteLine(response.Value.Content[0].Text);
        }

        /// <inheritdoc />
        public async Task<QuestDTO> GetCurrentQuestForGroupAsync(Guid groupId, Guid userId)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc />
        public async Task<QuestDTO> CreateQuestAsync(Guid groupId, Guid userId, CreateQuestDTO questCreateDTO)
        {
            await GenerateQuestsForGroupAsync(groupId, 3);

            // Check if the quest queue already has a quest for that date
            var existingQuest = await _repo.All<Quest>()
                .Where(q => q.FriendGroupId == groupId && q.ScheduledDate.Date == questCreateDTO.ScheduledDate.Date)
                .FirstOrDefaultAsync();

            if(existingQuest != null) throw new ArgumentException("Friend group already has a quest scheduled for that date.");

            questCreateDTO.UserId = userId;
            questCreateDTO.FriendGroupId = groupId;

            var quest = _mapper.Map<Quest>(questCreateDTO);
            await _repo.AddAsync<Quest>(quest);
            await _repo.SaveChangesAsync();

            return _mapper.Map<QuestDTO>(quest);    
        }

        /// <inheritdoc/>
        public async Task<QuestDTO> CompleteQuestAsync(Guid questId, Guid userId)
        {
            Quest? quest = await _repo.All<Quest>()
                .FirstOrDefaultAsync(q => q.Id == questId);

            if(quest == null)
                throw new KeyNotFoundException("Quest not found.");

            if(quest.UserId != userId)
                throw new UnauthorizedAccessException("User is not assigned to this quest.");

            quest.DateCompleted = DateTime.UtcNow;

            // TODO: Handle the generation of the next quest to fill te queue

            await _repo.SaveChangesAsync();

            return _mapper.Map<QuestDTO>(quest);
        }
    }
}
