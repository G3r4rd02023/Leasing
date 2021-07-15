using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Leasing.Helpers
{
    public interface IImageHelper
    {
        Task<string> UploadImageAsync(IFormFile imageFile);
    }
}
