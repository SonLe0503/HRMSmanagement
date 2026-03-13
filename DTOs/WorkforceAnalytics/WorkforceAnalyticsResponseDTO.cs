namespace HRManagement.DTOs.WorkforceAnalytics
{
    public class WorkforceAnalyticsResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public HeadcountAnalyticsDTO HeadcountAnalytics { get; set; } = new();
        public DemographicsAnalyticsDTO DemographicsAnalytics { get; set; } = new();
        public AttritionAnalyticsDTO AttritionAnalytics { get; set; } = new();
        public TalentAnalyticsDTO TalentAnalytics { get; set; } = new();
        public EngagementProductivityAnalyticsDTO EngagementProductivity { get; set; } = new();
    }

    public class HeadcountAnalyticsDTO
    {
        public int TotalHeadcount { get; set; }
        public int NewHires { get; set; }
        public int Terminations { get; set; }
        public List<object> HeadcountByDepartment { get; set; } = new();
        public List<object> HeadcountTrend { get; set; } = new();
        public decimal VacancyRate { get; set; }
        public decimal ContractorVsPermanentRatio { get; set; }
    }

    public class DemographicsAnalyticsDTO
    {
        public List<object> AgeDistribution { get; set; } = new();
        public List<object> GenderDistribution { get; set; } = new();
        public List<object> TenureDistribution { get; set; } = new();
        public List<object> LocationDistribution { get; set; } = new();
        public List<object> PositionLevelDistribution { get; set; } = new();
    }

    public class AttritionAnalyticsDTO
    {
        public decimal OverallTurnoverRate { get; set; }
        public List<object> TurnoverByDepartment { get; set; } = new();
        public List<object> TurnoverByTenureBand { get; set; } = new();
        public List<object> AttritionTrend { get; set; } = new();
        public List<object> ReasonsForLeaving { get; set; } = new();
    }

    public class TalentAnalyticsDTO
    {
        public List<object> PerformanceRatingDistribution { get; set; } = new();
        public int HighPerformerCount { get; set; }
        public decimal PromotionRate { get; set; }
        public List<object> InternalMobilityPatterns { get; set; } = new();
        public List<object> SkillGapAnalysis { get; set; } = new();
    }

    public class EngagementProductivityAnalyticsDTO
    {
        public decimal AverageAttendanceRate { get; set; }
        public List<object> LeaveUtilizationPatterns { get; set; } = new();
        public List<object> OvertimeTrends { get; set; } = new();
        public List<object> ProductivityMetrics { get; set; } = new();
    }
}
