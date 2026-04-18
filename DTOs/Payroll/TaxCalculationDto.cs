using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.Payroll
{
    public class TaxCalculationRequestDto
    {
        [Range(0, double.MaxValue)]
        public decimal GrossIncome { get; set; }

        [Range(0, 20)]
        public int NumberOfDependents { get; set; }

        public bool IsInsuranceApplicable { get; set; } = true;
    }

    public class TaxCalculationResultDto
    {
        public decimal GrossIncome { get; set; }
        public decimal InsuranceDeduction { get; set; }
        public decimal PersonalDeduction { get; set; }      // 11,000,000
        public decimal DependentDeduction { get; set; }     // 4,400,000 × số người
        public decimal TaxableIncome { get; set; }
        public decimal TaxAmount { get; set; }
        public int TaxBracket { get; set; }                 // Bậc thuế (1-7)
        public decimal EffectiveTaxRate { get; set; }       // %
    }
}
