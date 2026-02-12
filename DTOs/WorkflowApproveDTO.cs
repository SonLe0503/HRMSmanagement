namespace HRManagement.DTOs
{
    public class CreateWorkflowStageApproverDTO
    {
        public int StageId { get; set; }
        public int ApproverType { get; set; }
        public int? RoleId { get; set; }
        public int? UserId { get; set; }
        public string? DynamicRule { get; set; }
    }
    public class UpdateWorkflowStageApproverDTO
    {
        public int ApproverType { get; set; }
        public int? RoleId { get; set; }
        public int? UserId { get; set; }
        public string? DynamicRule { get; set; }
    }

}
