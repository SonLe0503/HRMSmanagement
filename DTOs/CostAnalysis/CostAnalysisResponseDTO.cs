namespace HRManagement.DTOs.CostAnalysis
{
    public class CostAnalysisResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public TotalCostAnalyticsDTO TotalCostAnalytics { get; set; } = new();
        public SalaryCompensationAnalyticsDTO SalaryCompensationAnalytics { get; set; } = new();
        public BenefitsCostAnalyticsDTO BenefitsCostAnalytics { get; set; } = new();
        public RecruitmentCostAnalyticsDTO RecruitmentCostAnalytics { get; set; } = new();
        public TrainingDevelopmentCostAnalyticsDTO TrainingDevelopmentCostAnalytics { get; set; } = new();
        public AttritionCostAnalyticsDTO AttritionCostAnalytics { get; set; } = new();
        public CostEfficiencyMetricsDTO CostEfficiencyMetrics { get; set; } = new();
    }

    public class TotalCostAnalyticsDTO
    {
        public decimal TotalWorkforceCost { get; set; }
        public decimal CostPerEmployee { get; set; }
        public List<object> CostByDepartment { get; set; } = new();
        public List<object> CostTrend { get; set; } = new();
        public List<object> BudgetVsActual { get; set; } = new();
        public decimal ForecastCost { get; set; }
    }

    public class SalaryCompensationAnalyticsDTO
    {
        public decimal BaseSalaryCosts { get; set; }
        public decimal VariablePayCosts { get; set; }
        public decimal OvertimeCosts { get; set; }
        public List<object> SalaryIncreaseAnalysis { get; set; } = new();
        public decimal CompensationRatio { get; set; }
        public List<object> PayEquityAnalysis { get; set; } = new();
    }

    public class BenefitsCostAnalyticsDTO
    {
        public decimal HealthInsuranceCosts { get; set; }
        public decimal RetirementContributions { get; set; }
        public decimal OtherBenefitsCosts { get; set; }
        public List<object> CostPerBenefitType { get; set; } = new();
        public decimal BenefitsUtilizationRate { get; set; }
        public decimal BenefitsRoiAnalysis { get; set; }
    }

    public class RecruitmentCostAnalyticsDTO
    {
        public decimal TotalRecruitmentCost { get; set; }
        public List<object> CostPerHireBySource { get; set; } = new();
        public List<object> CostPerHireByPosition { get; set; } = new();
        public List<object> RecruitmentCostTrend { get; set; } = new();
        public decimal RecruitmentRoi { get; set; }
    }

    public class TrainingDevelopmentCostAnalyticsDTO
    {
        public decimal TrainingCostsPerEmployee { get; set; }
        public decimal TotalTrainingCost { get; set; }
        public List<object> TrainingCostsByProgram { get; set; } = new();
        public decimal TrainingRoi { get; set; }
        public decimal ExternalVsInternalTrainingCostRatio { get; set; }
    }

    public class AttritionCostAnalyticsDTO
    {
        public decimal CostOfTurnover { get; set; }
        public List<object> ReplacementCostByPosition { get; set; } = new();
        public decimal LostProductivityCost { get; set; }
        public decimal KnowledgeLossImpact { get; set; }
    }

    public class CostEfficiencyMetricsDTO
    {
        public decimal RevenuePerEmployee { get; set; }
        public decimal ProfitPerEmployee { get; set; }
        public decimal HrCostAsPercentOfRevenue { get; set; }
        public decimal SpanOfControlEfficiency { get; set; }
        public decimal ProductivityCostRatio { get; set; }
    }
}