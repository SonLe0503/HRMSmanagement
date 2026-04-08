using HRManagement.DTOs.WorkforceAnalytics;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/workforce-analytics")]
    [Authorize]
    public class WorkforceAnalyticsController : ControllerBase
    {
        private readonly IWorkforceAnalyticsService _workforceAnalyticsService;

        public WorkforceAnalyticsController(IWorkforceAnalyticsService workforceAnalyticsService)
        {
            _workforceAnalyticsService = workforceAnalyticsService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] WorkforceAnalyticsRequestDTO request)
        {
            var result = await _workforceAnalyticsService.GenerateAnalyticsAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }

        [HttpPost("save-view")]
        public async Task<IActionResult> SaveView([FromBody] SaveWorkforceViewDTO request)
        {
            var success = await _workforceAnalyticsService.SaveViewAsync(request);

            if (!success)
                return BadRequest(new { message = "Failed to save view." });

            return Ok(new { message = "Custom view saved successfully." });
        }

        [HttpPost("schedule-report")]
        public async Task<IActionResult> ScheduleReport([FromBody] ScheduleWorkforceReportDTO request)
        {
            var message = await _workforceAnalyticsService.ScheduleReportAsync(request);
            return Ok(new { message });
        }

        [HttpPost("ai-insights")]
        public async Task<IActionResult> GetAIInsights([FromBody] WorkforceAnalyticsRequestDTO request)
        {
            var result = await _workforceAnalyticsService.GetAIInsightsAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }
    }
}