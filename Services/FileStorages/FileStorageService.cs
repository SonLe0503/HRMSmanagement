using System.Text.RegularExpressions;

namespace HRManagement.Services.FileStorages
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        
        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveBase64ImageAsync(string base64Image, string folderName, string filePrefix)
        {
            if (string.IsNullOrWhiteSpace(base64Image))
                throw new ArgumentException("Ảnh không hợp lệ.");

            var matches = Regex.Match(base64Image, @"data:image/(?<type>.+?);base64,(?<data>.+)");
            string extension = "jpg";
            string rawBase64 = base64Image;

            if (matches.Success)
            {
                extension = matches.Groups["type"].Value switch
                {
                    "jpeg" => "jpg",
                    "png" => "png",
                    "jpg" => "jpg",
                    _ => "jpg"
                };

                rawBase64 = matches.Groups["data"].Value;
            }

            byte[] imageBytes = Convert.FromBase64String(rawBase64);

            var uploadRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", folderName);
            if (!Directory.Exists(uploadRoot))
                Directory.CreateDirectory(uploadRoot);

            var fileName = $"{filePrefix}_{Guid.NewGuid():N}.{extension}";
            var fullPath = Path.Combine(uploadRoot, fileName);

            await File.WriteAllBytesAsync(fullPath, imageBytes);

            return $"/uploads/{folderName}/{fileName}";
        }

    }
}
