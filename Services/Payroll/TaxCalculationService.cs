using HRManagement.DTOs.Payroll;
using System;

namespace HRManagement.Services.Payroll
{
    public class TaxCalculationService
    {
        // (Min, Max, Rate, QuickDeduction) — công thức tính nhanh theo quy định Việt Nam
        private static readonly (decimal Min, decimal Max, decimal Rate, decimal Quick)[] Brackets =
        {
            (0,           5_000_000,          0.05m,  0),
            (5_000_000,   10_000_000,         0.10m,  250_000),
            (10_000_000,  18_000_000,         0.15m,  750_000),
            (18_000_000,  32_000_000,         0.20m,  1_650_000),
            (32_000_000,  52_000_000,         0.25m,  3_250_000),
            (52_000_000,  80_000_000,         0.30m,  5_850_000),
            (80_000_000,  decimal.MaxValue,   0.35m,  9_850_000),
        };

        private const decimal PersonalDeduction  = 11_000_000m;
        private const decimal DependentDeduction = 4_400_000m;
        private const decimal InsuranceRate      = 0.105m;      // 8% (BHXH) + 1.5% (BHYT) + 1% (BHTN)
        private const decimal InsuranceCap       = 46_800_000m; // Mức trần đóng BH (20 lần lương cơ sở)

        public TaxCalculationResultDto Calculate(
            decimal grossIncome,
            int numberOfDependents,
            bool isInsuranceApplicable = true)
        {
            // 1. Tính bảo hiểm bắt buộc
            var insurance = isInsuranceApplicable
                ? Math.Min(grossIncome, InsuranceCap) * InsuranceRate
                : 0m;

            // 2. Tính giảm trừ người phụ thuộc
            var dependent = DependentDeduction * numberOfDependents;

            // 3. Tính thu nhập tính thuế (TNTT)
            var taxableIncome = grossIncome
                - insurance
                - PersonalDeduction
                - dependent;

            taxableIncome = Math.Max(taxableIncome, 0m);

            // 4. Tính thuế theo biểu lũy tiến
            var (tax, bracket) = CalculateByBracket(taxableIncome);

            return new TaxCalculationResultDto
            {
                GrossIncome          = grossIncome,
                InsuranceDeduction   = insurance,
                PersonalDeduction    = PersonalDeduction,
                DependentDeduction   = dependent,
                TaxableIncome        = taxableIncome,
                TaxAmount            = Math.Round(tax, 0),
                TaxBracket           = bracket,
                EffectiveTaxRate     = grossIncome > 0
                    ? Math.Round(tax / grossIncome * 100, 2)
                    : 0m,
            };
        }

        private (decimal Tax, int Bracket) CalculateByBracket(decimal taxableIncome)
        {
            for (int i = 0; i < Brackets.Length; i++)
            {
                var (min, max, rate, quick) = Brackets[i];
                if (taxableIncome <= max)
                    return (taxableIncome * rate - quick, i + 1);
            }
            return (taxableIncome * 0.35m - 9_850_000m, 7);
        }
    }
}
