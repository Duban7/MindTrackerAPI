using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class GroupsNotFoundExeption: Exception
    {
        public GroupsNotFoundExeption() : base()
        {

        }
        public GroupsNotFoundExeption(string message) : base(message)
        {

        }
        public GroupsNotFoundExeption(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
