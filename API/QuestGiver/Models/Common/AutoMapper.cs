using AutoMapper;
using QuestGiver.Models.Send;

namespace QuestGiver.Models.Common
{
    /// <summary>
    /// AutoMapper profile for mapping between data models and view models in the QuestGiver application.
    /// </summary>
    public class AutoMapper : Profile
    {
        /// <summary>
        /// Creates maps.
        /// </summary>
        public AutoMapper()
        {
            this.CreateMap<Data.Models.User, Models.Send.UserDTO>().ReverseMap();
            this.CreateMap<Data.Models.User, Models.Receive.CreateUserDTO>().ReverseMap();
            this.CreateMap<Data.Models.Token, Models.Send.TokenDTO>().ReverseMap();
            this.CreateMap<Data.Models.FriendGroup, Models.Send.GroupDTO>()
                .ForMember(x => x.MembersCount, cd => cd.MapFrom(map => map.UserFriendGroups.Count()))
                .ForMember(x => x.CurrentQuestStatus, cd => cd.MapFrom(src => src.Quests
                    .Where(q => q.ScheduledDate.Date == DateTime.UtcNow.Date)
                    .Select(q => q.Status)
                    .FirstOrDefault())).ReverseMap();
            this.CreateMap<Data.Models.FriendGroup, Models.Receive.CreateGroupDTO>().ReverseMap();
            this.CreateMap<Data.Models.Quest, Models.Send.QuestDTO>().ReverseMap();
            this.CreateMap<Data.Models.Quest, Models.Receive.CreateQuestDTO>().ReverseMap();
            this.CreateMap<Data.Models.Quest, Models.Receive.GeneratedQuestDTO>().ReverseMap();
            this.CreateMap<Data.Models.User, GenerateQuestDTO>()
                .ForMember(x => x.UserBirthDate, cd => cd.MapFrom(map => map.BirthDate))
                .ForMember(x => x.UserDescription, cd => cd.MapFrom(map => map.Description));

        }
    }
}
