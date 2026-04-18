namespace HRManagement.DTOs.Attendances
{
    public class ApproveExplanationDto
    {
        public bool IsApproved { get; set; }
        public string? Response { get; set; }
        public TimeSpan? ManualCheckInTime { get; set; }
        public TimeSpan? ManualCheckOutTime { get; set; }
    }
}
