namespace HRManagement.DTOs
{
    public class CreateWorkflowStageDTO
    {
        public int WorkflowId { get; set; }
        public int StageOrder { get; set; }
        public string StageName { get; set; } = null!;
        public string ApprovalType { get; set; } = "Single"; // Single | Parallel
        public int? TimeoutHours { get; set; }
        public bool IsAutoApprove { get; set; }
    }
    public class UpdateWorkflowStageDTO
    {
        public int StageOrder { get; set; }
        public string StageName { get; set; } = null!;
        public string ApprovalType { get; set; } = "Single";
        public int? TimeoutHours { get; set; }
        public bool IsAutoApprove { get; set; }
    }

}
