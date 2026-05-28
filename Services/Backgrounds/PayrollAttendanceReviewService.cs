using HRManagement.Models;
using HRManagement.Services.Payroll;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Services.Backgrounds
{
    public class PayrollAttendanceReviewService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PayrollAttendanceReviewService> _logger;

        public PayrollAttendanceReviewService(IServiceScopeFactory scopeFactory, ILogger<PayrollAttendanceReviewService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payroll Attendance Review Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await TriggerDuePeriodReviewsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in PayrollAttendanceReviewService.");
                }

                // Chạy mỗi 1 giờ — đủ sát mà không bắn email nhiều lần trong ngày
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("Payroll Attendance Review Service stopped.");
        }

        private async Task TriggerDuePeriodReviewsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HrmsDbContext>();
            var payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();

            var today = DateOnly.FromDateTime(DateTime.Today);

            // Tìm các kỳ lương Open có AttendanceCutoffDate = hôm nay
            var duePeriodIds = await context.PayrollPeriods
                .Where(p => p.Status == "Open" && p.AttendanceCutoffDate == today)
                .Select(p => p.PeriodId)
                .ToListAsync();

            _logger.LogInformation("Found {Count} payroll periods due for attendance review trigger.", duePeriodIds.Count);

            foreach (var periodId in duePeriodIds)
            {
                try
                {
                    await payrollService.TriggerAttendanceReviewAsync(periodId);
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
