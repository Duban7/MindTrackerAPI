using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class UpdateGroupSchemaException : Exception
    {
        public UpdateGroupSchemaException() : base()
        {

        }
        public UpdateGroupSchemaException(string message) : base(message)
        {

        }
        public UpdateGroupSchemaException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
