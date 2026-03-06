namespace HRManagement.Services
{
    public interface ICloudinaryService
    {
        Task<CloudinaryUploadResult> UploadFileAsync(IFormFile file, string? folder = null);
        Task<bool> DeleteFileAsync(string publicId);
        string GetOptimizedUrl(string publicId, int? width = null, int? height = null);

    }
    public class CloudinaryUploadResult
    {
        public bool Success { get; set; }
        public string? Url { get; set; }
        public string? CheckUrl { get; set; }
        public string? PublicId { get; set; }
        public string? Format { get; set; }
        public long Bytes { get; set; }
        public string? Error { get; set; }
        public string? ResourceType { get; set; }
    }
}
