namespace Domain.Exceptions
{
    public class InvalidAccountException : Exception
    {
        public InvalidAccountException() : base()
        {

        }
        public InvalidAccountException(string message) : base(message)
        {

        }
        public InvalidAccountException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
