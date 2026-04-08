namespace HRManagement.DTOs.ShiftAssignments
{
    public class ShiftAssignmentResponseDto
    {
        public int AssignmentId { get; set; }

        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        public int ShiftId { get; set; }
        public string ShiftCode { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;

        public DateOnly AssignmentDate { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public string? RecurrencePattern { get; set; }
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }

    }
}
