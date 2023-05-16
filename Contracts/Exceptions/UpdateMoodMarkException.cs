using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class UpdateMoodMarkException : Exception
    {
        public UpdateMoodMarkException() : base()
        {

        }
        public UpdateMoodMarkException(string message) : base(message)
        {

        }
        public UpdateMoodMarkException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
