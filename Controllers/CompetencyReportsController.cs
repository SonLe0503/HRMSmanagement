using HRManagement.DTOs.CompetencyReport;
using HRManagement.Services.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/competency-reports")]
    [Authorize]
    public class CompetencyReportsController : ControllerBase
    {
        private readonly ICompetencyReportService _competencyReportService;

        public CompetencyReportsController(ICompetencyReportService competencyReportService)
        {
            _competencyReportService = competencyReportService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateReport([FromBody] CompetencyReportFilterDTO filter)
        {
            try
            {
                var result = await _competencyReportService.GenerateReportAsync(filter);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpPost("drilldown")]
        public async Task<IActionResult> Drilldown([FromBody] CompetencyDrilldownRequestDTO request)
        {
            try
            {
                var result = await _competencyReportService.GetDrilldownAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpPost("export")]
        public async Task<IActionResult> Export([FromBody] ExportCompetencyReportRequestDTO request)
        {
            try
            {
                var result = await _competencyReportService.ExportReportAsync(request);
                return File(result.FileContent, result.ContentType, result.FileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }
    }
}
