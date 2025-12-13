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

        public async Task<(string ImagePath, string ThumbPath)> UploadUserAvatarAsync(
    string userId,
    IFormFile file,
    CancellationToken ct = default)
        {
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("Empty file.");

            ct.ThrowIfCancellationRequested();

            // ✅ validation بسيط
            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowed.Contains(file.ContentType))
                throw new InvalidOperationException("Invalid image type.");

            var maxBytes = 2 * 1024 * 1024; // 2MB
            if (file.Length > maxBytes)
                throw new InvalidOperationException("File too large.");

            // ✅ مسارات التخزين داخل wwwroot/Images
            var imagesRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
            var userDir = Path.Combine(imagesRoot, "Users", userId);
            var thumbsDir = Path.Combine(userDir, "thumbs");

            Directory.CreateDirectory(userDir);
            Directory.CreateDirectory(thumbsDir);

            var stamp = Guid.NewGuid().ToString("N");
            var mainFileName = $"avatar_{stamp}.png";
            var thumbFileName = $"avatar_thumb_{stamp}.png";

            var absMainPath = Path.Combine(userDir, mainFileName);
            var absThumbPath = Path.Combine(thumbsDir, thumbFileName);

            const int mainSize = 512;
            const int thumbSize = 128;

            await using var stream = file.OpenReadStream();
            using var img = Image.Load<Rgba32>(stream);

            using var main = MakeCircular(img, mainSize);
            await main.SaveAsync(absMainPath, new PngEncoder(), ct);

            using var thumb = MakeCircular(img, thumbSize);
            await thumb.SaveAsync(absThumbPath, new PngEncoder(), ct);

            // ✅ نخزن relative path بحيث GetPublicUrl يطلع الرابط صح
            var relMain = $"Users/{userId}/{mainFileName}";
            var relThumb = $"Users/{userId}/thumbs/{thumbFileName}";

            return (relMain, relThumb);
        }

        // ✅ قص مربع + resize + ماسك دائري (خارج الدائرة شفاف)
        private static Image<Rgba32> MakeCircular(Image<Rgba32> source, int size)
        {
            int min = Math.Min(source.Width, source.Height);
            int x = (source.Width - min) / 2;
            int y = (source.Height - min) / 2;

            using var square = source.Clone(ctx =>
            {
                ctx.Crop(new Rectangle(x, y, min, min));
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(size, size),
                    Mode = ResizeMode.Crop
                });
            });

            var output = new Image<Rgba32>(size, size);
            output.Mutate(c => c.BackgroundColor(new Rgba32(0, 0, 0, 0)));

            var srcFrame = square.Frames.RootFrame;
            var dstFrame = output.Frames.RootFrame;

            float radius = size / 2f;
            float cx = radius - 0.5f;
            float cy = radius - 0.5f;
            float r2 = radius * radius;

            for (int j = 0; j < size; j++)
            {
                Span<Rgba32> srcRow = srcFrame.DangerousGetPixelRowMemory(j).Span;
                Span<Rgba32> dstRow = dstFrame.DangerousGetPixelRowMemory(j).Span;


                for (int i = 0; i < size; i++)
                {
                    float dx = i - cx;
                    float dy = j - cy;

                    dstRow[i] = (dx * dx + dy * dy) <= r2
                        ? srcRow[i]
                        : new Rgba32(0, 0, 0, 0);
                }
            }

            return output;
        }


    }
}
