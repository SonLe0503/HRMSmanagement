using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int? EmployeeId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<LeaveRequest> LeaveRequestApprovedByNavigations { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<LeaveRequest> LeaveRequestReviewedByNavigations { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<OvertimeRequest> OvertimeRequestApprovedByNavigations { get; set; } = new List<OvertimeRequest>();

    public virtual ICollection<OvertimeRequest> OvertimeRequestReviewedByNavigations { get; set; } = new List<OvertimeRequest>();

    public virtual ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<WorkflowStageApprover> WorkflowStageApprovers { get; set; } = new List<WorkflowStageApprover>();
}
