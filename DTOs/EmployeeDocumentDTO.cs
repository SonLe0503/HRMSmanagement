using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs
{
    public class UploadEmployeeDocumentDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(200)]
        public string DocumentTitle { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string DocumentCategory { get; set; } = null!;

        public bool IsConfidential { get; set; } = false;

        public int? UploadedBy { get; set; }

    }
    public class UpdateEmployeeDocumentDto
    {
        [Required]
        [MaxLength(200)]
        public string DocumentTitle { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string DocumentCategory { get; set; } = null!;

        public bool IsConfidential { get; set; }

        public int? ModifiedBy { get; set; }
    }
    public class EmployeeDocumentResponseDto
    {
        public int DocumentId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeFullName { get; set; } = null!;
        public string DocumentTitle { get; set; } = null!;
        public string DocumentCategory { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public int FileSize { get; set; }
        public string FileSizeFormatted { get; set; } = null!;
        public bool IsConfidential { get; set; }
        public DateTime UploadDate { get; set; }
        public int UploadedBy { get; set; }
        public string UploadedByName { get; set; } = null!;
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
    }
    public class EmployeeDocumentListDto
    {
        public int DocumentId { get; set; }
        public int EmployeeId { get; set; }
        public string DocumentTitle { get; set; } = null!;
        public string DocumentCategory { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public string FileSizeFormatted { get; set; } = null!;
        public bool IsConfidential { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedByName { get; set; } = null!;
    }
}