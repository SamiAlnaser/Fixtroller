using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;


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

            // ✅ normalize
            fileName = fileName.Replace("\\", "/").Trim();

            // ✅ لو انخزن كـ URL
            if (fileName.StartsWith("/Images/", StringComparison.OrdinalIgnoreCase))
                fileName = fileName.Substring("/Images/".Length);

            if (fileName.StartsWith("Images/", StringComparison.OrdinalIgnoreCase))
                fileName = fileName.Substring("Images/".Length);

            // ✅ احذف querystring لو موجود (نادر)
            var qIndex = fileName.IndexOf("?", StringComparison.Ordinal);
            if (qIndex >= 0)
                fileName = fileName.Substring(0, qIndex);

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

        public async Task<string> UploadUserAvatarAsync(
        string userId,
        IFormFile file,
        CancellationToken ct = default)
        {
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("Empty file.");

            ct.ThrowIfCancellationRequested();

            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowed.Contains(file.ContentType))
                throw new InvalidOperationException("Invalid image type.");

            var maxBytes = 2 * 1024 * 1024; // 2MB
            if (file.Length > maxBytes)
                throw new InvalidOperationException("File too large.");

            var imagesRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
            var userDir = Path.Combine(imagesRoot, "Users", userId);
            Directory.CreateDirectory(userDir);

            // ✅ تنظيف أي thumbs قديمة (لو كانت لسا موجودة على السيرفر)
            var legacyThumbsDir = Path.Combine(userDir, "thumbs");
            if (Directory.Exists(legacyThumbsDir))
            {
                try { Directory.Delete(legacyThumbsDir, recursive: true); }
                catch { /* ignore */ }
            }

            var stamp = Guid.NewGuid().ToString("N");
            var mainFileName = $"avatar_{stamp}.png";
            var absMainPath = Path.Combine(userDir, mainFileName);

            await using var stream = file.OpenReadStream();
            using var img = Image.Load<Rgba32>(stream);

            // ✅ بدون دويرة وبدون قص: فقط resize لأقصى حد
            const int maxSide = 512;
            if (img.Width > maxSide || img.Height > maxSide)
            {
                img.Mutate(ctx => ctx.Resize(new ResizeOptions
                {
                    Size = new Size(maxSide, maxSide),
                    Mode = ResizeMode.Max
                }));
            }

            await img.SaveAsync(absMainPath, new PngEncoder(), ct);

            // ✅ relative path
            return $"Users/{userId}/{mainFileName}";
        }


    }
}
