namespace HRManagement.DTOs.LeaveBalance
{
    public class GenerateBalanceResultDTO
    {
        public int Year { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalLeaveTypes { get; set; }
        public int Created { get; set; }
        public int Skipped { get; set; }
        public int CarriedForward { get; set; }
    }
}
