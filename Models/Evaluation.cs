using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class Evaluation
{
    public int EvaluationId { get; set; }

    public int CycleId { get; set; }

    public int EmployeeId { get; set; }

    public int TemplateId { get; set; }

    public int? PrimaryEvaluatorId { get; set; }

    public int? SecondaryEvaluatorId { get; set; }

    public string Status { get; set; } = null!;

    public decimal? OverallRating { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public DateTime? AcknowledgedDate { get; set; }

    public string? AcknowledgementComments { get; set; }

    public virtual EvaluationCycle Cycle { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<EvaluationRating> EvaluationRatings { get; set; } = new List<EvaluationRating>();

    public virtual Employee? PrimaryEvaluator { get; set; }

    public virtual Employee? SecondaryEvaluator { get; set; }

    public virtual EvaluationTemplate Template { get; set; } = null!;
}
