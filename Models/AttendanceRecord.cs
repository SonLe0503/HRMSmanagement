using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class AttendanceRecord
{
    public int AttendanceId { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public int? ShiftId { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public decimal? WorkingHours { get; set; }

    public decimal? OvertimeHours { get; set; }

    public int? LateMinutes { get; set; }

    public int? EarlyLeaveMinutes { get; set; }

    public string Status { get; set; } = null!;

    // NEW
    public string? Source { get; set; }

    // NEW
    public bool? IsManualAdjusted { get; set; }

    // NEW
    public bool? IsLocked { get; set; }

    // NEW
    public int? ApprovedBy { get; set; }

    // NEW
    public DateTime? ApprovedDate { get; set; }

    public string? Location { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Shift? Shift { get; set; }
}
