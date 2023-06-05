using BLL.Abstraction;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BLL.Implementation
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<Account> account)=>
            _cloudinary = new(account.Value);
        
        
        public async Task DestroyAsycn(string imageUrl)
        {
            string publicId = imageUrl.Split('/').Last().Split('.').First();
            DeletionParams deletionParams = new(publicId);
            var res = await _cloudinary.DestroyAsync(deletionParams);
        }

        public async Task<string> UploadAsync(IFormFile formFile)
        {
            ImageUploadParams uploadParams = new()
            {
                File = new FileDescription(formFile.FileName, formFile.OpenReadStream())
            };
            var res = await _cloudinary.UploadAsync(uploadParams);

            return res.SecureUrl.ToString();
        }
    }
}
