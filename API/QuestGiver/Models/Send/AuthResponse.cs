namespace QuestGiver.Models.Send
{
    /// <summary>
    /// Join DTO for users and tokens during auth.
    /// </summary>
    public class AuthResponse
    {
        public AuthResponse(UserDTO user, TokenDTO token, bool hasInterestsInfo)
        {
            User = user;
            Token = token;
            HasInterestsInfo = hasInterestsInfo;
        }

        public UserDTO User { get; set; }
        public TokenDTO Token { get; set; }

        /// <summary>
        /// If the user has null description, birth date, etc. then they will need to go through a screen to fill that out
        /// </summary>
        public bool HasInterestsInfo { get; set; }
    }
}
