namespace HRManagement.DTOs.ShiftAssignments
{
    public class UpdateShiftAssignmentDto
    {
        public int ShiftId { get; set; }
        public DateOnly AssignmentDate { get; set; }
        public string Status { get; set; } = "Active";
    }
}
