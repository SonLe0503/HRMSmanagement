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
            var result = await _competencyReportService.GenerateReportAsync(filter);
            return Ok(result);
        }

        [HttpPost("drilldown")]
        public async Task<IActionResult> Drilldown([FromBody] CompetencyDrilldownRequestDTO request)
        {
            var result = await _competencyReportService.GetDrilldownAsync(request);
            return Ok(result);
        }

        [HttpPost("export")]
        public async Task<IActionResult> Export([FromBody] ExportCompetencyReportRequestDTO request)
        {
            var result = await _competencyReportService.ExportReportAsync(request);
            return File(result.FileContent, result.ContentType, result.FileName);
        }
    }
}
