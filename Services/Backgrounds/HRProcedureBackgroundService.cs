using HRManagement.Models;
using HRManagement.Services.HRProceduces;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Services.Backgrounds
{
    public class HRProcedureBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HRProcedureBackgroundService> _logger;

        public HRProcedureBackgroundService(IServiceScopeFactory scopeFactory, ILogger<HRProcedureBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HR Procedure Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("HR Procedure Background Service is checking for pending-apply procedures.");

                try
                {
                    await ApplyPendingProceduresAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while applying pending HR procedures.");
                }

                // Chạy mỗi ngày một lần (để test có thể để ngắn hơn, nhưng thực tế là daily)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("HR Procedure Background Service is stopping.");
        }

        private async Task ApplyPendingProceduresAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HrmsDbContext>();
                var procedureService = scope.ServiceProvider.GetRequiredService<IHRProcedureService>();

                var today = DateTime.Today;

                // Lấy các procedure đã Approved, chưa Applied, và có EffectiveDate <= today
                var pendingApplyIds = await context.Hrprocedures
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
                        await procedureService.ApplyApprovedProcedureAsync(procedureId, 0); 
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
}
