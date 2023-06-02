namespace Domain.Exceptions
{
    public class MoodMarkNotFoundException : Exception
    {
        public MoodMarkNotFoundException() : base()
        {

        }
        public MoodMarkNotFoundException(string message) : base(message)
        {

        }
        public MoodMarkNotFoundException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
