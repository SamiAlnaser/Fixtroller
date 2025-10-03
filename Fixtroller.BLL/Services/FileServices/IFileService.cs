using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.FileService
{
    public interface IFileService
    {
        Task<string> UploadAsync(IFormFile file);
    }
}
