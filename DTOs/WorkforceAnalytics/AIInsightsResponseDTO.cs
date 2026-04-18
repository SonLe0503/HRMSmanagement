namespace HRManagement.DTOs.WorkforceAnalytics
{
    public class AIInsightsResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<object> AttritionRisks { get; set; } = new();
        public List<object> HeadcountRecommendations { get; set; } = new();
        public List<object> HiringForecasts { get; set; } = new();
        public List<object> RetentionSuggestions { get; set; } = new();
    }
}
