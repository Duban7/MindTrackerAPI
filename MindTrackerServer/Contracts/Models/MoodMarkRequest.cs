using Microsoft.AspNetCore.Http;

namespace Domain.Models
{
    public class MoodMarkRequest
    {
        public string? Record { get; set; }
        public string? Images { get; set; }
        public List<IFormFile>? NewImages {  get; set; }
        public string? DeletedImages { get; set; }
     
    }
}
