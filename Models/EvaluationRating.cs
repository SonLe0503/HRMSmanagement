using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class EvaluationRating
{
    public int RatingId { get; set; }

    public int EvaluationId { get; set; }

    public int CriteriaId { get; set; }

    public decimal? SelfRating { get; set; }

    public string? SelfComments { get; set; }

    public decimal? ManagerRating { get; set; }

    public string? ManagerComments { get; set; }

    public string? Evidence { get; set; }

    public virtual EvaluationCriterion Criteria { get; set; } = null!;

    public virtual Evaluation Evaluation { get; set; } = null!;
}
