using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HRManagement.Configuration;
using Microsoft.Extensions.Options;

namespace HRManagement.Services.Cloudinaries
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
                    var isImage = IsImagePublicId(extension);
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
                            Type = "upload"
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
                            Overwrite = false,
                            Type = "upload"
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
                            Overwrite = false,
                            Type = "upload"
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

            public async Task<bool> DeleteFileAsync(string publicIdOrUrl)
            {
                try
                {
                    if (string.IsNullOrEmpty(publicIdOrUrl))
                        return false;

                    string publicId;
                    ResourceType resourceType;

                    if (publicIdOrUrl.StartsWith("http://") || publicIdOrUrl.StartsWith("https://"))
                    {
                        (publicId, resourceType) = ExtractPublicIdFromUrl(publicIdOrUrl);
                    }
                    else
                    {
                        publicId = publicIdOrUrl;
                        resourceType = ResourceType.Image;
                    }

                    if (string.IsNullOrEmpty(publicId))
                        return false;

                    var deletionParams = new DeletionParams(publicId)
                    {
                        ResourceType = resourceType
                    };

                    var result = await _cloudinary.DestroyAsync(deletionParams);

                    if (result.Result == "not found" && resourceType == ResourceType.Image)
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

            private (string publicId, ResourceType resourceType) ExtractPublicIdFromUrl(string url)
            {
                // URL: https://res.cloudinary.com/{cloud}/{resource_type}/upload/{version?}/{public_id}
                var uri = new Uri(url);
                var path = uri.AbsolutePath; // e.g. /cloudname/image/upload/v123/HRMS/folder/file.png

                var uploadMarker = "/upload/";
                var uploadIndex = path.IndexOf(uploadMarker, StringComparison.OrdinalIgnoreCase);
                if (uploadIndex < 0)
                    return (url, ResourceType.Image);

                // Determine resource type from URL segment
                var resourceType = path.Contains("/image/", StringComparison.OrdinalIgnoreCase)
                    ? ResourceType.Image
                    : ResourceType.Raw;

                var afterUpload = path.Substring(uploadIndex + uploadMarker.Length);

                // Skip version segment (e.g. "v1234567890/")
                if (afterUpload.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                {
                    var slashIdx = afterUpload.IndexOf('/');
                    if (slashIdx > 0 && afterUpload.Substring(1, slashIdx - 1).All(char.IsDigit))
                        afterUpload = afterUpload.Substring(slashIdx + 1);
                }

                // For images, Cloudinary PublicId excludes the file extension
                if (resourceType == ResourceType.Image)
                {
                    var lastDot = afterUpload.LastIndexOf('.');
                    if (lastDot > 0)
                        afterUpload = afterUpload.Substring(0, lastDot);
                }

                return (afterUpload, resourceType);
            }

            public string GetOptimizedUrl(string publicId, string? fileType = null, int? width = null, int? height = null)
            {
                if (string.IsNullOrEmpty(publicId))
                    return string.Empty;

                bool isImage;

                if (!string.IsNullOrEmpty(fileType))
                {
                    isImage = IsImageFileType(fileType);
                }
                else
                {
                    isImage = IsImagePublicId(publicId);
                }

                if (isImage)
                {
                    var transformation = new Transformation();

                    if (width.HasValue)
                        transformation = transformation.Width(width.Value);

                    if (height.HasValue)
                        transformation = transformation.Height(height.Value);

                    transformation = transformation.Quality("auto").FetchFormat("auto");

                    return _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
                }
                else
                {
                    var baseUrl = _settings.CheckUrl
                        ? $"https://res.cloudinary.com/{_settings.CloudName}"
                        : $"http://res.cloudinary.com/{_settings.CloudName}";

                    return $"{baseUrl}/raw/upload/{publicId}";
                }
            }

            private bool IsImageFileType(string fileType)
            {
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
                return imageExtensions.Any(ext =>
                    ext.Equals(fileType, StringComparison.OrdinalIgnoreCase));
            }

            private bool IsImagePublicId(string publicId)
            {
                if (string.IsNullOrEmpty(publicId))
                    return false;

                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };

                return imageExtensions.Any(ext =>
                    publicId.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
