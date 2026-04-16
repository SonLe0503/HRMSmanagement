using System;
using System.Collections.Generic;

namespace HRManagement.DTOs
{
    public class HrDashboardDto
    {
        public HrStatisticsDto Statistics { get; set; } = new();
        public List<UpcomingEventDto> UpcomingProbationEnds { get; set; } = new();
        public List<UpcomingEventDto> ContractRenewals { get; set; } = new();
        public List<UpcomingEventDto> Birthdays { get; set; } = new();
        public List<RecentActivityDto> RecentHrActivities { get; set; } = new();
    }

    public class HrStatisticsDto
    {
        public int TotalHeadcount { get; set; }
        public int NewHires { get; set; }
        public int Terminations { get; set; }
        public double OverallAttendanceRate { get; set; }
        public double AverageLeaveDays { get; set; }
        public int PendingLeaveRequests { get; set; }
        public int PendingEvaluations { get; set; }
        public int CompletedEvaluations { get; set; }
        public double AveragePerformanceScore { get; set; }
    }

    public class UpcomingEventDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public DateOnly Date { get; set; }
        public string? Detail { get; set; }
    }
}
