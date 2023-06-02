namespace Domain.Exceptions
{
    public class DeleteMoodMarkException : Exception
    {
        public DeleteMoodMarkException() : base()
        {

        }
        public DeleteMoodMarkException(string message) : base(message)
        {

        }
        public DeleteMoodMarkException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
