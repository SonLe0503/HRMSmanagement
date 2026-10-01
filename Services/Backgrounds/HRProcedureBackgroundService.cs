using Hangfire;
using HRManagement.Models;
using HRManagement.Services.HRProceduces;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Services.Backgrounds
{
    /// <summary>
    /// Job định kỳ (Hangfire, mỗi 5 phút): áp dụng các thủ tục nhân sự đã duyệt và đến ngày hiệu lực.
    /// </summary>
    public class HRProcedureBackgroundService
    {
        public const string JobId = "apply-pending-hr-procedures";

        private readonly HrmsDbContext _context;
        private readonly IHRProcedureService _procedureService;
        private readonly ILogger<HRProcedureBackgroundService> _logger;

        public HRProcedureBackgroundService(
            HrmsDbContext context,
            IHRProcedureService procedureService,
            ILogger<HRProcedureBackgroundService> logger)
        {
            _context = context;
            _procedureService = procedureService;
            _logger = logger;
        }

        // Job chạy lại sau 5 phút nên không cần retry; không cho 2 lần chạy chồng nhau.
        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        public async Task ApplyPendingProceduresAsync()
        {
            var today = DateTime.Today;

            // Lấy các procedure đã Approved, chưa Applied, và có EffectiveDate <= today
            var pendingApplyIds = await _context.Hrprocedures
                .Where(p => p.Status == "Approved" && p.AppliedDate == null && p.EffectiveDate <= DateOnly.FromDateTime(today))
                .Select(p => p.ProcedureId)
                .ToListAsync();

            _logger.LogInformation("Found {Count} procedures pending application.", pendingApplyIds.Count);

            foreach (var procedureId in pendingApplyIds)
            {
                try
                {
                    // Note: ApplyApprovedProcedureAsync usually takes currentUserId.
                    // In background context, we use 0 or a dedicated System User ID.
                    await _procedureService.ApplyApprovedProcedureAsync(procedureId, 0);
                    _logger.LogInformation("Applied procedure ID {ProcedureId} successfully.", procedureId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to apply procedure ID {ProcedureId}.", procedureId);
                }
            }
        }
    }
}
