using AutoMapper;

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
            this.CreateMap<Data.Models.FriendGroup, Models.Send.GroupDTO>().ReverseMap();
            this.CreateMap<Data.Models.FriendGroup, Models.Receive.CreateGroupDTO>().ReverseMap();
        }
    }
}
