namespace HRManagement.DTOs.CostAnalysis
{
    public class CostScenarioDTO
    {
        public int HeadcountChange { get; set; }
        public decimal SalaryAdjustmentPercent { get; set; }
        public decimal BenefitsAdjustmentPercent { get; set; }
        public decimal OtherCostAdjustmentPercent { get; set; }
    }
}