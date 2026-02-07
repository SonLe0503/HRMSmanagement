using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class EmployeeDocument
{
    public int DocumentId { get; set; }

    public int EmployeeId { get; set; }

    public string DocumentTitle { get; set; } = null!;

    public string DocumentCategory { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string FileType { get; set; } = null!;

    public int FileSize { get; set; }

    public bool IsConfidential { get; set; }

    public DateTime UploadDate { get; set; }

    public int UploadedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
