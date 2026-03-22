namespace HRManagement.DTOs.Attendances
{
    public class ManualCreateAttendanceDto
    {
        public int EmployeeId { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public int? ShiftId { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = "Present";
        public string? Remarks { get; set; }
    }
}
