namespace HRManagement.DTOs.CostAnalysis
{
    public class CostScenarioResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal CurrentCost { get; set; }
        public decimal ScenarioCost { get; set; }
        public decimal CostDifference { get; set; }
        public List<object> Breakdown { get; set; } = new();
    }
}