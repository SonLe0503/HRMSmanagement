using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class EvaluationCycle
{
    public int CycleId { get; set; }

    public string CycleName { get; set; } = null!;

    public string CycleType { get; set; } = null!;

    public DateOnly EvaluationPeriodStart { get; set; }

    public DateOnly EvaluationPeriodEnd { get; set; }

    public DateOnly SelfEvaluationStart { get; set; }

    public DateOnly SelfEvaluationEnd { get; set; }

    public DateOnly ManagerEvaluationStart { get; set; }

    public DateOnly ManagerEvaluationEnd { get; set; }

    public DateOnly? ReviewMeetingStart { get; set; }

    public DateOnly? ReviewMeetingEnd { get; set; }

    public string? ApplicableDepartments { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}
