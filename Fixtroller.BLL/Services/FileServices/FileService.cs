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

        public async Task<string> UploadAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("Empty file.");

            ct.ThrowIfCancellationRequested();

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";

            var imagesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
            Directory.CreateDirectory(imagesDir);

            var path = Path.Combine(imagesDir, fileName);

            await using var stream = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await file.CopyToAsync(stream, ct);
            return fileName;
        }

        public Task DeleteAsync(string fileName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Task.CompletedTask;

            ct.ThrowIfCancellationRequested();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);
            if (File.Exists(path))
                File.Delete(path);

            return Task.CompletedTask;
        }

        public string GetPublicUrl(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return string.Empty;

            return string.IsNullOrEmpty(_publicBaseUrl)
                ? $"/Images/{fileName}"
                : $"{_publicBaseUrl}/Images/{fileName}";
        }
    }
}
