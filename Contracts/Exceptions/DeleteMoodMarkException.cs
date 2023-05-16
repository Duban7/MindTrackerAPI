using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class DeleteMoodMarkException : Exception
    {
        public DeleteMoodMarkException() : base()
        {

        }
        public DeleteMoodMarkException(string message) : base(message)
        {

        }
        public DeleteMoodMarkException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
