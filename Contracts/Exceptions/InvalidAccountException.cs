using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
