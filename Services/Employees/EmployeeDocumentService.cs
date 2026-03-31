using HRManagement.DataAcess.Interfaces;
using HRManagement.DTOs;
using HRManagement.Models;
using HRManagement.Services.Cloudinaries;
using System.Reflection.Metadata;

namespace HRManagement.Services.Employees
{
    public class EmployeeDocumentService : IEmployeeDocumentService
    {
        private readonly IEmployeeDocumentRepository _documentRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public EmployeeDocumentService(IEmployeeDocumentRepository documentRepository,ICloudinaryService cloudinaryService, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _documentRepository = documentRepository;
            _cloudinaryService = cloudinaryService;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<bool> DeleteDocumentAsync(int documentId)
        {
            var document = await _documentRepository.GetDocumentByIdAsync(documentId);
            if (document == null)
            {
                return false;
            }
            var cloudinaryPublicId = document.FilePath;
            var deleted = await _cloudinaryService.DeleteFileAsync(cloudinaryPublicId);

            if (!deleted)
            {
                throw new InvalidOperationException("Failed to delete file from storage.");
            }

            return await _documentRepository.DeleteDocumentAsync(documentId);
        }

        public async Task<(byte[] fileContent, string fileName, string contentType)?> DownloadDocumentAsync(int documentId)
        {
            var document = await _documentRepository.GetDocumentByIdAsync(documentId);
            if (document == null)
                return null;

            if (string.IsNullOrEmpty(document.FilePath))
                return null;

            var httpClient = _httpClientFactory.CreateClient();

            byte[] fileContent;
            try
            {
                fileContent = await httpClient.GetByteArrayAsync(document.FilePath);
            }
            catch
            {
                throw new InvalidOperationException("Failed to download file from storage.");
            }
            var contentType = GetContentType(document.FileType);

            return (fileContent, document.FileName, contentType);
        }

        public async Task<EmployeeDocumentResponseDto?> GetDocumentByIdAsync(int documentId)
        {
            var document = await _documentRepository.GetDocumentByIdWithDetailsAsync(documentId);
            if (document == null)
                return null;

            
            var cloudinaryUrl = _cloudinaryService.GetOptimizedUrl(document.FilePath, document.FileType);
            return new EmployeeDocumentResponseDto
            {
                DocumentId = document.DocumentId,
                EmployeeId = document.EmployeeId,
                EmployeeFullName = document.Employee?.FullName ?? "Unknown",
                DocumentTitle = document.DocumentTitle,
                DocumentCategory = document.DocumentCategory,
                FileName = document.FileName,
                FilePath = document.FilePath, 
                FileType = document.FileType,
                FileSize = document.FileSize,
                FileSizeFormatted = FormatFileSize(document.FileSize),
                IsConfidential = document.IsConfidential,
                UploadDate = document.UploadDate,
                UploadedBy = document.UploadedBy,
                UploadedByName = "System",
                ModifiedDate = document.ModifiedDate,
                ModifiedBy = document.ModifiedBy,
                ModifiedByName = document.ModifiedBy.HasValue ? "System" : null
            };
        }

        public async Task<IEnumerable<EmployeeDocumentListDto>> GetDocumentsByCategoryAsync(
            int employeeId,
            string category)
        {
            var documents = await _documentRepository.GetDocumentsByCategoryAsync(employeeId, category);

            return documents.Select(d => new EmployeeDocumentListDto
            {
                DocumentId = d.DocumentId,
                EmployeeId = d.EmployeeId,
                DocumentTitle = d.DocumentTitle,
                DocumentCategory = d.DocumentCategory,
                FileName = d.FileName,
                FileType = d.FileType,
                FileSizeFormatted = FormatFileSize(d.FileSize),
                IsConfidential = d.IsConfidential,
                UploadDate = d.UploadDate,
                UploadedByName = "System"
            }).ToList();
        }

        public async Task<IEnumerable<EmployeeDocumentListDto>> GetEmployeeDocumentsAsync(int employeeId)
        {
            var document = await _documentRepository.GetDocumentsByEmployeeIdAsync(employeeId);
                
            return document.Select(d => new EmployeeDocumentListDto
            {
                DocumentId = d.DocumentId,
                EmployeeId = d.EmployeeId,
                DocumentTitle = d.DocumentTitle,
                DocumentCategory = d.DocumentCategory,
                FileName = d.FileName,
                FileType = d.FileType,
                FileSizeFormatted = FormatFileSize(d.FileSize),
                IsConfidential = d.IsConfidential,
                UploadDate = d.UploadDate,
                UploadedByName = "System"
            });
        }

        public async Task<EmployeeDocumentResponseDto?> UpdateDocumentAsync(int documentId, UpdateEmployeeDocumentDto updateDto)
        {
            var document = await _documentRepository.GetDocumentByIdAsync(documentId);
            if(document == null)
                return null;
            var validCategories = new[]
{
                "Contract","Certificate","Identification","Resume","Other"
            };

            if (!validCategories.Contains(updateDto.DocumentCategory))
            {
                throw new ArgumentException("Invalid document category.");
            }
            if (string.IsNullOrWhiteSpace(updateDto.DocumentTitle))
            {
                throw new ArgumentException("Document title is required.");
            }

                document.DocumentTitle = updateDto.DocumentTitle;
            document.DocumentCategory = updateDto.DocumentCategory;
            document.IsConfidential = updateDto.IsConfidential;
            document.ModifiedDate = DateTime.UtcNow;
            document.ModifiedBy = GetCurrentUserId();

            await _documentRepository.UpdateDocumentAsync(document);
            var cloudinaryUrl = _cloudinaryService.GetOptimizedUrl(document.FilePath, document.FileType);

            return new EmployeeDocumentResponseDto
            {
                DocumentId = document.DocumentId,
                EmployeeId = document.EmployeeId,
                EmployeeFullName = document.Employee?.FullName ?? "Unknown",
                DocumentTitle = document.DocumentTitle,
                DocumentCategory = document.DocumentCategory,
                FileName = document.FileName,
                FilePath = document.FilePath, 
                FileType = document.FileType,
                FileSize = document.FileSize,
                FileSizeFormatted = FormatFileSize(document.FileSize),
                IsConfidential = document.IsConfidential,
                UploadDate = document.UploadDate,
                UploadedBy = document.UploadedBy,
                UploadedByName = "System",
                ModifiedDate = document.ModifiedDate,
                ModifiedBy = document.ModifiedBy,
                ModifiedByName = document.ModifiedBy.HasValue ? "System" : null
            };
        }

        public async Task<EmployeeDocumentResponseDto> UploadDocumentAsync(UploadEmployeeDocumentDto uploadDto, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file selected for upload.");
            }
                
            var allowedExtensions = new[] { ".pdf", ".jpg", ".png", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("The file format is not supported.");
            }           
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("File size exceeds the 5MB limit.");
            }
                
            if (!await _documentRepository.EmployeeExistsAsync(uploadDto.EmployeeId))
            {
                throw new KeyNotFoundException("Employee not found in the system.");
            }
            var validCategories = new[]
            {
                "Contract","Certificate","Identification","Resume","Other"
            };

            if (!validCategories.Contains(uploadDto.DocumentCategory))
            {
                throw new ArgumentException("Invalid document category.");
            }
            if (string.IsNullOrWhiteSpace(uploadDto.DocumentTitle))
            {
                throw new ArgumentException("Document title is required.");
            }

            int uploadedBy = uploadDto.UploadedBy ?? GetCurrentUserId();

            if (uploadedBy <= 0)
            {
                throw new ArgumentException("UploadedBy must be a valid user.");
            }

            var folder = $"HRMS/Employee_{uploadDto.EmployeeId}/{uploadDto.DocumentCategory}";
            var uploadResult = await _cloudinaryService.UploadFileAsync(file, folder);
            if (!uploadResult.Success || string.IsNullOrEmpty(uploadResult.PublicId))
            {
                throw new InvalidOperationException($"Upload failed: {uploadResult.Error}");
            }
            var document = new EmployeeDocument
            {
                EmployeeId = uploadDto.EmployeeId,
                DocumentTitle = uploadDto.DocumentTitle,
                DocumentCategory = uploadDto.DocumentCategory,
                FileName = file.FileName,
                FilePath = uploadResult.CheckUrl!,
                FileType = extension,
                FileSize = (int)file.Length,
                IsConfidential = uploadDto.IsConfidential,
                UploadDate = DateTime.UtcNow,
                UploadedBy = GetCurrentUserId()
            };
            await _documentRepository.AddDocumentAsync(document);
            return new EmployeeDocumentResponseDto
            {
                DocumentId = document.DocumentId,
                EmployeeId = document.EmployeeId,
                EmployeeFullName = document.Employee?.FullName ?? "Unknown",
                DocumentTitle = document.DocumentTitle,
                DocumentCategory = document.DocumentCategory,
                FileName = document.FileName,
                FilePath = uploadResult.CheckUrl!, 
                FileType = document.FileType,
                FileSize = document.FileSize,
                FileSizeFormatted = FormatFileSize(document.FileSize),
                IsConfidential = document.IsConfidential,
                UploadDate = document.UploadDate,
                UploadedBy = document.UploadedBy,
                UploadedByName = "System",
                ModifiedDate = document.ModifiedDate,
                ModifiedBy = document.ModifiedBy,
                ModifiedByName = document.ModifiedBy.HasValue ? "System" : null
            };
        }
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        private string GetContentType(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
        private int GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext?
                .User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (int.TryParse(claim, out int userId))
                return userId;

            return 0;
        }
    }

}
