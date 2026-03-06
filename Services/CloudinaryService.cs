using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HRManagement.Configuration;
using Microsoft.Extensions.Options;

namespace HRManagement.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _settings;

        public CloudinaryService(IOptions<CloudinarySettings> settings)
        {
            _settings = settings.Value;

            var account = new Account(
                _settings.CloudName,
                _settings.ApiKey,
                _settings.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = _settings.CheckUrl;
        }

        public async Task<CloudinaryUploadResult> UploadFileAsync(IFormFile file, string? folder = null)
        {
            var result = new CloudinaryUploadResult();

            try
            {
                if (file.Length == 0)
                {
                    result.Success = false;
                    result.Error = "File is empty";
                    return result;
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var isImage = IsImageFile(extension);
                var isPdf = extension == ".pdf";

                var folderPath = string.IsNullOrEmpty(folder)
                    ? _settings.FolderName
                    : $"{_settings.FolderName}/{folder}";

                using var stream = file.OpenReadStream();

                if (isImage)
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folderPath,
                        UniqueFilename = true,
                        Overwrite = false,
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    result.Success = uploadResult.StatusCode == System.Net.HttpStatusCode.OK;
                    result.Url = uploadResult.Url?.ToString();
                    result.CheckUrl = uploadResult.SecureUrl?.ToString();
                    result.PublicId = uploadResult.PublicId;
                    result.Format = uploadResult.Format;
                    result.Bytes = uploadResult.Bytes;
                    result.ResourceType = "image";
                    result.Error = uploadResult.Error?.Message;
                }
                else if (isPdf)
                {
                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folderPath,
                        UniqueFilename = true,
                        Overwrite = false
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    result.Success = uploadResult.StatusCode == System.Net.HttpStatusCode.OK;
                    result.Url = uploadResult.Url?.ToString();
                    result.CheckUrl = uploadResult.SecureUrl?.ToString();
                    result.PublicId = uploadResult.PublicId;
                    result.Format = uploadResult.Format;
                    result.Bytes = uploadResult.Bytes;
                    result.ResourceType = "raw";
                    result.Error = uploadResult.Error?.Message;
                }
                else
                {
                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folderPath,
                        UniqueFilename = true,
                        Overwrite = false
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    result.Success = uploadResult.StatusCode == System.Net.HttpStatusCode.OK;
                    result.Url = uploadResult.Url?.ToString();
                    result.CheckUrl = uploadResult.SecureUrl?.ToString();
                    result.PublicId = uploadResult.PublicId;
                    result.Format = uploadResult.Format;
                    result.Bytes = uploadResult.Bytes;
                    result.ResourceType = "raw";
                    result.Error = uploadResult.Error?.Message;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
            }

            return result;
        }

        public async Task<bool> DeleteFileAsync(string publicId)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                    return false;

                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Result == "not found")
                {
                    deletionParams.ResourceType = ResourceType.Raw;
                    result = await _cloudinary.DestroyAsync(deletionParams);
                }
                return result.Result == "ok";
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetOptimizedUrl(string publicId, int? width = null, int? height = null)
        {
            if (string.IsNullOrEmpty(publicId))
                return string.Empty;

            var transformation = new Transformation();

            if (width.HasValue)
                transformation = transformation.Width(width.Value);

            if (height.HasValue)
                transformation = transformation.Height(height.Value);

            transformation = transformation.Quality("auto").FetchFormat("auto");

            return _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
        }

        private bool IsImageFile(string extension)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
            return Array.Exists(imageExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }
    }
}
