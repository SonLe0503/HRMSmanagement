using HRManagement.DTOs;
using HRManagement.Services.Exports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpPost("export")]
        public async Task<IActionResult> Export([FromBody] ExportRequestDTO request)
        {
            var result = await _exportService.ExportAsync(request);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            // n?u g?i email thì không download
            if (request.SendToEmail)
            {
                return Ok(new
                {
                    message = result.Message
                });
            }

            return File(
                result.FileBytes,
                result.ContentType,
                result.FileName
            );
        }
    }
}
