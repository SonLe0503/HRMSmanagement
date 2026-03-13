using HRManagement.DTOs.CostAnalysis;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/cost-analysis")]
    [Authorize]
    public class CostAnalysisController : ControllerBase
    {
        private readonly ICostAnalysisService _costAnalysisService;

        public CostAnalysisController(ICostAnalysisService costAnalysisService)
        {
            _costAnalysisService = costAnalysisService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] CostAnalysisRequestDTO request)
        {
            var result = await _costAnalysisService.GenerateCostAnalysisAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }

        [HttpPost("scenario")]
        public async Task<IActionResult> CreateScenario([FromBody] CostScenarioDTO request)
        {
            var result = await _costAnalysisService.CreateScenarioAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }

        [HttpPost("set-alert")]
        public async Task<IActionResult> SetAlert([FromBody] CostAlertDTO request)
        {
            var message = await _costAnalysisService.SetCostAlertAsync(request);
            return Ok(new { message });
        }
    }
}