namespace QuestGiver.Models.Send
{
    /// <summary>
    /// Join DTO for users and tokens during auth.
    /// </summary>
    public class AuthResponse
    {
        public AuthResponse(UserDTO user, TokenDTO token)
        {
            User = user;
            Token = token;
        }

        public UserDTO User { get; set; }
        public TokenDTO Token { get; set; }
    }
}
