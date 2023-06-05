using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Abstraction
{
    public interface ICloudinaryService
    {
        public Task<string> UploadAsync(IFormFile formFile);
        public Task DestroyAsycn(string publicId);
    }
}
