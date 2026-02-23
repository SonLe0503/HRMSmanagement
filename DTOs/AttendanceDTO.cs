namespace HRManagement.DTOs;

public class CheckInRequestDTO
{
    public int EmployeeId { get; set; }
    public string? Location { get; set; } // Captured from Browser/GPS
    public string? Remarks { get; set; }
}

public class AttendanceResponseDTO
{
    public int AttendanceId { get; set; }
    public DateOnly AttendanceDate { get; set; }
    public DateTime? CheckInTime { get; set; }
    public string Status { get; set; } = null!; // Present, Late, etc.
    public int? LateMinutes { get; set; }
    public string Message { get; set; } = null!; // Using our MSG-SUC-04 code
}

public class AttendanceHistoryDTO
{
    public DateOnly Date { get; set; }
    public string ShiftName { get; set; } = null!;
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public decimal? TotalHours { get; set; }
    public string Status { get; set; } = null!;
}