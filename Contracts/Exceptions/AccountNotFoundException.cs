namespace Domain.Exceptions
{
    public class AccountNotFoundException : Exception
    {
        public AccountNotFoundException() : base()
        {

        }
        public AccountNotFoundException(string message) : base(message)
        {

        }
        public AccountNotFoundException(string message, Exception innerException) : base(message, innerException)
        {

        }

    }
}
