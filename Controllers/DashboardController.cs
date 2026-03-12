using HRManagement.DTOs.Dashboard;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _dashboardService.GetDashboardAsync();
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshDashboard([FromBody] RefreshDashboardDTO request)
        {
            var result = await _dashboardService.RefreshDashboardAsync(request);
            return Ok(result);
        }

        [HttpPost("layout")]
        public async Task<IActionResult> SaveLayout([FromBody] DashboardLayoutUpdateDTO request)
        {
            var success = await _dashboardService.SaveLayoutAsync(request);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "MSG-80: Failed to save dashboard layout."
                });
            }

            return Ok(new
            {
                message = "MSG-80: Dashboard layout saved successfully."
            });
        }

        [HttpPost("widgets/{widgetKey}/retry")]
        public async Task<IActionResult> RetryWidget(string widgetKey)
        {
            var result = await _dashboardService.RetryWidgetAsync(widgetKey);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("widgets/{widgetKey}/details")]
        public async Task<IActionResult> GetWidgetDetails(string widgetKey)
        {
            var result = await _dashboardService.GetWidgetDetailsAsync(widgetKey);
            return Ok(result);
        }
    }
}