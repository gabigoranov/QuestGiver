using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using OpenAI.Chat;
using QuestGiver.Data.Common;
using QuestGiver.Data.Constants;
using QuestGiver.Data.Models;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using System.Diagnostics;
using System.Text.Json;

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
        private int CalculateNeededQuestsCount(Guid groupId)
        {
            return QuestQueeueConstants.DesiredQueeueSize - GetFriendGroupQuestQueuee(groupId).Count;
        }

        /// <summary>
        /// Handles generating neededCount quests for the friend group and saving them to the database.
        /// </summary>
        private async Task GenerateQuestsForGroupAsync(Guid groupId, int neededCount)
        {
            var group = await GetFriendGroupWithUsersAsync(groupId);
            var questModels = AssignQuestsToUsers(group.Users, group.LastUserId, neededCount, groupId);

            var prompt = BuildQuestGenerationPrompt(questModels, neededCount);

            var response = await _chatClient.CompleteChatAsync(prompt);
            var content = response.Value.Content[0].Text;

            // TODO: save quests to DB & Implement logic for calculating the scheduled date based on the last quest's scheduled date
        }

        #region Helpers

        private async Task<(FriendGroup Group, User[] Users, Guid LastUserId)> GetFriendGroupWithUsersAsync(Guid groupId)
        {
            var group = await _repo.AllReadonly<FriendGroup>()
                .Include(g => g.Quests)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                throw new ArgumentException("No friend group found.");

            var users = await _repo.AllReadonly<User>()
                .Where(u => u.UserFriendGroups.Any(fg => fg.FriendGroupId == groupId))
                .ToArrayAsync();

            if (users.Length == 0)
                throw new ArgumentException("No users found in the friend group.");

            var lastUserId = group.Quests
                .OrderBy(q => q.ScheduledDate)
                .LastOrDefault()?.UserId ?? users[0].Id;

            return (group, users, lastUserId);
        }

        private List<GenerateQuestModel> AssignQuestsToUsers(User[] users, Guid lastUserId, int count, Guid groupId)
        {
            var index = Array.FindIndex(users, u => u.Id == lastUserId);
            var models = new List<GenerateQuestModel>();

            for (int i = 0; i < count; i++)
            {
                index = (index + 1) % users.Length;
                var user = users[index];
                var model = _mapper.Map<GenerateQuestModel>(user);
                model.UserId = user.Id;
                model.FriendGroupId = groupId;
                models.Add(model);
            }

            return models;
        }

        private string BuildQuestGenerationPrompt(List<GenerateQuestModel> users, int neededCount)
        {
            return $@"
            You are generating fun daily challenges for a group of friends.

            Generate {neededCount} quests.

            Each quest must:
            - Be assigned to a specific user
            - Be fun, social, and realistic
            - Be doable in one day
            - Vary in type (physical, creative, social, etc.)

            Return ONLY valid JSON in this format:

            [
              {{
                ""userId"": ""GUID"",
                ""title"": ""short title"",
                ""description"": ""detailed description"",
                ""difficulty"": 1-5
              }}
            ]

            Users:
            {JsonSerializer.Serialize(users)}
            ";
        }

        #endregion

        /// <summary>
        /// Returns all the uncompleted quests for the friend group ordered by scheduled date ascending. This is the "quest queue" that the group will work through.
        /// </summary>
        /// <param name="groupId">The group Id.</param>
        /// <returns>List of quest.</returns>
        private List<Quest> GetFriendGroupQuestQueuee(Guid groupId)
        {
            return _repo.AllReadonly<Quest>()
                .Where(q => q.FriendGroupId == groupId && q.DateCompleted == null)
                .OrderBy(q => q.ScheduledDate).ToList();
        }

        /// <inheritdoc />
        public async Task<QuestDTO> GetCurrentQuestForGroupAsync(Guid groupId, Guid userId)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc />
        public async Task<QuestDTO> CreateQuestAsync(Guid groupId, Guid userId, CreateQuestDTO questCreateDTO)
        {
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

            if (quest.UserId != userId)
                throw new UnauthorizedAccessException("User is not assigned to this quest.");

            quest.DateCompleted = DateTime.UtcNow;

            await GenerateQuestsForGroupAsync(quest.FriendGroupId, CalculateNeededQuestsCount(quest.FriendGroupId));

            await _repo.SaveChangesAsync();

            return _mapper.Map<QuestDTO>(quest);
        }
    }
}
