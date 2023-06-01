namespace Domain.Exceptions
{
    public class AccountAlreadyExistsException : Exception
    {
        public AccountAlreadyExistsException() : base()
        {

        }
        public AccountAlreadyExistsException(string message) : base(message)
        {

        }
        public AccountAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
