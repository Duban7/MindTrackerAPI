using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class MoodMarkRequest
    {
        public string? Record { get; set; }
        public List<string>? Images { get; set; }
        public List<IFormFile>? NewImages {  get; set; }
        public List<string>? DeletedImages { get; set; }
     
    }
}
