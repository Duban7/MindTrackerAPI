using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class AccountRefreshTokenMatchException : Exception
    {
        public AccountRefreshTokenMatchException() : base()
        {

        }
        public AccountRefreshTokenMatchException(string message) : base(message)
        {

        }
        public AccountRefreshTokenMatchException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
