namespace HRManagement.DTOs
{
    public class AdjustLeaveBalanceDTO
    {
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }

        public string AdjustmentType { get; set; } = null!;

        public decimal Days { get; set; }

        public string Reason { get; set; } = null!;

        public DateTime EffectiveDate { get; set; }
    }
}
