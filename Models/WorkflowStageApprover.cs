using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class WorkflowStageApprover
{
    public int StageApproverId { get; set; }

    public int StageId { get; set; }

    public int ApproverType { get; set; }

    public int? RoleId { get; set; }

    public int? UserId { get; set; }

    public bool IsDynamic { get; set; }

    public string? DynamicRule { get; set; }

    public virtual Role? Role { get; set; }

    public virtual WorkflowStage Stage { get; set; } = null!;

    public virtual User? User { get; set; }
}
