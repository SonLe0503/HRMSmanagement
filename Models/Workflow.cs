using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class Workflow
{
    public int WorkflowId { get; set; }

    public string WorkflowName { get; set; } = null!;

    public string WorkflowType { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual ICollection<WorkflowStage> WorkflowStages { get; set; } = new List<WorkflowStage>();
}
