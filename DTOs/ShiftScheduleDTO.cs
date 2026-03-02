namespace HRManagement.DTOs;

public class ShiftScheduleDTO
{
    public DateOnly Date { get; set; }
    public string ShiftName { get; set; } = null!;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = null!;
}