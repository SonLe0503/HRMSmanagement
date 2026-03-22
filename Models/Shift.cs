using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class Shift
{
    public int ShiftId { get; set; }

    public string ShiftCode { get; set; } = null!;

    public string ShiftName { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int WorkingHours { get; set; }

    public string ShiftType { get; set; } = null!;

    // NEW
    public int? LateGraceMinutes { get; set; }

    // NEW
    public int? EarlyCheckInMinutes { get; set; }

    // NEW
    public int? LatestCheckInMinutes { get; set; }

    // NEW
    public int? LatestCheckOutMinutes { get; set; }

    // NEW
    public bool? IsOvernight { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();

    public virtual ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
}
