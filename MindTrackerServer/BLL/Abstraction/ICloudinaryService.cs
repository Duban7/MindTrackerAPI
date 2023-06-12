using Microsoft.AspNetCore.Http;

namespace BLL.Abstraction
{
    public interface ICloudinaryService
    {
        public Task<string> UploadAsync(IFormFile formFile);
        public Task DestroyAsycn(string publicId);
    }
}
