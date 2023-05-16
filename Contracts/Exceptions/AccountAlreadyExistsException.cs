using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
