namespace HRManagement.DTOs.Attendances
{
    public class ManualAdjustAttendanceDto
    {
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = "Adjusted";
        public string? Remarks { get; set; }
    }
}
