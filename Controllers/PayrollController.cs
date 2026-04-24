using HRManagement.DTOs.Payroll;
using HRManagement.Services.Payroll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;

        public PayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        // ══════════════════════════════════════════════
        // KỲ LƯƠNG (PERIODS)
        // ══════════════════════════════════════════════

        [HttpGet("periods")]
        [Authorize(Roles = "ADMIN,HR,MANAGE")]
        public async Task<IActionResult> GetPeriods()
        {
            var periods = await _payrollService.GetAllPeriodsAsync();
            return Ok(periods);
        }

        [HttpPost("periods")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> CreatePeriod([FromBody] CreatePayrollPeriodDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var period = await _payrollService.CreatePeriodAsync(dto);
                return CreatedAtAction(nameof(GetPeriodById), new { periodId = period.PeriodId }, period);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("periods/{periodId:int}")]
        [Authorize(Roles = "ADMIN,HR,MANAGE")]
        public async Task<IActionResult> GetPeriodById(int periodId)
        {
            try
            {
                var period = await _payrollService.GetPeriodByIdAsync(periodId);
                return Ok(period);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("periods/{periodId:int}/summary")]
        [Authorize(Roles = "ADMIN,HR,MANAGE")]
        public async Task<IActionResult> GetPeriodSummary(int periodId)
        {
            try
            {
                var summary = await _payrollService.GetPeriodSummaryAsync(periodId);
                return Ok(summary);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("periods/{periodId:int}/calculate")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> CalculateAll(int periodId)
        {
            try
            {
                var results = await _payrollService.CalculateForAllEmployeesAsync(periodId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("lock-approved-attendance")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> LockApprovedAttendance()
        {
            try
            {
                var count = await _payrollService.LockAttendanceForAllApprovedPeriodsAsync();
                return Ok(new { message = $"Đã khóa {count} bản ghi chấm công thuộc các kỳ lương đã phê duyệt." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("periods/{periodId:int}/approve")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ApprovePeriod(int periodId)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

                var period = await _payrollService.ApprovePeriodAsync(periodId, userId);
                return Ok(period);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ══════════════════════════════════════════════
        // BẢNG LƯƠNG (RECORDS)
        // ══════════════════════════════════════════════

        [HttpGet("records/{periodId:int}")]
        [Authorize(Roles = "ADMIN,HR,MANAGE")]
        public async Task<IActionResult> GetRecordsByPeriod(int periodId)
        {
            var records = await _payrollService.GetRecordsByPeriodAsync(periodId);
            return Ok(records);
        }

        [HttpGet("records/detail/{recordId:int}")]
        [Authorize(Roles = "ADMIN,HR,MANAGE")]
        public async Task<IActionResult> GetRecord(int recordId)
        {
            try
            {
                var record = await _payrollService.GetRecordByIdAsync(recordId);
                return Ok(record);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("records/employee/{employeeId:int}")]
        public async Task<IActionResult> GetRecordsByEmployee(int employeeId)
        {
            // TODO: Phân quyền để NV chỉ xem được của chính mình
            var records = await _payrollService.GetRecordsByEmployeeAsync(employeeId);
            return Ok(records);
        }

        [HttpPost("periods/{periodId:int}/calculate/{employeeId:int}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> CalculateForEmployee(int periodId, int employeeId)
        {
            try
            {
                var record = await _payrollService.CalculateForEmployeeAsync(employeeId, periodId);
                return Ok(record);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("records/{recordId:int}/bonus")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> UpdateBonus(int recordId, [FromBody] decimal bonusAmount)
        {
            try
            {
                var record = await _payrollService.UpdateBonusAsync(recordId, bonusAmount);
                return Ok(record);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("records/{recordId:int}/allowance")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> AddAllowance(int recordId, [FromBody] CreatePayrollAllowanceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var record = await _payrollService.AddAllowanceAsync(recordId, dto);
                return Ok(record);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("records/{recordId:int}/allowance/{allowanceId:int}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> RemoveAllowance(int recordId, int allowanceId)
        {
            try
            {
                var record = await _payrollService.RemoveAllowanceAsync(recordId, allowanceId);
                return Ok(record);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("records/{recordId:int}/deduction")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> AddDeduction(int recordId, [FromBody] CreatePayrollDeductionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var record = await _payrollService.AddDeductionAsync(recordId, dto);
                return Ok(record);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("records/{recordId:int}/deduction/{deductionId:int}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> RemoveDeduction(int recordId, int deductionId)
        {
            try
            {
                var record = await _payrollService.RemoveDeductionAsync(recordId, deductionId);
                return Ok(record);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ══════════════════════════════════════════════
        // PHIẾU LƯƠNG (PAYSLIPS)
        // ══════════════════════════════════════════════

        [HttpPost("payslips/period/{periodId:int}/generate-all")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> GeneratePayslipsForPeriod(int periodId)
        {
            try
            {
                var count = await _payrollService.GeneratePayslipsForPeriodAsync(periodId);
                return Ok(new { message = $"Đã tạo {count} phiếu lương.", count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("payslips/{recordId:int}/generate")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> GeneratePayslip(int recordId)
        {
            try {
                var payslip = await _payrollService.GeneratePayslipAsync(recordId);
                return Ok(payslip);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("payslips/employee/{employeeId:int}")]
        public async Task<IActionResult> GetPayslipsByEmployee(int employeeId)
        {
            var payslips = await _payrollService.GetPayslipsByEmployeeAsync(employeeId);
            return Ok(payslips);
        }

        [HttpGet("payslips/{payslipId:int}/pdf")]
        public async Task<IActionResult> DownloadPdf(int payslipId)
        {
            try {
                var pdfBytes = await _payrollService.GetPayslipPdfAsync(payslipId);
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return BadRequest(new { message = "Không thể tạo PDF" });
                }
                return File(pdfBytes, "application/pdf", $"payslip_{payslipId}.pdf");
            } catch (Exception ex) {
                return NotFound(new { message = ex.Message });
            }
        }

        // ══════════════════════════════════════════════
        // TIỆN ÍCH
        // ══════════════════════════════════════════════

        [HttpGet("export/{periodId:int}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> ExportExcel(int periodId)
        {
            try {
                var excelBytes = await _payrollService.ExportPayrollExcelAsync(periodId);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Payroll_Period_{periodId}.xlsx");
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("tax/calculate")]
        public async Task<IActionResult> CalculateTax([FromBody] TaxCalculationRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _payrollService.CalculateTaxAsync(request);
            return Ok(result);
        }
    }
}
