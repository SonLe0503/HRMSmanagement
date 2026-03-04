namespace HRManagement.DTOs
{
    public class PendingApprovalDTO
    {
        public int RequestId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public string RequestType { get; set; } = null!;
        public DateTime SubmissionDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal TotalUnits { get; set; } // days hoặc hours
        public string Reason { get; set; } = null!;
        public bool IsUrgent { get; set; }
    }
}
