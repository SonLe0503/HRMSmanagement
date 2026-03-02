public class AdminAttendanceDTO
{
    public int AttendanceId { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
    public string Date { get; set; } = null!;
    public string ShiftName { get; set; } = null!;
    public string? CheckIn { get; set; }
    public string? CheckOut { get; set; }
    public string Status { get; set; } = null!;
    public int? LateMinutes { get; set; }
}