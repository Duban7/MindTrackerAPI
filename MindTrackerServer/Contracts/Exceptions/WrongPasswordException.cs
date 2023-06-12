namespace Domain.Exceptions
{
    public class WrongPasswordException : Exception
    {
        public WrongPasswordException() : base()
        {

        }
        public WrongPasswordException(string message) : base(message)
        {

        }
        public WrongPasswordException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
