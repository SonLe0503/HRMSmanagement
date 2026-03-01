namespace HRManagement.DTOs
{
    public class PayrollPolicyListDTO
    {
        public int PolicyId { get; set; }
        public string PolicyName { get; set; } = null!;
        public string PolicyType { get; set; } = null!;
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
    public class PayrollPolicyDetailDTO
    {
        public int PolicyId { get; set; }
        public string PolicyName { get; set; } = null!;
        public string PolicyType { get; set; } = null!;
        public string? Description { get; set; }
        public string? CalculationFormula { get; set; }
        public string? ApplicableEmployeeGroup { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }
        public bool IsActive { get; set; }
    }
    public class CreatePayrollPolicyDTO
    {
        public string PolicyName { get; set; } = null!;
        public string PolicyType { get; set; } = null!;
        public string? Description { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }
        public string? ApplicableEmployeeGroup { get; set; }
        public string? CalculationFormula { get; set; }
    }
}
