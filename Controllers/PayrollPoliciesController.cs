using AutoMapper;
using HRManagement.DTOs;
using HRManagement.Models;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayrollPoliciesController : Controller
    {
        private readonly HrmsDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public PayrollPoliciesController(HrmsDbContext context, IMapper mapper, ICurrentUserService currentUser)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
        }
        [HttpGet]
        public async Task<IActionResult> GetPayrollPolicies()
        {
            var query = _context.PayrollPolicies.AsQueryable();

            var policies = await query
                .OrderByDescending(p => p.EffectiveStartDate)
                .ToListAsync();

            if (!policies.Any())
            {
                return NotFound(new
                {
                    MessageCode = "MSG-50",
                    Message = "No payroll policies configured."
                });
            }

            var result = _mapper.Map<List<PayrollPolicyListDTO>>(policies);
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayrollPolicyDetail(int id)
        {
            var policy = await _context.PayrollPolicies
                .FirstOrDefaultAsync(p => p.PolicyId == id);

            if (policy == null)
                return NotFound();

            var result = _mapper.Map<PayrollPolicyDetailDTO>(policy);

            return Ok(result);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePayrollPolicy([FromBody] CreatePayrollPolicyDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PolicyName) ||
        string.IsNullOrWhiteSpace(dto.PolicyType) ||
        dto.EffectiveStartDate == default)
            {
                return BadRequest(new { MessageCode = "MSG-52", Message = "Required fields missing." });
            }

            // Check policy type hợp lệ (BR-30)
            var validTypes = new[] { "Salary", "Allowance", "Deduction", "Overtime", "Bonus", "Tax" };
            if (!validTypes.Contains(dto.PolicyType))
            {
                return BadRequest(new { MessageCode = "MSG-52", Message = "Invalid policy type." });
            }

            // Effective date in the past (BR-31)
            if (dto.EffectiveStartDate < DateOnly.FromDateTime(DateTime.Today))
            {
                return BadRequest(new { MessageCode = "MSG-54", Message = "Effective date cannot be in the past." });
            }

            // Check trùng tên
            var nameExists = await _context.PayrollPolicies
                .AnyAsync(p => p.PolicyName == dto.PolicyName);

            if (nameExists)
            {
                return Conflict(new { MessageCode = "MSG-55", Message = "Policy name already exists." });
            }

            // ===== STEP 7 CHECK POLICY CONFLICT =====

            var conflict = await _context.PayrollPolicies
                .Where(p =>
                    p.PolicyType == dto.PolicyType &&
                    p.ApplicableEmployeeGroup == dto.ApplicableEmployeeGroup &&
                    p.IsActive)
                .Where(p =>
                    (dto.EffectiveEndDate == null || p.EffectiveStartDate <= dto.EffectiveEndDate) &&
                    (p.EffectiveEndDate == null || p.EffectiveEndDate >= dto.EffectiveStartDate))
                .FirstOrDefaultAsync();

            if (conflict != null)
            {
                return Conflict(new
                {
                    MessageCode = "MSG-56",
                    Message = "Policy conflict detected with existing policy.",
                    ConflictPolicyId = conflict.PolicyId,
                    ConflictPolicyName = conflict.PolicyName
                });
            }

            // ===== STEP 8 SAVE POLICY =====

            var policy = _mapper.Map<PayrollPolicy>(dto);

            policy.CreatedDate = DateTime.Now;
            policy.CreatedBy = _currentUser.UserId;

            // ===== STEP 9 SET STATUS =====

            if (dto.EffectiveStartDate <= DateOnly.FromDateTime(DateTime.Today))
                policy.IsActive = true;
            else
                policy.IsActive = false; // Draft

            _context.PayrollPolicies.Add(policy);
            await _context.SaveChangesAsync();

            // ===== STEP 10 SUCCESS =====

            return Ok(new
            {
                MessageCode = "MSG-51",
                Message = "Payroll policy created successfully.",
                PolicyId = policy.PolicyId
            });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayrollPolicy(int id, [FromBody] UpdatePayrollPolicyDto dto)
        {
            var existingPolicy = await _context.PayrollPolicies
                .FirstOrDefaultAsync(p => p.PolicyId == id);

            if (existingPolicy == null)
                return NotFound(new { Message = "Payroll policy not found." });

            // ===== STEP 8 VALIDATION =====

            if (string.IsNullOrWhiteSpace(dto.PolicyName) ||
                string.IsNullOrWhiteSpace(dto.PolicyType))
            {
                return BadRequest(new
                {
                    MessageCode = "MSG-52",
                    Message = "Required fields missing."
                });
            }

            // Check duplicate name
            var duplicateName = await _context.PayrollPolicies
                .AnyAsync(p => p.PolicyName == dto.PolicyName && p.PolicyId != id);

            if (duplicateName)
            {
                return Conflict(new
                {
                    MessageCode = "MSG-55",
                    Message = "Policy name already exists."
                });
            }

            // ===== STEP 9 CHECK PAYROLL IMPACT =====

            var affectedPayroll = await _context.PayrollRecords
                .Include(r => r.Period)
                .AnyAsync(r =>
                    r.Period.StartDate >= dto.EffectiveStartDate
                    && r.Status != "Draft");

            if (affectedPayroll)
            {
                return Conflict(new
                {
                    MessageCode = "MSG-58",
                    Message = "Update affects processed payroll periods."
                });
            }

            // ===== STEP 12 CREATE NEW POLICY VERSION =====

            existingPolicy.IsActive = false;

            var newPolicy = _mapper.Map<PayrollPolicy>(dto);

            newPolicy.CreatedDate = DateTime.Now;
            newPolicy.CreatedBy = _currentUser.UserId;
            newPolicy.IsActive = dto.EffectiveStartDate <= DateOnly.FromDateTime(DateTime.Today);

            _context.PayrollPolicies.Add(newPolicy);

            await _context.SaveChangesAsync();

            // ===== STEP 14 SUCCESS =====

            return Ok(new
            {
                MessageCode = "MSG-57",
                Message = "Payroll policy updated successfully.",
                NewPolicyId = newPolicy.PolicyId
            });
        }
    }
}
