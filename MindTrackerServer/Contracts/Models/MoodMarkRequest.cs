using Microsoft.AspNetCore.Http;

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
