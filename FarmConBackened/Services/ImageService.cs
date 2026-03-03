using FarmConBackened.Interfaces;
//using static System.Net.Mime.MediaTypeNames;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.EntityFrameworkCore;

namespace FarmConBackened.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

        public ImageService(IWebHostEnvironment env, IConfiguration config)
        { _env = env; _config = config; }

        public bool IsValidImage(IFormFile file)
        {
            if (file.Length > MaxFileSizeBytes) return false;
            var ext = Path.GetExtension(file.FileName).ToLower();
            return _allowedExtensions.Contains(ext);
        }

        public async Task<(string url, string thumbnailUrl, long sizeBytes)> SaveImageAsync(IFormFile file, string folder)
        {
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsPath);

            // Change this line in ImageService.cs
            var fileName = $"{Guid.NewGuid():N}.webp";
            var thumbName = $"thumb_{fileName}";
            var filePath = Path.Combine(uploadsPath, fileName);
            var thumbPath = Path.Combine(uploadsPath, thumbName);

            using var image = await Image.LoadAsync(file.OpenReadStream());

            // Optimize: resize to max 1200px wide, save as WebP quality 80
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new SixLabors.ImageSharp.Size(1200, 1200)
            }));
            await image.SaveAsWebpAsync(filePath);

            // Thumbnail: 300px
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new SixLabors.ImageSharp.Size(300, 300)
            }));
            await image.SaveAsWebpAsync(thumbPath);

            var baseUrl = _config["App:BaseUrl"] ?? "http://localhost:5000";
            return (
                $"{baseUrl}/uploads/{folder}/{fileName}",
                $"{baseUrl}/uploads/{folder}/{thumbName}",
                new FileInfo(filePath).Length
            );
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            await Task.Run(() =>
            {
                try
                {
                    var baseUrl = _config["App:BaseUrl"] ?? "https://localhost:5001";
                    var relativePath = imageUrl.Replace(baseUrl, "").TrimStart('/');
                    var filePath = Path.Combine(_env.WebRootPath, relativePath);

                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    // Also try deleting the thumbnail
                    var thumbPath = Path.Combine(Path.GetDirectoryName(filePath)!, "thumb_" + Path.GetFileName(filePath));
                    if (File.Exists(thumbPath))
                        File.Delete(thumbPath);
                }
                catch { /* Log this in production */ }
            });
        }

    }
}
