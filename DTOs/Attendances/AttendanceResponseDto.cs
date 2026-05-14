namespace HRManagement.DTOs.Attendances
{
    public class AttendanceResponseDto
    {
        public int AttendanceId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly AttendanceDate { get; set; }
        public int? ShiftId { get; set; }
        public string? ShiftName { get; set; }

        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        public decimal? WorkingHours { get; set; }
        public decimal? OvertimeHours { get; set; }
        public decimal? ActualOvertimeHours { get; set; }
        public decimal? ApprovedOvertimeHours { get; set; }
        public decimal? PayrollOvertimeHours { get; set; }
        public int? LateMinutes { get; set; }
        public int? EarlyLeaveMinutes { get; set; }

        public string Status { get; set; } = string.Empty;
        public string? Source { get; set; }
        public bool? IsManualAdjusted { get; set; }
        public bool? IsLocked { get; set; }

        public string? Location { get; set; }
        public string? Remarks { get; set; }

        public string? ExplanationMessage { get; set; }
        public string? ExplanationStatus { get; set; }
        public string? ExplanationResponse { get; set; }
        public string? ExplanationType { get; set; }
        public int? ExplanationLeaveTypeId { get; set; }
        public string? ExplanationLeaveTypeName { get; set; }
        public TimeSpan? ExplanationRequestedCheckInTime { get; set; }
        public TimeSpan? ExplanationRequestedCheckOutTime { get; set; }
        public TimeSpan? ShiftStartTime { get; set; }
        public TimeSpan? ShiftEndTime { get; set; }
        public bool? ShiftIsOvernight { get; set; }
        public DateTime? AllowedCheckInFrom { get; set; }
        public DateTime? AllowedCheckInTo { get; set; }
        public DateTime? AllowedCheckOutFrom { get; set; }
        public DateTime? AllowedCheckOutTo { get; set; }
    }
}
