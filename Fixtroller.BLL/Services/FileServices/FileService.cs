using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.BLL.Services.FileService
{
    public class FileService : IFileService
    {
        private readonly string _publicBaseUrl;

        public FileService(IConfiguration configuration)
        {
            _publicBaseUrl = (configuration["App:PublicBaseUrl"] ?? string.Empty).TrimEnd('/');
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) throw new InvalidOperationException("Empty file.");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var stream = File.Create(path);
            await file.CopyToAsync(stream);

            return fileName;
        }

        public Task DeleteAsync(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }

        public string GetPublicUrl(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return string.Empty;

            // إن ما كان في BaseUrl، رجّع مسار نسبي (ينفع مع StaticFiles)
            if (string.IsNullOrEmpty(_publicBaseUrl))
                return $"/Images/{fileName}";

            return $"{_publicBaseUrl}/Images/{fileName}";
        }
    }
}


