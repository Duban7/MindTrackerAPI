using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class AccountIdMatchException : Exception
    {
        public AccountIdMatchException() : base()
        {

        }
        public AccountIdMatchException(string message) : base(message)
        {

        }
        public AccountIdMatchException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
