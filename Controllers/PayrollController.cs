using HRManagement.DTOs;
using HRManagement.Models;
using HRManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Controllers
{
    public class PayrollController : Controller
    {
        private readonly HrmsDbContext _context;
        private readonly IPayrollService _payrollService;

        public PayrollController(IPayrollService payrollService, HrmsDbContext context)
        {
            _payrollService = payrollService;
            _context = context;
        }

        [HttpPost("aggregate")]
        public async Task<IActionResult> AggregatePayroll([FromBody] AggregatePayrollRequestDTO request)
        {
            if (request == null || request.PeriodId <= 0)
                return BadRequest("Invalid payroll period");

            var result = await _payrollService.AggregatePayrollData(request.PeriodId);

            if (result == null)
                return NotFound("Payroll period not found");

            return Ok(result);
        }
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculatePayroll([FromBody] CalculatePayrollRequestDTO request)
        {
            var result = await _payrollService.CalculatePayroll(request.PeriodId);
            if (result == null)
                return NotFound("Payroll period not found");
            return Ok(result);

        }
        [HttpGet("summary/{periodId}")]
        public async Task<IActionResult> GetSummary(int periodId)
        {
            var result = await _payrollService.GetPayrollSummary(periodId);

            return Ok(result);
        }
        [HttpPost("approve/{periodId}")]
        public async Task<IActionResult> ApprovePayroll(int periodId)
        {
            if (periodId <= 0)
                return BadRequest("Invalid periodId");

            try
            {
                var userId = 1; // sau này lấy từ JWT

                var result = await _payrollService.ApprovePayroll(periodId, userId);

                return Ok(new
                {
                    message = "Payroll approved successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("send-back")]
        public async Task<IActionResult> SendBack([FromBody] SendBackPayrollDTO request)
        {
            try
            {
                var result = await _payrollService.SendBackForCorrection(request.PeriodId, request.Note);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("generate-payslips")]
        public async Task<IActionResult> GeneratePayslips([FromBody] GeneratePayslipRequestDTO request)
        {
            try
            {
                var result = await _payrollService.GeneratePayslips(request.PeriodId, request.DeliveryMethod);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetPayslips(int employeeId)
        {
            var result = await _payrollService.GetEmployeePayslips(employeeId);

            return Ok(result);
        }
        [HttpGet("view/{payslipId}")]
        public async Task<IActionResult> ViewPayslip(int payslipId)
        {
            if (payslipId <= 0)
                return BadRequest("Invalid payslipId");

            try
            {
                var result = await _payrollService.ViewPayslip(payslipId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpGet("download/{payslipId}")]
        public async Task<IActionResult> DownloadPayslip(int payslipId)
        {
            var payslip = await _context.Payslips.FindAsync(payslipId);

            if (payslip == null || string.IsNullOrEmpty(payslip.Pdfpath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(payslip.Pdfpath);

            return File(fileBytes, "application/pdf", "payslip.pdf");
        }
    }
}
