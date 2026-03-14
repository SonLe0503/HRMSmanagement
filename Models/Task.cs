using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public string TaskTitle { get; set; } = null!;

    public string TaskType { get; set; } = null!;

    public string? TaskDescription { get; set; }

    public int AssignedTo { get; set; }

    public int? RelatedRequestId { get; set; }

    public string? RelatedRequestType { get; set; }

    public string Priority { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateOnly? DueDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CompletedDate { get; set; }

    public int? CompletedBy { get; set; }

    public string? CompletionNotes { get; set; }

    public virtual User AssignedToNavigation { get; set; } = null!;
}
