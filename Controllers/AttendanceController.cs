using HRManagement.DTOs;
using HRManagement.Models;
using HRManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost("assign-shift")]
    public async Task<IActionResult> AssignShift([FromBody] CreateShiftAssignmentDTO dto)
    {
        var result = await _attendanceService.AssignShiftAsync(dto);

        if (result == "MSG-ATT-01")
            return BadRequest(new { code = result, message = "Shift Overlap Detected" });

        return Ok(new { code = result });
    }

    [HttpGet("schedule/{employeeId}")]
    public async Task<IActionResult> GetMySchedule(int employeeId)
    {
        var schedule = await _attendanceService.GetWeeklyScheduleAsync(employeeId);
        return Ok(schedule);
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDTO DTO)
    {
        var response = await _attendanceService.CheckInAsync(DTO);

        if (response.Status == "Error")
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("check-out/{employeeId}")]
    public async Task<IActionResult> CheckOut(int employeeId)
    {
        var response = await _attendanceService.CheckOutAsync(employeeId);

        if (response.Status == "Error")
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut("assignment/{id}")]
    public async Task<IActionResult> UpdateAssignment(int id, [FromBody] UpdateShiftAssignmentDTO dto)
    {
        var result = await _attendanceService.UpdateAssignmentAsync(id, dto);
        if (result.StartsWith("MSG-SUC")) return Ok(new { code = result });
        return BadRequest(new { code = result });
    }

    // GET: api/Attendance/history/1
    [HttpGet("history/{employeeId}")]
    public async Task<IActionResult> GetAttendanceHistory(int employeeId)
    {
        var history = await _attendanceService.GetHistoryAsync(employeeId);
        return Ok(history);
    }

    [HttpGet("admin-view")]
    public async Task<IActionResult> GetAdminView([FromQuery] DateOnly? date, [FromQuery] int? deptId, [FromQuery] string? status)
    {
        var data = await _attendanceService.GetAdminViewAsync(date, deptId, status);
        return Ok(data);
    }
}