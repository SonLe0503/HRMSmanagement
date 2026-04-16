using System;
using System.Collections.Generic;

namespace HRManagement.DTOs
{
    public class ManagerDashboardDto
    {
        public ManagerStatisticsDto Statistics { get; set; } = new();
        public List<PendingRequestDto> PendingLeaveRequests { get; set; } = new();
        public List<UpcomingLeaveDto> UpcomingTeamLeaves { get; set; } = new();
        public TaskPerformanceDto TaskPerformance { get; set; } = new();
        public RequestActionSummaryDto ActionSummary { get; set; } = new();
        public List<TeamInsightDto> TeamInsights { get; set; } = new();
        public List<RecentActivityDto> RecentTeamActivities { get; set; } = new();
        public List<UpcomingEventDto> TeamMilestones { get; set; } = new();
    }

    public class ManagerStatisticsDto
    {
        public int TeamSize { get; set; }
        public int PresentToday { get; set; }
        public int OnLeaveToday { get; set; }
        public double TeamAttendanceRate { get; set; }
    }

    public class PendingRequestDto
    {
        public int RequestId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public string LeaveType { get; set; } = null!;
        public decimal Days { get; set; }
    }

    public class UpcomingLeaveDto
    {
        public string EmployeeName { get; set; } = null!;
        public string LeaveType { get; set; } = null!;
        public string DateRange { get; set; } = null!;
    }

    public class TaskPerformanceDto
    {
        public int ActiveTasks { get; set; }
        public int OverdueTasks { get; set; }
        public double CompletionRate { get; set; }
        public int PendingEvaluations { get; set; }
    }

    public class RequestActionSummaryDto
    {
        public int PendingLeaveApprovals { get; set; }
        public int PendingOvertimeApprovals { get; set; }
        public int PendingAttendanceCorrections { get; set; }
        public int TotalPendingApprovals { get; set; }
    }

    public class TeamInsightDto
    {
        public string Label { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
