namespace HRManagement.DTOs
{
    public class CreateWorkflowDTO
    {
        public string WorkflowName { get; set; } = string.Empty;
        public string WorkflowType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
    public class UpdateWorkflowDTO
    {
        public string WorkflowName { get; set; } = null!;
        public string WorkflowType { get; set; } = null!;
        public string? Description { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public bool IsActive { get; set; }
    }

}
