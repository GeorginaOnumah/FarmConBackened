namespace FarmConBackened.Interfaces
{
    public interface IImageService
    {
        Task<(string url, string thumbnailUrl, long sizeBytes)> SaveImageAsync(IFormFile file, string folder);
        Task DeleteImageAsync(string imageUrl);
        bool IsValidImage(IFormFile file);
    }
}
