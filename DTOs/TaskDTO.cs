namespace HRManagement.DTOs
{
    public class TaskDTO
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = null!;
        public string TaskType { get; set; } = null!;
        public string? TaskDescription { get; set; }
        public string Priority { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateOnly? DueDate { get; set; }
        public DateTime CreatedDate { get; set; }

        public int AssignedTo { get; set; }
        public string? AssignedUsername { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? CompletedDate { get; set; }
        public string? CompletionNotes { get; set; }
    }
    public class CreateTaskDTO
    {
        public string TaskTitle { get; set; } = null!;
        public string TaskType { get; set; } = null!;
        public string? TaskDescription { get; set; }
        public int AssignedTo { get; set; }
        public string Priority { get; set; } = null!;
        public DateOnly? DueDate { get; set; }
    }
    public class UpdateTaskDTO
    {
        public string? TaskTitle { get; set; }
        public string? TaskType { get; set; }
        public string? TaskDescription { get; set; }
        public int? AssignedTo { get; set; }
        public string? Priority { get; set; }
        public DateOnly? DueDate { get; set; }
        public string? CompletionNotes { get; set; }
    }
    public class ApproveTaskDTO
    {
        public string? Comments { get; set; }
    }

    public class RejectTaskDTO
    {
        public string Reason { get; set; } = null!;
    }
}
