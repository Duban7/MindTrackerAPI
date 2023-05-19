using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
