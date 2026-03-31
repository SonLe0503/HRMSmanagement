namespace HRManagement.DTOs.Attendances
{
    public class AssignShiftDto
    {
        public int EmployeeId { get; set; }
        public int ShiftId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        // optional: nếu muốn chọn kiểu lặp
        // Daily = mỗi ngày
        // Weekly = chỉ theo các thứ được chọn trong tuần
        public string AssignType { get; set; } = "Daily";

        // dùng cho Weekly: ví dụ [1,2,3,4,5] = T2 -> T6
        public List<int>? DaysOfWeek { get; set; }
    }
}
