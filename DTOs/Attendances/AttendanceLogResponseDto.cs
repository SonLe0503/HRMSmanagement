namespace HRManagement.DTOs.Attendances
{
    public class AttendanceLogResponseDto
    {
        public int LogId { get; set; }
        public int EmployeeId { get; set; }
        public int? ShiftId { get; set; }
        public DateTime LogTime { get; set; }
        public string LogType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? Location { get; set; }
        public string? Remarks { get; set; }
    }
}
