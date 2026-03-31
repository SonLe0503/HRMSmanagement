namespace HRManagement.DTOs.LeaveTypes
{
    public class LeaveTypeDTO
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = null!;
        public string LeaveTypeName { get; set; } = null!;
        public int AnnualEntitlement { get; set; }
        public bool IsPaid { get; set; }
        public bool RequiresApproval { get; set; }
        public bool IsCarryForward { get; set; }
        public int? MaxCarryForwardDays { get; set; }
        public bool IsActive { get; set; }
    }
}
