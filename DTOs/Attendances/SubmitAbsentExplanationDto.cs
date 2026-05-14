using System;

namespace HRManagement.DTOs.Attendances
{
    public class SubmitAbsentExplanationDto
    {
        public DateOnly Date { get; set; }
        public string Message { get; set; } = null!;
        public string? ExplanationType { get; set; }
        public int? LeaveTypeId { get; set; }
        public TimeSpan? RequestedCheckInTime { get; set; }
        public TimeSpan? RequestedCheckOutTime { get; set; }
    }
}
