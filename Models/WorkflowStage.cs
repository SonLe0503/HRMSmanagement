using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class WorkflowStage
{
    public int StageId { get; set; }

    public int WorkflowId { get; set; }

    public int StageOrder { get; set; }

    public string StageName { get; set; } = null!;

    public string ApprovalType { get; set; } = null!;

    public int? TimeoutHours { get; set; }

    public bool IsAutoApprove { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Workflow Workflow { get; set; } = null!;

    public virtual ICollection<WorkflowStageApprover> WorkflowStageApprovers { get; set; } = new List<WorkflowStageApprover>();
}
