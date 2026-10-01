using Hangfire;
using HRManagement.Models;
using HRManagement.Services.Payroll;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Services.Backgrounds
{
    /// <summary>
    /// Job định kỳ (Hangfire, mỗi giờ): tự chuyển kỳ lương sang giai đoạn xem xét chấm công
    /// khi đến AttendanceCutoffDate.
    /// </summary>
    public class PayrollAttendanceReviewService
    {
        public const string JobId = "trigger-payroll-attendance-review";

        private readonly HrmsDbContext _context;
        private readonly IPayrollService _payrollService;
        private readonly ILogger<PayrollAttendanceReviewService> _logger;

        public PayrollAttendanceReviewService(
            HrmsDbContext context,
            IPayrollService payrollService,
            ILogger<PayrollAttendanceReviewService> logger)
        {
            _context = context;
            _payrollService = payrollService;
            _logger = logger;
        }

        // Job chạy lại sau 1 giờ nên không cần retry; không cho 2 lần chạy chồng nhau (tránh gửi email trùng).
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        public async Task TriggerDuePeriodReviewsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Tìm các kỳ lương Open có AttendanceCutoffDate = hôm nay
            var duePeriodIds = await _context.PayrollPeriods
                .Where(p => p.Status == "Open" && p.AttendanceCutoffDate == today)
                .Select(p => p.PeriodId)
                .ToListAsync();

            _logger.LogInformation("Found {Count} payroll periods due for attendance review trigger.", duePeriodIds.Count);

            foreach (var periodId in duePeriodIds)
            {
                try
                {
                    await _payrollService.TriggerAttendanceReviewAsync(periodId);
                    _logger.LogInformation("Triggered attendance review for period {PeriodId}.", periodId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to trigger attendance review for period {PeriodId}.", periodId);
                }
            }
        }
    }
}
