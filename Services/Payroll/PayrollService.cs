using AutoMapper;
using HRManagement.DataAcess.Interfaces;
using HRManagement.DTOs.Payroll;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRManagement.Services.Payroll
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _payrollRepo;
        private readonly IPayrollPeriodRepository _periodRepo;
        private readonly TaxCalculationService _taxService;
        private readonly HrmsDbContext _context;
        private readonly IMapper _mapper;

        public PayrollService(
            IPayrollRepository payrollRepo,
            IPayrollPeriodRepository periodRepo,
            TaxCalculationService taxService,
            HrmsDbContext context,
            IMapper mapper)
        {
            _payrollRepo = payrollRepo;
            _periodRepo = periodRepo;
            _taxService = taxService;
            _context = context;
            _mapper = mapper;
        }

        // ── Kỳ lương ──────────────────────────────────────────────────────────
        public async Task<PayrollPeriodDto> CreatePeriodAsync(CreatePayrollPeriodDto dto)
        {
            if (await _periodRepo.ExistsAsync(dto.Month, dto.Year))
                throw new InvalidOperationException($"Kỳ lương {dto.Month}/{dto.Year} đã tồn tại.");

            var period = new PayrollPeriod
            {
                Month = dto.Month,
                Year = dto.Year,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = "Open",
            };
            await _periodRepo.CreateAsync(period);
            return _mapper.Map<PayrollPeriodDto>(period);
        }

        public async Task<List<PayrollPeriodDto>> GetAllPeriodsAsync()
        {
            var periods = await _periodRepo.GetAllAsync();
            return _mapper.Map<List<PayrollPeriodDto>>(periods);
        }

        public async Task<PayrollPeriodDto> GetPeriodByIdAsync(int periodId)
        {
            var period = await _periodRepo.GetByIdAsync(periodId)
                ?? throw new KeyNotFoundException($"Không tìm thấy kỳ lương {periodId}.");
            return _mapper.Map<PayrollPeriodDto>(period);
        }

        public async Task<PayrollSummaryDto> GetPeriodSummaryAsync(int periodId)
        {
            var period = await _periodRepo.GetByIdAsync(periodId)
                ?? throw new KeyNotFoundException("Không tìm thấy kỳ lương.");

            var records = await _payrollRepo.GetByPeriodAsync(periodId);

            var summary = new PayrollSummaryDto
            {
                PeriodId = periodId,
                Month = period.Month,
                Year = period.Year,
                TotalEmployees = records.Count,
                TotalBaseSalary = records.Sum(r => r.BaseSalary),
                TotalAllowances = records.Sum(r => r.TotalAllowances),
                TotalOvertimePay = records.Sum(r => r.OvertimePay),
                TotalBonuses = records.Sum(r => r.BonusAmount),
                TotalGrossPay = records.Sum(r => r.GrossPay),
                TotalInsurance = records.Sum(r => r.InsuranceAmount),
                TotalTax = records.Sum(r => r.TaxAmount),
                TotalDeductions = records.Sum(r => r.TotalDeductions),
                TotalNetPay = records.Sum(r => r.NetPay)
            };

            summary.ByDepartment = records
                .GroupBy(r => r.Employee.Department?.DepartmentName ?? "Unknown")
                .Select(g => new DepartmentPayrollSummary
                {
                    DepartmentName = g.Key,
                    EmployeeCount = g.Count(),
                    TotalNetPay = g.Sum(r => r.NetPay)
                })
                .ToList();

            return summary;
        }

        // ── Tính lương ────────────────────────────────────────────────────────
        public async Task<PayrollRecordDto> CalculateForEmployeeAsync(int employeeId, int periodId)
        {
            var period = await _periodRepo.GetByIdAsync(periodId)
                ?? throw new KeyNotFoundException("Không tìm thấy kỳ lương.");

            if (period.Status == "Approved" || period.Status == "Closed")
                throw new InvalidOperationException("Kỳ lương đã được duyệt, không thể tính lại.");

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId)
                ?? throw new KeyNotFoundException("Không tìm thấy nhân viên.");

            // 1. Tính ngày công
            var workingDays = CalculateWorkingDays(period.StartDate, period.EndDate);
            var actualWorkingDays = await CalculateActualWorkingDaysAsync(employeeId, period);

            // 2. Lương theo ngày công thực tế
            var baseSalary = employee.BaseSalary ?? 0m;
            var dailySalary = workingDays > 0 ? baseSalary / workingDays : 0m;
            var salariedAmount = Math.Round(dailySalary * actualWorkingDays, 0);

            // 3. Phụ cấp chính sách
            var allowances = await BuildAllowancesAsync(employee, period);
            
            // 4. Lương OT
            var (overtimePay, otAllowances) = await CalculateOvertimeAsync(employeeId, period, baseSalary, workingDays);

            // 5. Thưởng & Khấu trừ thủ công từ record cũ (nếu có)
            var existingRecord = await _payrollRepo.GetByEmployeeAndPeriodAsync(employeeId, periodId);
            var bonusAmount = existingRecord?.BonusAmount ?? 0m;

            var totalAllowances = allowances.Sum(a => a.Amount) + otAllowances.Sum(a => a.Amount);
            var grossPay = salariedAmount + totalAllowances + bonusAmount;

            // 6. Bảo hiểm (10.5%)
            var insuranceSalary = Math.Min(grossPay, 46_800_000m);
            var insuranceAmount = Math.Round(insuranceSalary * 0.105m, 0);

            // 7. Thuế TNCN
            // Giả sử 0 người phụ thuộc cho đơn giản bước đầu
            var taxResult = _taxService.Calculate(grossPay, numberOfDependents: 0);
            var taxAmount = taxResult.TaxAmount;

            // 8. Khấu trừ thủ công (giữ nguyên nếu tính lại)
            var manualDeductions = existingRecord?.PayrollDeductions
                .Where(d => d.DeductionType != "Insurance" && d.DeductionType != "Tax")
                .Select(d => new PayrollDeduction {
                    DeductionType = d.DeductionType,
                    DeductionName = d.DeductionName,
                    Amount = d.Amount,
                    Description = d.Description
                })
                .ToList() ?? new List<PayrollDeduction>();

            var totalDeductions = insuranceAmount + taxAmount + manualDeductions.Sum(d => d.Amount);
            var netPay = grossPay - totalDeductions;

            // 9. Upsert record
            var record = existingRecord ?? new PayrollRecord();
            record.EmployeeId = employeeId;
            record.PeriodId = periodId;
            record.BaseSalary = baseSalary;
            record.WorkingDays = workingDays;
            record.ActualWorkingDays = actualWorkingDays;
            record.TotalAllowances = totalAllowances;
            record.OvertimePay = overtimePay;
            record.BonusAmount = bonusAmount;
            record.InsuranceAmount = insuranceAmount;
            record.TaxAmount = taxAmount;
            record.TotalDeductions = totalDeductions;
            record.NetPay = netPay;
            record.Status = "Calculated";
            record.CalculatedDate = DateTime.Now;

            // Xử lý quan hệ 1-n: Cách an toàn nhất là xóa n và add lại
            if (existingRecord != null)
            {
                _context.PayrollAllowances.RemoveRange(existingRecord.PayrollAllowances);
                _context.PayrollDeductions.RemoveRange(existingRecord.PayrollDeductions);
                await _context.SaveChangesAsync(); // Lưu để xóa triệt để trước khi add mới
            }

            record.PayrollAllowances = allowances.Concat(otAllowances).ToList();
            record.PayrollDeductions = manualDeductions;
            record.PayrollDeductions.Add(new PayrollDeduction
            {
                DeductionType = "Insurance",
                DeductionName = "BHXH + BHYT + BHTN (10.5%)",
                Amount = insuranceAmount,
            });
            record.PayrollDeductions.Add(new PayrollDeduction
            {
                DeductionType = "Tax",
                DeductionName = $"Thuế TNCN (Bậc {taxResult.TaxBracket})",
                Amount = taxAmount,
            });

            if (existingRecord == null)
                await _payrollRepo.CreateAsync(record);
            else
                await _payrollRepo.UpdateAsync(record);

            return _mapper.Map<PayrollRecordDto>(record);
        }

        public async Task<List<PayrollRecordDto>> CalculateForAllEmployeesAsync(int periodId)
        {
            var employees = await _context.Employees
                .Where(e => e.EmploymentStatus == "Active")
                .Select(e => e.EmployeeId)
                .ToListAsync();

            return await CalculateBatchAsync(periodId, employees);
        }

        public async Task<List<PayrollRecordDto>> CalculateBatchAsync(int periodId, List<int> employeeIds)
        {
            var results = new List<PayrollRecordDto>();
            foreach (var empId in employeeIds)
            {
                try
                {
                    var result = await CalculateForEmployeeAsync(empId, periodId);
                    results.Add(result);
                }
                catch (Exception)
                {
                    // Tiếp tục với nhân viên khác khi có lỗi đơn lẻ
                }
            }
            return results;
        }

        // ── Lấy dữ liệu ───────────────────────────────────────────────────────
        public async Task<List<PayrollRecordDto>> GetRecordsByPeriodAsync(int periodId)
        {
            var records = await _payrollRepo.GetByPeriodWithDetailsAsync(periodId);
            return _mapper.Map<List<PayrollRecordDto>>(records);
        }

        public async Task<PayrollRecordDto> GetRecordByIdAsync(int payrollRecordId)
        {
            var record = await _payrollRepo.GetByIdAsync(payrollRecordId)
                ?? throw new KeyNotFoundException("Không tìm thấy bản ghi lương.");
            return _mapper.Map<PayrollRecordDto>(record);
        }

        public async Task<List<PayrollRecordDto>> GetRecordsByEmployeeAsync(int employeeId)
        {
            var records = await _payrollRepo.GetByEmployeeAsync(employeeId);
            return _mapper.Map<List<PayrollRecordDto>>(records);
        }

        // ── Điều chỉnh thủ công ───────────────────────────────────────────────
        public async Task<PayrollRecordDto> AddAllowanceAsync(int payrollRecordId, CreatePayrollAllowanceDto dto)
        {
            var record = await _payrollRepo.GetByIdAsync(payrollRecordId) ?? throw new KeyNotFoundException();
            record.PayrollAllowances.Add(new PayrollAllowance {
                AllowanceType = dto.AllowanceType,
                AllowanceName = dto.AllowanceName,
                Amount = dto.Amount,
                Description = dto.Description
            });
            record.TotalAllowances += dto.Amount;
            record.NetPay += dto.Amount; // Tăng thực lĩnh
            await _payrollRepo.UpdateAsync(record);
            return _mapper.Map<PayrollRecordDto>(record);
        }

        public async Task<PayrollRecordDto> RemoveAllowanceAsync(int payrollRecordId, int allowanceId)
        {
            var record = await _payrollRepo.GetByIdAsync(payrollRecordId) ?? throw new KeyNotFoundException();
            var item = record.PayrollAllowances.FirstOrDefault(a => a.AllowanceId == allowanceId);
            if (item != null)
            {
                record.TotalAllowances -= item.Amount;
                record.NetPay -= item.Amount;
                _context.PayrollAllowances.Remove(item);
                await _payrollRepo.UpdateAsync(record);
            }
            return _mapper.Map<PayrollRecordDto>(record);
        }

        public async Task<PayrollRecordDto> AddDeductionAsync(int payrollRecordId, CreatePayrollDeductionDto dto)
        {
            var record = await _payrollRepo.GetByIdAsync(payrollRecordId) ?? throw new KeyNotFoundException();
            record.PayrollDeductions.Add(new PayrollDeduction {
                DeductionType = dto.DeductionType,
                DeductionName = dto.DeductionName,
                Amount = dto.Amount,
                Description = dto.Description
            });
            record.TotalDeductions += dto.Amount;
            record.NetPay -= dto.Amount; 
            await _payrollRepo.UpdateAsync(record);
            return _mapper.Map<PayrollRecordDto>(record);
        }

        public async Task<PayrollRecordDto> RemoveDeductionAsync(int payrollRecordId, int deductionId)
        {
            var record = await _payrollRepo.GetByIdAsync(payrollRecordId) ?? throw new KeyNotFoundException();
            var item = record.PayrollDeductions.FirstOrDefault(d => d.DeductionId == deductionId);
            if (item != null)
            {
                record.TotalDeductions -= item.Amount;
                record.NetPay += item.Amount;
                _context.PayrollDeductions.Remove(item);
                await _payrollRepo.UpdateAsync(record);
            }
            return _mapper.Map<PayrollRecordDto>(record);
        }

        public async Task<PayrollRecordDto> UpdateBonusAsync(int payrollRecordId, decimal bonusAmount)
        {
            var record = await _payrollRepo.GetByIdAsync(payrollRecordId) ?? throw new KeyNotFoundException();
            var diff = bonusAmount - record.BonusAmount;
            record.BonusAmount = bonusAmount;
            record.NetPay += diff;
            await _payrollRepo.UpdateAsync(record);
            return _mapper.Map<PayrollRecordDto>(record);
        }

        // ── Phê duyệt ─────────────────────────────────────────────────────────
        public async Task<PayrollPeriodDto> ApprovePeriodAsync(int periodId, int approvedByUserId)
        {
            var period = await _periodRepo.GetByIdAsync(periodId)
                ?? throw new KeyNotFoundException("Không tìm thấy kỳ lương.");

            period.Status = "Approved";
            period.ApprovedDate = DateTime.Now;
            period.ApprovedBy = approvedByUserId;
            await _periodRepo.UpdateAsync(period);

            return _mapper.Map<PayrollPeriodDto>(period);
        }

        public async Task<PayrollRecordDto> ApproveRecordAsync(int payrollRecordId, int approvedByUserId)
        {
            var record = await _payrollRepo.GetByIdAsync(payrollRecordId) ?? throw new KeyNotFoundException();
            record.Status = "Approved";
            record.ApprovedDate = DateTime.Now;
            // Thuộc tính ApprovedBy trong PayrollRecord model nếu có
            await _payrollRepo.UpdateAsync(record);
            return _mapper.Map<PayrollRecordDto>(record);
        }

        public async Task<List<PayslipDto>> GetPayslipsByEmployeeAsync(int employeeId)
        {
            var payslips = await _context.Payslips
                .Include(p => p.Period)
                .Include(p => p.PayrollRecord)
                .Include(p => p.Employee)
                    .ThenInclude(e => e.Department)
                .Include(p => p.Employee)
                    .ThenInclude(e => e.Position)
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.Period.Year)
                .ThenByDescending(p => p.Period.Month)
                .ToListAsync();

            return _mapper.Map<List<PayslipDto>>(payslips);
        }

        public async Task<TaxCalculationResultDto> CalculateTaxAsync(TaxCalculationRequestDto request)
        {
            return await Task.FromResult(_taxService.Calculate(request.GrossIncome, request.NumberOfDependents, request.IsInsuranceApplicable));
        }

        // ── Helper methods ─────────────────────────────────────────────────────
        private decimal CalculateWorkingDays(DateOnly startDate, DateOnly endDate)
        {
            var count = 0;
            var current = startDate;
            while (current <= endDate)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                    count++;
                current = current.AddDays(1);
            }
            return count;
        }

        private async Task<decimal> CalculateActualWorkingDaysAsync(int employeeId, PayrollPeriod period)
        {
            // Ngày có mặt từ AttendanceRecord (Status = Present, Late)
            var attendanceDays = await _context.AttendanceRecords
                .Where(a => a.EmployeeId == employeeId
                    && a.AttendanceDate >= period.StartDate
                    && a.AttendanceDate <= period.EndDate
                    && (a.Status == "Present" || a.Status == "Late"))
                .CountAsync();

            // Ngày nghỉ có lương (LeaveRequest approved + IsPaid)
            var paidLeaveRequests = await _context.LeaveRequests
                .Include(l => l.LeaveType)
                .Where(l => l.EmployeeId == employeeId
                    && l.Status == "Approved"
                    && l.StartDate <= period.EndDate
                    && l.EndDate >= period.StartDate
                    && l.LeaveType.IsPaid)
                .ToListAsync();

            decimal paidLeaveDays = 0;
            foreach (var req in paidLeaveRequests) {
                var start = req.StartDate > period.StartDate ? req.StartDate : period.StartDate;
                var end = req.EndDate < period.EndDate ? req.EndDate : period.EndDate;
                
                // Logic đơn giản cho các ngày trong kỳ
                var current = start;
                while (current <= end) {
                    if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                        paidLeaveDays++;
                    current = current.AddDays(1);
                }
            }

            return (decimal)attendanceDays + paidLeaveDays;
        }

        private async Task<List<PayrollAllowance>> BuildAllowancesAsync(Employee employee, PayrollPeriod period)
        {
            var policies = await _context.PayrollPolicies
                .Where(p => p.PolicyType == "Allowance"
                    && p.IsActive
                    && (p.ApplicableEmployeeGroup == "All" || p.ApplicableEmployeeGroup == employee.EmploymentType))
                .ToListAsync();

            return policies.Select(p => new PayrollAllowance
            {
                AllowanceType = "Policy",
                AllowanceName = p.PolicyName,
                Amount = p.BaseAmount, // Tạm thời dùng BaseAmount cố định
                Description = p.Description,
            }).ToList();
        }

        private async Task<(decimal OvertimePay, List<PayrollAllowance> Details)>
            CalculateOvertimeAsync(int employeeId, PayrollPeriod period, decimal baseSalary, decimal workingDays)
        {
            var overtimeRequests = await _context.OvertimeRequests
                .Where(o => o.EmployeeId == employeeId
                    && o.Status == "Approved"
                    && o.OvertimeDate >= period.StartDate
                    && o.OvertimeDate <= period.EndDate)
                .ToListAsync();

            if (!overtimeRequests.Any())
                return (0m, new List<PayrollAllowance>());

            var hourlySalary = workingDays > 0 ? baseSalary / (workingDays * 8) : 0m;
            var details = new List<PayrollAllowance>();
            var totalOt = 0m;

            foreach (var ot in overtimeRequests)
            {
                var rate = ot.OvertimeDate.DayOfWeek switch
                {
                    DayOfWeek.Saturday or DayOfWeek.Sunday => 2.0m,
                    _ => 1.5m,
                };

                var otPay = Math.Round(hourlySalary * (decimal)ot.TotalHours * rate, 0);
                totalOt += otPay;

                details.Add(new PayrollAllowance
                {
                    AllowanceType = "Overtime",
                    AllowanceName = $"Lương OT {ot.OvertimeDate:dd/MM} (x{rate})",
                    Amount = otPay,
                    Description = ot.Reason,
                });
            }

            return (totalOt, details);
        }
    }
}
