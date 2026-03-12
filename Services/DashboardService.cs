using System.Text.Json;
using HRManagement.DTOs.Dashboard;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly HrmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public DashboardService(
            HrmsDbContext context,
            ICurrentUserService currentUserService,
            IAuditService auditService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        public async Task<DashboardResponseDTO> GetDashboardAsync()
        {
            int userId = _currentUserService.GetUserId();
            string role = _currentUserService.GetRole();

            var widgets = role switch
            {
                "Employee" => await BuildEmployeeWidgetsAsync(userId),
                "Manager" => await BuildManagerWidgetsAsync(userId),
                "HRStaff" => await BuildHRWidgetsAsync(),
                "SystemAdministrator" => await BuildAdminWidgetsAsync(),
                _ => new List<DashboardWidgetDTO>()
            };

            string layoutKey = GetLayoutSettingKey(userId);
            string refreshKey = GetRefreshSettingKey(userId);

            var layoutSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(x => x.SettingKey == layoutKey);

            var refreshSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(x => x.SettingKey == refreshKey);

            int refreshInterval = 300;
            if (refreshSetting != null && int.TryParse(refreshSetting.SettingValue, out int parsedInterval))
            {
                refreshInterval = parsedInterval;
            }

            await _auditService.TrackAsync(userId, "SELECT", "User viewed dashboard");

            return new DashboardResponseDTO
            {
                Role = role,
                HomeScreenCode = "SR-18",
                LastRefreshed = DateTime.UtcNow,
                RefreshIntervalSeconds = refreshInterval,
                CanCustomizeLayout = true,
                Widgets = ApplySavedLayout(widgets, layoutSetting?.SettingValue)
            };
        }

        public async Task<DashboardResponseDTO> RefreshDashboardAsync(RefreshDashboardDTO request)
        {
            var dashboard = await GetDashboardAsync();

            if (!string.IsNullOrWhiteSpace(request.WidgetKey))
            {
                dashboard.Widgets = dashboard.Widgets
                    .Where(x => x.Key == request.WidgetKey)
                    .ToList();
            }

            dashboard.LastRefreshed = DateTime.UtcNow;
            return dashboard;
        }

        public async Task<bool> SaveLayoutAsync(DashboardLayoutUpdateDTO request)
        {
            int userId = _currentUserService.GetUserId();
            string key = GetLayoutSettingKey(userId);
            string layoutJson = JsonSerializer.Serialize(request.Widgets);

            var existing = await _context.SystemSettings
                .FirstOrDefaultAsync(x => x.SettingKey == key);

            if (existing == null)
            {
                existing = new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = layoutJson,
                    SettingCategory = "General",
                    Description = $"Dashboard layout for user {userId}",
                    ModifiedDate = DateTime.UtcNow,
                    ModifiedBy = userId
                };

                _context.SystemSettings.Add(existing);
            }
            else
            {
                existing.SettingValue = layoutJson;
                existing.ModifiedDate = DateTime.UtcNow;
                existing.ModifiedBy = userId;

                _context.SystemSettings.Update(existing);
            }

            await _context.SaveChangesAsync();
            await _auditService.TrackAsync(userId, "UPDATE", "User saved dashboard layout");

            return true;
        }

        public async Task<RetryWidgetResponseDTO> RetryWidgetAsync(string widgetKey)
        {
            var dashboard = await GetDashboardAsync();
            var widget = dashboard.Widgets.FirstOrDefault(x => x.Key == widgetKey);

            if (widget == null)
            {
                return new RetryWidgetResponseDTO
                {
                    WidgetKey = widgetKey,
                    Success = false,
                    Message = "MSG-79: Widget reload failed because widget was not found."
                };
            }

            widget.HasError = false;
            widget.ErrorMessage = null;
            widget.LastUpdated = DateTime.UtcNow;

            await _auditService.TrackAsync(_currentUserService.GetUserId(), "SELECT", $"User retried widget {widgetKey}");

            return new RetryWidgetResponseDTO
            {
                WidgetKey = widgetKey,
                Success = true,
                Message = "Widget reloaded successfully.",
                Widget = widget
            };
        }

        public Task<WidgetDetailResponseDTO> GetWidgetDetailsAsync(string widgetKey)
        {
            var role = _currentUserService.GetRole();

            var response = new WidgetDetailResponseDTO
            {
                WidgetKey = widgetKey,
                ReportName = $"{widgetKey} details",
                RedirectUrl = $"/reports/{widgetKey}",
                Filters = new Dictionary<string, string>
                {
                    { "role", role },
                    { "period", "current" }
                }
            };

            return System.Threading.Tasks.Task.FromResult(response);
        }

        private async Task<List<DashboardWidgetDTO>> BuildEmployeeWidgetsAsync(int userId)
        {
            var employeeId = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.EmployeeId)
                .FirstOrDefaultAsync();

            if (employeeId == null)
                return new List<DashboardWidgetDTO>();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId.Value);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currentYear = DateTime.UtcNow.Year;

            var profileWidget = await SafeLoadWidgetAsync(
                "profile-summary",
                "Personal Profile Summary",
                "Basic employee information",
                "table",
                async () => new
                {
                    employee?.EmployeeCode,
                    employee?.FullName,
                    Department = employee?.Department?.DepartmentName,
                    Position = employee?.Position?.PositionName,
                    employee?.EmploymentStatus
                });

            var scheduleWidget = await SafeLoadWidgetAsync(
                "upcoming-schedule",
                "Upcoming Schedule and Shifts",
                "Next assigned shifts",
                "list",
                async () =>
                {
                    var data = await (
                        from sa in _context.ShiftAssignments
                        join s in _context.Shifts on sa.ShiftId equals s.ShiftId
                        where sa.EmployeeId == employeeId.Value
                              && sa.StartDate >= today
                              && sa.Status == "Active"
                        orderby sa.StartDate
                        select new
                        {
                            sa.StartDate,
                            s.ShiftName,
                            s.StartTime,
                            s.EndTime
                        }
                    ).Take(5).ToListAsync();

                    return data;
                });

            var leaveBalanceWidget = await SafeLoadWidgetAsync(
                "leave-balance",
                "Leave Balance Summary",
                "Current leave balances",
                "table",
                async () =>
                {
                    var data = await _context.LeaveBalances
                        .Where(lb => lb.EmployeeId == employeeId.Value && lb.Year == currentYear)
                        .Select(lb => new
                        {
                            lb.LeaveTypeId,
                            lb.TotalEntitlement,
                            lb.UsedDays,
                            lb.RemainingDays
                        })
                        .ToListAsync();

                    return data;
                });

            var pendingRequestWidget = await SafeLoadWidgetAsync(
                "pending-requests",
                "Pending Requests Status",
                "Pending leave and overtime requests",
                "number",
                async () =>
                {
                    var pendingLeaveCount = await _context.LeaveRequests
                        .CountAsync(x => x.EmployeeId == employeeId.Value && x.Status == "Pending");

                    var pendingOtCount = await _context.OvertimeRequests
                        .CountAsync(x => x.EmployeeId == employeeId.Value && x.Status == "Pending");

                    return new
                    {
                        PendingLeaveRequests = pendingLeaveCount,
                        PendingOvertimeRequests = pendingOtCount,
                        TotalPending = pendingLeaveCount + pendingOtCount
                    };
                });

            var payslipWidget = await SafeLoadWidgetAsync(
                "recent-payslip",
                "Recent Payslip Summary",
                "Latest payroll snapshot",
                "table",
                async () =>
                {
                    var data = await (
                        from p in _context.Payslips
                        join pr in _context.PayrollRecords on p.PayrollRecordId equals pr.PayrollRecordId
                        join pp in _context.PayrollPeriods on p.PeriodId equals pp.PeriodId
                        where p.EmployeeId == employeeId.Value
                        orderby pp.Year descending, pp.Month descending
                        select new
                        {
                            pp.Month,
                            pp.Year,
                            pr.NetPay,
                            p.IsViewed
                        }
                    ).FirstOrDefaultAsync();

                    return data;
                });

            var evaluationWidget = await SafeLoadWidgetAsync(
                "evaluation-deadlines",
                "Upcoming Evaluation Deadlines",
                "Upcoming performance deadlines",
                "list",
                async () =>
                {
                    var data = await (
                        from ev in _context.Evaluations
                        join ec in _context.EvaluationCycles on ev.CycleId equals ec.CycleId
                        where ev.EmployeeId == employeeId.Value
                              && ec.ManagerEvaluationEnd >= today
                        orderby ec.ManagerEvaluationEnd
                        select new
                        {
                            ec.CycleName,
                            ec.ManagerEvaluationEnd,
                            ev.Status
                        }
                    ).Take(5).ToListAsync();

                    return data;
                });

            var notificationWidget = await SafeLoadWidgetAsync(
                "announcements",
                "Announcements and Notifications",
                "Latest notifications",
                "list",
                async () =>
                {
                    var data = await _context.Notifications
                        .Where(n => n.RecipientUserId == userId)
                        .OrderByDescending(n => n.SentDate)
                        .Take(5)
                        .Select(n => new
                        {
                            n.Title,
                            n.Message,
                            n.SentDate,
                            n.IsRead
                        })
                        .ToListAsync();

                    return data;
                });

            var quickActionWidget = CreateWidget(
                "quick-actions",
                "Quick Actions",
                "Frequently used actions",
                "list",
                new[]
                {
                    "Submit Leave Request",
                    "View Payslip",
                    "View Attendance"
                });

            return new List<DashboardWidgetDTO>
            {
                profileWidget,
                scheduleWidget,
                leaveBalanceWidget,
                pendingRequestWidget,
                payslipWidget,
                evaluationWidget,
                notificationWidget,
                quickActionWidget
            };
        }

        private async Task<List<DashboardWidgetDTO>> BuildManagerWidgetsAsync(int userId)
        {
            var managerEmployeeId = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.EmployeeId)
                .FirstOrDefaultAsync();

            if (managerEmployeeId == null)
                return new List<DashboardWidgetDTO>();

            var teamEmployeeIds = await _context.Employees
                .Where(e => e.ManagerId == managerEmployeeId.Value && e.EmploymentStatus == "Active")
                .Select(e => e.EmployeeId)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var teamOverviewWidget = await SafeLoadWidgetAsync(
                "team-overview",
                "Team Overview",
                "Team headcount and attendance rate",
                "chart",
                async () =>
                {
                    int headcount = teamEmployeeIds.Count;

                    var todayAttendance = await _context.AttendanceRecords
                        .Where(a => teamEmployeeIds.Contains(a.EmployeeId) && a.AttendanceDate == today)
                        .ToListAsync();

                    int presentCount = todayAttendance.Count(x => x.Status == "Present" || x.Status == "Late");
                    decimal attendanceRate = headcount == 0 ? 0 : (decimal)presentCount / headcount * 100;

                    return new
                    {
                        Headcount = headcount,
                        AttendanceRate = Math.Round(attendanceRate, 2)
                    };
                });

            var pendingApprovalWidget = await SafeLoadWidgetAsync(
                "pending-approvals",
                "Pending Approvals Count",
                "Pending leave and overtime approvals",
                "number",
                async () =>
                {
                    var pendingLeaveApprovals = await _context.LeaveRequests
                        .Where(x => teamEmployeeIds.Contains(x.EmployeeId) && x.Status == "Pending")
                        .CountAsync();

                    var pendingOtApprovals = await _context.OvertimeRequests
                        .Where(x => teamEmployeeIds.Contains(x.EmployeeId) && x.Status == "Pending")
                        .CountAsync();

                    return new
                    {
                        PendingLeaveApprovals = pendingLeaveApprovals,
                        PendingOvertimeApprovals = pendingOtApprovals,
                        TotalPending = pendingLeaveApprovals + pendingOtApprovals
                    };
                });

            var attendanceWidget = await SafeLoadWidgetAsync(
                "team-attendance",
                "Team Attendance Summary",
                "Attendance summary for today",
                "chart",
                async () =>
                {
                    int headcount = teamEmployeeIds.Count;

                    var todayAttendance = await _context.AttendanceRecords
                        .Where(a => teamEmployeeIds.Contains(a.EmployeeId) && a.AttendanceDate == today)
                        .ToListAsync();

                    int presentCount = todayAttendance.Count(x => x.Status == "Present" || x.Status == "Late");

                    return new
                    {
                        Present = presentCount,
                        Absent = Math.Max(0, headcount - presentCount)
                    };
                });

            var performanceWidget = await SafeLoadWidgetAsync(
                "team-performance",
                "Team Performance Metrics",
                "Evaluation summary",
                "chart",
                async () =>
                {
                    var totalEvaluations = await _context.Evaluations
                        .CountAsync(e => teamEmployeeIds.Contains(e.EmployeeId));

                    var completedEvaluations = await _context.Evaluations
                        .CountAsync(e => teamEmployeeIds.Contains(e.EmployeeId) && e.Status == "Completed");

                    return new
                    {
                        TotalEvaluations = totalEvaluations,
                        CompletedEvaluations = completedEvaluations
                    };
                });

            var teamEvaluationWidget = await SafeLoadWidgetAsync(
                "team-evaluations",
                "Upcoming Team Evaluations",
                "Upcoming evaluation deadlines",
                "list",
                async () =>
                {
                    var data = await (
                        from ev in _context.Evaluations
                        join ec in _context.EvaluationCycles on ev.CycleId equals ec.CycleId
                        where teamEmployeeIds.Contains(ev.EmployeeId)
                              && ec.ManagerEvaluationEnd >= today
                        orderby ec.ManagerEvaluationEnd
                        select new
                        {
                            ev.EmployeeId,
                            ec.CycleName,
                            ec.ManagerEvaluationEnd,
                            ev.Status
                        }
                    ).Take(10).ToListAsync();

                    return data;
                });

            var leaveCalendarWidget = await SafeLoadWidgetAsync(
                "leave-calendar",
                "Leave Calendar Visualization",
                "Approved upcoming leaves",
                "calendar",
                async () =>
                {
                    var data = await (
                        from lr in _context.LeaveRequests
                        join e in _context.Employees on lr.EmployeeId equals e.EmployeeId
                        where teamEmployeeIds.Contains(lr.EmployeeId)
                              && lr.Status == "Approved"
                              && lr.StartDate >= today
                        orderby lr.StartDate
                        select new
                        {
                            e.FullName,
                            lr.StartDate,
                            lr.EndDate,
                            lr.NumberOfDays
                        }
                    ).Take(10).ToListAsync();

                    return data;
                });

            var directReportsWidget = await SafeLoadWidgetAsync(
                "direct-reports",
                "Direct Reports List",
                "Employees reporting to current manager",
                "table",
                async () =>
                {
                    var data = await _context.Employees
                        .Where(e => teamEmployeeIds.Contains(e.EmployeeId))
                        .Select(e => new
                        {
                            e.EmployeeId,
                            e.EmployeeCode,
                            e.FullName,
                            e.EmploymentStatus
                        })
                        .ToListAsync();

                    return data;
                });

            var criticalAlertWidget = await SafeLoadWidgetAsync(
                "critical-alerts",
                "Critical Alerts",
                "Overdue or pending critical items",
                "list",
                async () =>
                {
                    var pendingLeaveApprovals = await _context.LeaveRequests
                        .Where(x => teamEmployeeIds.Contains(x.EmployeeId) && x.Status == "Pending")
                        .CountAsync();

                    var pendingOtApprovals = await _context.OvertimeRequests
                        .Where(x => teamEmployeeIds.Contains(x.EmployeeId) && x.Status == "Pending")
                        .CountAsync();

                    return new[]
                    {
                        $"Pending approvals: {pendingLeaveApprovals + pendingOtApprovals}"
                    };
                });

            return new List<DashboardWidgetDTO>
            {
                teamOverviewWidget,
                pendingApprovalWidget,
                attendanceWidget,
                performanceWidget,
                teamEvaluationWidget,
                leaveCalendarWidget,
                directReportsWidget,
                criticalAlertWidget
            };
        }

        private async Task<List<DashboardWidgetDTO>> BuildHRWidgetsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var currentMonth = today.Month;
            var currentYear = today.Year;

            var headcountWidget = await SafeLoadWidgetAsync(
                "org-headcount",
                "Organization Headcount and Trends",
                "Current active employees",
                "chart",
                async () =>
                {
                    int totalHeadcount = await _context.Employees
                        .CountAsync(e => e.EmploymentStatus == "Active");

                    return new
                    {
                        TotalHeadcount = totalHeadcount
                    };
                });

            var recruitmentWidget = CreateWidget(
                "recruitment-pipeline",
                "Recruitment Pipeline Summary",
                "Recruitment pipeline is not modeled in current database schema",
                "number",
                new
                {
                    Message = "No dedicated recruitment tables in current schema."
                });

            var attendanceLeaveWidget = await SafeLoadWidgetAsync(
                "attendance-leave-stats",
                "Attendance and Leave Statistics",
                "Organization leave overview",
                "chart",
                async () =>
                {
                    var leaveStats = await _context.LeaveRequests
                        .Where(l => l.StartDate.Year == currentYear)
                        .GroupBy(l => l.Status)
                        .Select(g => new
                        {
                            Status = g.Key,
                            Count = g.Count()
                        })
                        .ToListAsync();

                    return leaveStats;
                });

            var payrollStatusWidget = await SafeLoadWidgetAsync(
                "payroll-status",
                "Payroll Processing Status",
                "Current payroll run status",
                "chart",
                async () =>
                {
                    var data = await (
                        from pr in _context.PayrollRecords
                        join pp in _context.PayrollPeriods on pr.PeriodId equals pp.PeriodId
                        where pp.Month == currentMonth && pp.Year == currentYear
                        group pr by pr.Status into g
                        select new
                        {
                            Status = g.Key,
                            Count = g.Count()
                        }
                    ).ToListAsync();

                    return data;
                });

            var lifecycleWidget = await SafeLoadWidgetAsync(
                "lifecycle-metrics",
                "Employee Lifecycle Metrics",
                "New hires and terminations",
                "chart",
                async () =>
                {
                    int newHires = await _context.Employees
                        .CountAsync(e => e.JoinDate.Month == currentMonth && e.JoinDate.Year == currentYear);

                    int terminations = await _context.Employees
                        .CountAsync(e =>
                            e.ResignationDate.HasValue &&
                            e.ResignationDate.Value.Month == currentMonth &&
                            e.ResignationDate.Value.Year == currentYear);

                    return new
                    {
                        NewHires = newHires,
                        Terminations = terminations
                    };
                });

            var complianceWidget = CreateWidget(
                "compliance-alerts",
                "Compliance Alerts and Deadlines",
                "Notifications and reminders",
                "list",
                new[]
                {
                    "Review expiring contracts",
                    "Review pending evaluations"
                });

            var evaluationProgressWidget = await SafeLoadWidgetAsync(
                "evaluation-progress",
                "Performance Evaluation Progress",
                "Evaluation status summary",
                "chart",
                async () =>
                {
                    var data = await _context.Evaluations
                        .GroupBy(e => e.Status)
                        .Select(g => new
                        {
                            Status = g.Key,
                            Count = g.Count()
                        })
                        .ToListAsync();

                    return data;
                });

            var costAnalysisWidget = await SafeLoadWidgetAsync(
                "cost-analysis",
                "Cost Analysis Overview",
                "Payroll cost overview",
                "chart",
                async () =>
                {
                    decimal totalPayrollCost = await _context.PayrollRecords
                        .SumAsync(x => (decimal?)x.NetPay) ?? 0;

                    return new
                    {
                        TotalPayrollCost = totalPayrollCost
                    };
                });

            return new List<DashboardWidgetDTO>
            {
                headcountWidget,
                recruitmentWidget,
                attendanceLeaveWidget,
                payrollStatusWidget,
                lifecycleWidget,
                complianceWidget,
                evaluationProgressWidget,
                costAnalysisWidget
            };
        }

        private async Task<List<DashboardWidgetDTO>> BuildAdminWidgetsAsync()
        {
            var activeUsersWidget = await SafeLoadWidgetAsync(
                "active-users",
                "Active Users Count",
                "Currently active users in system",
                "number",
                async () =>
                {
                    int count = await _context.Users.CountAsync(u => u.IsActive);
                    return new { Count = count };
                });

            var recentActivitiesWidget = await SafeLoadWidgetAsync(
                "recent-activities",
                "Recent System Activities",
                "Recent audit log records",
                "list",
                async () =>
                {
                    var data = await _context.AuditLogs
                        .OrderByDescending(x => x.ActionDate)
                        .Take(10)
                        .Select(x => new
                        {
                            x.TableName,
                            x.Action,
                            x.RecordId,
                            x.ActionDate
                        })
                        .ToListAsync();

                    return data;
                });

            var auditSummaryWidget = await SafeLoadWidgetAsync(
                "audit-log-summary",
                "Audit Log Summary",
                "Audit summary of today",
                "number",
                async () =>
                {
                    int logsToday = await _context.AuditLogs
                        .CountAsync(x => x.ActionDate.Date == DateTime.UtcNow.Date);

                    return new
                    {
                        LogsToday = logsToday
                    };
                });

            var userActivityWidget = await SafeLoadWidgetAsync(
                "user-activity-stats",
                "User Activity Statistics",
                "User activity from audit logs",
                "chart",
                async () =>
                {
                    int totalAuditEntries = await _context.AuditLogs.CountAsync();

                    return new
                    {
                        TotalAuditEntries = totalAuditEntries
                    };
                });

            var systemHealthWidget = CreateWidget(
                "system-health",
                "System Health Status",
                "Application/system health should come from monitoring service",
                "number",
                new
                {
                    Message = "Monitoring source not present in current database."
                });

            var securityAlertWidget = CreateWidget(
                "security-alerts",
                "Security Alerts",
                "Security alerts should come from monitoring/security service",
                "list",
                new[]
                {
                    "No dedicated security alert table in current schema."
                });

            var integrationWidget = CreateWidget(
                "integration-status",
                "Integration Status",
                "Integration status should come from configuration/monitoring source",
                "table",
                new[]
                {
                    new { Name = "Current schema", Status = "No dedicated integration status table" }
                });

            var dbPerformanceWidget = CreateWidget(
                "db-performance",
                "Database Performance Metrics",
                "DB performance metrics should come from SQL monitoring",
                "chart",
                new
                {
                    Message = "Not stored in business schema."
                });

            return new List<DashboardWidgetDTO>
            {
                systemHealthWidget,
                activeUsersWidget,
                recentActivitiesWidget,
                securityAlertWidget,
                integrationWidget,
                dbPerformanceWidget,
                auditSummaryWidget,
                userActivityWidget
            };
        }

        private async Task<DashboardWidgetDTO> SafeLoadWidgetAsync(
            string key,
            string title,
            string description,
            string widgetType,
            Func<Task<object?>> loader)
        {
            try
            {
                var data = await loader();

                return new DashboardWidgetDTO
                {
                    Key = key,
                    Title = title,
                    Description = description,
                    WidgetType = widgetType,
                    Data = data,
                    TimePeriod = "Current",
                    LastUpdated = DateTime.UtcNow,
                    ViewDetailsUrl = $"/api/dashboard/widgets/{key}/details",
                    CanRefresh = true,
                    CanRemove = true,
                    CanResize = true
                };
            }
            catch
            {
                return new DashboardWidgetDTO
                {
                    Key = key,
                    Title = title,
                    Description = description,
                    WidgetType = widgetType,
                    HasError = true,
                    ErrorMessage = "MSG-79: Failed to load widget data.",
                    TimePeriod = "Current",
                    LastUpdated = DateTime.UtcNow,
                    ViewDetailsUrl = $"/api/dashboard/widgets/{key}/details",
                    CanRefresh = true,
                    CanRemove = true,
                    CanResize = true
                };
            }
        }

        private DashboardWidgetDTO CreateWidget(
            string key,
            string title,
            string description,
            string widgetType,
            object? data)
        {
            return new DashboardWidgetDTO
            {
                Key = key,
                Title = title,
                Description = description,
                WidgetType = widgetType,
                Data = data,
                TimePeriod = "Current",
                LastUpdated = DateTime.UtcNow,
                ViewDetailsUrl = $"/api/dashboard/widgets/{key}/details",
                CanRefresh = true,
                CanRemove = true,
                CanResize = true
            };
        }

        private List<DashboardWidgetDTO> ApplySavedLayout(List<DashboardWidgetDTO> widgets, string? layoutJson)
        {
            if (string.IsNullOrWhiteSpace(layoutJson))
                return widgets;

            var layouts = JsonSerializer.Deserialize<List<DashboardWidgetLayoutItemDTO>>(layoutJson) ?? new();

            return widgets
                .Where(w =>
                {
                    var item = layouts.FirstOrDefault(x => x.WidgetKey == w.Key);
                    return item == null || item.IsVisible;
                })
                .OrderBy(w => layouts.FirstOrDefault(x => x.WidgetKey == w.Key)?.PositionY ?? int.MaxValue)
                .ThenBy(w => layouts.FirstOrDefault(x => x.WidgetKey == w.Key)?.PositionX ?? int.MaxValue)
                .ToList();
        }

        private string GetLayoutSettingKey(int userId)
        {
            return $"DASHBOARD_LAYOUT_USER_{userId}";
        }

        private string GetRefreshSettingKey(int userId)
        {
            return $"DASHBOARD_REFRESH_INTERVAL_USER_{userId}";
        }
    }
}