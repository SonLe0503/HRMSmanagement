namespace HRManagement.DTOs.CostAnalysis
{
    public class CostAlertDTO
    {
        public string CostCategory { get; set; } = string.Empty;
        public decimal Threshold { get; set; }
        public string ThresholdType { get; set; } = "absolute"; // absolute, percentage
        public string AlertFrequency { get; set; } = "weekly";
        public List<string> Recipients { get; set; } = new();
    }
}