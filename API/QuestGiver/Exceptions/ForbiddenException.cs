namespace QuestGiver.Exceptions
{
    /// <summary>
    /// Used as an exception to express that a user does not have access to a certain entity, despite being authenticated
    /// </summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
