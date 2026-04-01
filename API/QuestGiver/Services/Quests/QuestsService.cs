using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using OpenAI.Chat;
using QuestGiver.Data.Common;
using QuestGiver.Data.Constants;
using QuestGiver.Data.Models;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using QuestGiver.Services.Groups;
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
            if(neededCount <= 0)
                return;

            await SetIsGeneratingQuestsAsync(groupId, true);

            try
            {
                var group = await GetFriendGroupWithUsersAsync(groupId);
                var questModels = AssignQuestsToUsers(group.Users, group.LastUserId, neededCount, groupId);

                var prompt = BuildQuestGenerationPrompt(questModels, neededCount);

                var response = await _chatClient.CompleteChatAsync(prompt);
                var content = response.Value.Content[0].Text;

                // Deserialize generated content
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                GeneratedQuestDTO[]? deserialized = JsonSerializer.Deserialize<GeneratedQuestDTO[]>(content, options);

                if (deserialized == null)
                    throw new ArgumentNullException("Could not generate quests.");

                // Map the generated quests while managing the scheduled dates
                await _repo.AddRangeAsync<Quest>(MapDeserializedGeneratedQuests(deserialized, groupId));
                await _repo.SaveChangesAsync();
            }
            catch
            {
                await SetIsGeneratingQuestsAsync(groupId, false);
                throw;
            }
        }

        #region Helpers

        /// <summary>
        /// Maps an array of generated quest DTOs into Quest entities and assigns scheduling metadata.
        /// </summary>
        /// <param name="deserialized">
        /// The array of generated quest DTOs that have been deserialized from an external source.
        /// </param>
        /// <param name="groupId">
        /// The unique identifier of the friend group to which the quests will belong.
        /// </param>
        /// <returns>
        /// An array of <see cref="Quest"/> entities with assigned scheduled dates and group association.
        /// </returns>
        /// <remarks>
        /// Each quest is scheduled sequentially starting from today (UTC/local system date depending on environment),
        /// with each subsequent quest assigned to the next day.
        /// We assume deserialized is not empty.
        /// </remarks>
        private Quest[] MapDeserializedGeneratedQuests(GeneratedQuestDTO[] deserialized, Guid groupId)
        {
            DateTime scheduledDate = DateTime.UtcNow.Date;
            Quest[] quests = _mapper.Map<Quest[]>(deserialized);
            for (int i = 0; i < deserialized.Length; i++)
            {
                quests[i].ScheduledDate = scheduledDate.AddDays(i+1);
                quests[i].FriendGroupId = groupId;
            }

            return quests;
        }

        /// <summary>
        /// Retrieves a friend group along with its associated users and determines the last user assigned a quest.
        /// </summary>
        /// <param name="groupId">
        /// The unique identifier of the friend group to retrieve.
        /// </param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item>
        /// <description>The <see cref="FriendGroup"/> including its quests.</description>
        /// </item>
        /// <item>
        /// <description>An array of <see cref="User"/> entities that belong to the group.</description>
        /// </item>
        /// <item>
        /// <description>
        /// The ID of the last user who was assigned a quest, or the first user in the group if no quests exist.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the friend group does not exist or when no users are found in the group.
        /// </exception>
        /// <remarks>
        /// The last user is determined based on the most recently scheduled quest (by <c>ScheduledDate</c>).
        /// If the group has no quests, the first user in the retrieved users array is used as a fallback.
        /// </remarks>
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

        /// <summary>
        /// Assigns a sequence of quests to users in a round-robin fashion, starting after the last assigned user.
        /// </summary>
        /// <param name="users">
        /// The array of users participating in the friend group.
        /// </param>
        /// <param name="lastUserId">
        /// The ID of the last user who was assigned a quest. Assignment will continue from the next user.
        /// </param>
        /// <param name="count">
        /// The number of quest assignments to generate.
        /// </param>
        /// <param name="groupId">
        /// The unique identifier of the friend group for which the quests are being generated.
        /// </param>
        /// <returns>
        /// A list of <see cref="GenerateQuestDTO"/> objects, each representing a quest assigned to a user.
        /// </returns>
        /// <remarks>
        /// Users are assigned quests in a circular (round-robin) order.
        /// If the end of the users array is reached, assignment continues from the beginning.
        /// The assignment always starts from the user immediately following <paramref name="lastUserId"/>.
        /// </remarks>
        private List<GenerateQuestDTO> AssignQuestsToUsers(User[] users, Guid lastUserId, int count, Guid groupId)
        {
            var index = Array.FindIndex(users, u => u.Id == lastUserId);
            var models = new List<GenerateQuestDTO>();

            // Loop through users and assign generate quests
            for (int i = 0; i < count; i++)
            {
                index = (index + 1) % users.Length;
                var user = users[index];
                var model = _mapper.Map<GenerateQuestDTO>(user);
                model.UserId = user.Id;
                model.FriendGroupId = groupId;
                models.Add(model);
            }

            return models;
        }

        /// <summary>
        /// Builds a prompt for the AI model to generate personalized quests for a group of users.
        /// </summary>
        /// <param name="users">
        /// A list of users (as <see cref="GenerateQuestDTO"/>) for whom quests should be generated.
        /// Each entry contains user-specific data used to personalize the quests.
        /// </param>
        /// <param name="neededCount">
        /// The number of quests the AI should generate.
        /// </param>
        /// <returns>
        /// A formatted string prompt instructing the AI to generate quests in a strict JSON structure.
        /// </returns>
        /// <remarks>
        /// The prompt includes:
        /// - Clear generation rules (fun, realistic, varied, one-day scope)
        /// - Personalization instructions based on user descriptions
        /// - A strict JSON output format to ensure consistent deserialization
        /// - Serialized user data embedded directly into the prompt
        ///
        /// The AI is expected to return ONLY valid JSON matching the specified schema.
        /// Any deviation (extra text, invalid JSON) may cause deserialization to fail.
        /// </remarks>
        private string BuildQuestGenerationPrompt(List<GenerateQuestDTO> users, int neededCount)
        {
            return $@"
            You are generating interesting daily challenges for a group of friends.

            Generate {neededCount} personalized quests by taking into account the specific user's description.

            Each quest must:
            - Be assigned to a specific user
            - Be fun, social, realistic ( compared to their age and interests ) and not cringe
            - Be doable in one day
            - Vary in type (physical, creative, social, etc.)
            - Try to respect the user's wishes ( if they have any ) from their description

            Return ONLY valid JSON in this format:

            [
              {{
                ""userId"": ""GUID"",
                ""title"": ""short title"", ( max length 50 )
                ""description"": ""detailed description"", ( max length 400 )
                ""rewardPoints"": 0-100
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
            // Include navigational properties to then validate if the user trying to access the quest belongs to the friend group
            Quest? model = await _repo.AllReadonly<Quest>()
                .Include(x => x.FriendGroup)
                .ThenInclude(x => x.UserFriendGroups)
                .SingleOrDefaultAsync(x => x.FriendGroupId == groupId && x.ScheduledDate.Date == DateTime.UtcNow.Date);

            FriendGroup? group = await _repo.AllReadonly<FriendGroup>().FirstOrDefaultAsync(x => x.Id == groupId);

            // Start generating quests if the queue is empty
            if (model == null && group?.IsGeneratingQuests == false)
            {
                await GenerateQuestsForGroupAsync(groupId, CalculateNeededQuestsCount(groupId));
                throw new KeyNotFoundException("No scheduled quest was found");
            }

            // If the user does not belong => UnauthorizedAccessException
            if (!model.FriendGroup.UserFriendGroups.Any(x => x.UserId == userId))
                throw new UnauthorizedAccessException("User does not belong to this friend group.");

            // When loaded ( viewed by a user ) set to in progress if not completed yet
            if (model.Status == QuestStatusType.New && model.DateCompleted == null)
                model.Status = QuestStatusType.InProgress;

            _repo.Update<Quest>(model);
            await _repo.SaveChangesAsync();

            return _mapper.Map<QuestDTO>(model);
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
                .FirstOrDefaultAsync(q => q.Id == questId && q.UserId == userId);

            if(quest == null)
                throw new KeyNotFoundException("Quest not found.");

            if (quest.UserId != userId)
                throw new UnauthorizedAccessException("User is not assigned to this quest.");

            quest.DateCompleted = DateTime.UtcNow;
            quest.Status = QuestStatusType.Completed;

            // Attribute the reward points to the user
            var user = await _repo.All<User>()
                .FirstAsync(u => u.Id == userId); // We assume the user exists because we find the quest with his user id

            // TODO: Eventually move this logic into a users service
            user.ExperiencePoints += quest.RewardPoints;
            if(user.ExperiencePoints >= user.NextLevelExperience)
            {
                user.Level++;
                user.ExperiencePoints -= user.NextLevelExperience;
                user.NextLevelExperience = (int)(user.NextLevelExperience * 1.25);
            }

            _repo.Update<User>(user);
            await _repo.SaveChangesAsync();

            try
            {
                // Has it's own SaveChangesAsync if it is completed successfully
                await GenerateQuestsForGroupAsync(quest.FriendGroupId, CalculateNeededQuestsCount(quest.FriendGroupId));
            }
            catch
            {
                // Retry once, if it fails again we move on
                await GenerateQuestsForGroupAsync(quest.FriendGroupId, CalculateNeededQuestsCount(quest.FriendGroupId));
            }

            return _mapper.Map<QuestDTO>(quest);
        }

        /// <inheritdoc />
        public async Task SetIsGeneratingQuestsAsync(Guid groupId, bool isGeneratingQuests)
        {
            FriendGroup? group = await _repo.AllReadonly<FriendGroup>().FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                throw new KeyNotFoundException("No group with specified id was found");

            group.IsGeneratingQuests = isGeneratingQuests;

            _repo.Update(group);
            await _repo.SaveChangesAsync();
        }
    }
}
