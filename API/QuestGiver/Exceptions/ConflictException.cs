namespace QuestGiver.Exceptions
{
    /// <summary>
    /// Used to express that an entity can not be modified
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
