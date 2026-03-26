namespace HRManagement.DTOs.LeaveRequest
{
    public class LeaveTypeDTO
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
    }
}
