using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class EvaluationCriterion
{
    public int CriteriaId { get; set; }

    public int TemplateId { get; set; }

    public string CriteriaName { get; set; } = null!;

    public string? CriteriaCategory { get; set; }

    public string? Description { get; set; }

    public int Weightage { get; set; }

    public int DisplayOrder { get; set; }

    public virtual ICollection<EvaluationRating> EvaluationRatings { get; set; } = new List<EvaluationRating>();

    public virtual EvaluationTemplate Template { get; set; } = null!;
}
