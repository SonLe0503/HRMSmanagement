namespace HRManagement.DTOs.LeaveBalance
{
    public class LeaveBalanceListDTO
    {
        public int BalanceId { get; set; }

        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public int LeaveTypeId { get; set; }

        public string LeaveTypeName { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal TotalEntitlement { get; set; }

        public decimal UsedDays { get; set; }

        public decimal RemainingDays { get; set; }

        public decimal CarriedForward { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
