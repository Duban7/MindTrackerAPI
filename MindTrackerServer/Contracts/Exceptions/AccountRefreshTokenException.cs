namespace Domain.Exceptions
{
    public class AccountRefreshTokenException : Exception
    {
        public AccountRefreshTokenException() : base()
        {

        }
        public AccountRefreshTokenException(string message) : base(message)
        {

        }
        public AccountRefreshTokenException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
