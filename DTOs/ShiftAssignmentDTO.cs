namespace HRManagement.DTOs;

public class ShiftAssignmentDTO
{
    public int AssignmentId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = null!; // For the table view
    public int ShiftId { get; set; }
    public string ShiftName { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Status { get; set; } = null!;
}

public class CreateShiftAssignmentDTO
{
    public int EmployeeId { get; set; }
    public int ShiftId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; } // Can be null for permanent assignments
}