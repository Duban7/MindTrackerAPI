using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class MoodMark
    {
        public string? AccountId { get; set; } 
        public DateTime Date { get; set; }
        public string? Mood { get; set; }
        public List<MoodActivity>? Activities { get; set; }
        public string[]? Images { get; set; }
        public string? Note { get; set; }
    }
}
