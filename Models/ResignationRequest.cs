namespace HRManagement.Models;

public partial class ResignationRequest
{
    public int ResignationRequestId { get; set; }

    public string RequestNumber { get; set; } = null!;

    public int EmployeeId { get; set; }

    public DateOnly ExpectedLastWorkingDate { get; set; }

    public string? Reason { get; set; }

    public string? HandoverNote { get; set; }

    public int? HandoverToEmployeeId { get; set; }

    public string Status { get; set; } = "Pending";

    public string? RejectionReason { get; set; }

    public string? ReviewerComments { get; set; }

    public DateTime SubmittedDate { get; set; }

    public DateTime? ReviewedDate { get; set; }

    public int? ReviewedBy { get; set; }

    public int? TargetApproverId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Employee? HandoverToEmployee { get; set; }

    public virtual User? ReviewedByNavigation { get; set; }

    public virtual User? TargetApprover { get; set; }
}
