namespace HRManagement.DTOs
{
    public class AdjustLeaveBalanceResponseDTO
    {
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;

        public decimal OldUsedDays { get; set; }
        public decimal OldRemainingBalance { get; set; }

        public decimal NewUsedDays { get; set; }
        public decimal NewRemainingBalance { get; set; }

        public string AdjustmentType { get; set; } = string.Empty;
        public decimal NumberOfDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateOnly EffectiveDate { get; set; }

        public string? MessageCode { get; set; }
        public string? Message { get; set; }
    }
}
