namespace HRManagement.DTOs.Attendances
{
    public class AttendanceDetailResponseDto
    {
        public AttendanceResponseDto Attendance { get; set; } = new();
        public List<AttendanceLogResponseDto> Logs { get; set; } = new();
    }
}
