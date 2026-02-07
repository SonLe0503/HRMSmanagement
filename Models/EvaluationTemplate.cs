using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class EvaluationTemplate
{
    public int TemplateId { get; set; }

    public string TemplateName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public virtual ICollection<EvaluationCriterion> EvaluationCriteria { get; set; } = new List<EvaluationCriterion>();

    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}
