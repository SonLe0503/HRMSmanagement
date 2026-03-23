namespace HRManagement.DTOs.Shifts
{
    public class UpdateShiftDto
    {
        public string ShiftCode { get; set; } = null!;
        public string ShiftName { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int WorkingHours { get; set; }
        public string ShiftType { get; set; } = null!;

        public int? LateGraceMinutes { get; set; }
        public int? EarlyCheckInMinutes { get; set; }
        public int? LatestCheckInMinutes { get; set; }
        public int? LatestCheckOutMinutes { get; set; }
        public bool? IsOvernight { get; set; }
        public bool IsActive { get; set; }
    }
}
