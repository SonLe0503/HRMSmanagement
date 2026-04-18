using AutoMapper;
using HRManagement.DataAcess.Interfaces;
using HRManagement.DTOs.Payroll;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

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
                TotalGrossPay = records.Sum(r => r.GrossPay ?? 0m),
                TotalInsurance = records.Sum(r => r.InsuranceAmount),
                TotalTax = records.Sum(r => r.TaxAmount),
                TotalDeductions = records.Sum(r => r.TotalDeductions),
                TotalNetPay = records.Sum(r => r.NetPay ?? 0m)
            };

            summary.ByDepartment = records
                .GroupBy(r => r.Employee.Department?.DepartmentName ?? "Unknown")
                .Select(g => new DepartmentPayrollSummary
                {
                    DepartmentName = g.Key,
                    EmployeeCount = g.Count(),
                    TotalNetPay = g.Sum(r => r.NetPay ?? 0m)
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
            var workingDays = await CalculateAssignedWorkingDaysAsync(employeeId, period);
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

            // TotalAllowances chỉ gồm phụ cấp chính sách (KHÔNG gộp OT)
            // OvertimePay lưu riêng để hiển thị tách biệt trên phiếu lương
            var policyAllowancesTotal = allowances.Sum(a => a.Amount);
            var grossPay = salariedAmount + policyAllowancesTotal + overtimePay + bonusAmount;

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
            record.TotalAllowances = policyAllowancesTotal;  // chỉ phụ cấp chính sách
            record.OvertimePay = overtimePay;                // OT riêng biệt
            record.BonusAmount = bonusAmount;
            record.GrossPay = grossPay;                      // Gross = ngày công + phụ cấp + OT + thưởng
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

        public async Task<PayslipDto> GeneratePayslipAsync(int payrollRecordId)
        {
            var record = await _payrollRepo.GetByIdAsync(payrollRecordId)
                ?? throw new KeyNotFoundException("Không tìm thấy bản ghi lương.");

            var period = await _periodRepo.GetByIdAsync(record.PeriodId)
                ?? throw new KeyNotFoundException();

            // 1. Kiểm tra Payslip đã tồn tại chưa
            var existingPayslip = await _context.Payslips.FirstOrDefaultAsync(p => p.PayrollRecordId == payrollRecordId);
            
            var payslip = existingPayslip ?? new Payslip();
            payslip.PayrollRecordId = payrollRecordId;
            payslip.EmployeeId = record.EmployeeId;
            payslip.PeriodId = record.PeriodId;
            payslip.PayslipNumber = $"PS-{period.Year}{period.Month:D2}-{record.PayrollRecordId:D5}";
            payslip.GeneratedDate = DateTime.Now;
            payslip.IsViewed = false;

            if (existingPayslip == null)
                _context.Payslips.Add(payslip);
            else
                _context.Payslips.Update(payslip);

            await _context.SaveChangesAsync();
            return _mapper.Map<PayslipDto>(payslip);
        }

        public async Task<byte[]> GetPayslipPdfAsync(int payslipId)
        {
            var payslip = await _context.Payslips
                .Include(p => p.PayrollRecord).ThenInclude(r => r.PayrollAllowances)
                .Include(p => p.PayrollRecord).ThenInclude(r => r.PayrollDeductions)
                .Include(p => p.PayrollRecord).ThenInclude(r => r.Employee).ThenInclude(e => e.Department)
                .Include(p => p.PayrollRecord).ThenInclude(r => r.Employee).ThenInclude(e => e.Position)
                .Include(p => p.Period)
                .FirstOrDefaultAsync(p => p.PayslipId == payslipId)
                ?? throw new KeyNotFoundException();

            var pdfService = new PayslipPdfService();
            return pdfService.GeneratePdf(payslip.PayrollRecord, payslip.Period);
        }

        public async Task<byte[]> ExportPayrollExcelAsync(int periodId)
        {
            var period = await _periodRepo.GetByIdAsync(periodId) ?? throw new KeyNotFoundException();
            var records = await _payrollRepo.GetByPeriodWithDetailsAsync(periodId);
            
            var excelService = new PayrollExportService();
            return excelService.ExportPayrollExcel(records, period);
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

        /// <summary>
        /// Mẫu số: Số ngày được phân ca làm việc (ShiftAssignment) trong kỳ.
        /// Fallback về đếm T2-T6 nếu nhân viên chưa được phân ca.
        /// </summary>
        private async Task<decimal> CalculateAssignedWorkingDaysAsync(int employeeId, PayrollPeriod period)
        {
            var assignedDays = await _context.ShiftAssignments
                .Where(sa => sa.EmployeeId == employeeId
                    && sa.AssignmentDate >= period.StartDate
                    && sa.AssignmentDate <= period.EndDate)
                .Select(sa => sa.AssignmentDate)
                .Distinct()
                .CountAsync();

            if (assignedDays > 0)
                return assignedDays;

            // Fallback: đếm ngày T2–T6 nếu chưa có phân ca
            return CalculateWeekdayCount(period.StartDate, period.EndDate);
        }

        /// <summary>
        /// Tính ngày T2–T6 trong khoảng — dùng làm fallback khi chưa có ShiftAssignment.
        /// </summary>
        private static decimal CalculateWeekdayCount(DateOnly startDate, DateOnly endDate)
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

        /// <summary>
        /// Tử số: Ngày công thực tế quy đổi = Σ(WorkingHours / ShiftStandardHours).
        /// Tính cho:
        ///   - AttendanceRecord Status = "Present" hoặc "Late" (đi làm bình thường)
        ///   - AttendanceRecord có ExplanationStatus = "Approved" (giải trình được phê duyệt)
        /// Cộng thêm ngày nghỉ có lương (LeaveRequest Approved + IsPaid).
        /// </summary>
        private async Task<decimal> CalculateActualWorkingDaysAsync(int employeeId, PayrollPeriod period)
        {
            // Bước 1: Lấy ShiftAssignment (chỉ lấy scalar, tránh navigation property sau GroupBy)
            var assignments = await _context.ShiftAssignments
                .Where(sa => sa.EmployeeId == employeeId
                    && sa.AssignmentDate >= period.StartDate
                    && sa.AssignmentDate <= period.EndDate)
                .Select(sa => new { sa.AssignmentDate, sa.ShiftId })
                .ToListAsync();

            // Lấy thông tin giờ chuẩn của từng Shift
            var shiftIds = assignments.Select(a => a.ShiftId).Distinct().ToList();
            var shiftHoursMap = shiftIds.Count > 0
                ? await _context.Shifts
                    .Where(s => shiftIds.Contains(s.ShiftId))
                    .ToDictionaryAsync(s => s.ShiftId, s => (decimal)s.WorkingHours)
                : new Dictionary<int, decimal>();

            // Xây dựng lookup: date → stdHours
            var stdHoursByDate = assignments
                .GroupBy(a => a.AssignmentDate)
                .ToDictionary(
                    g => g.Key,
                    g => shiftHoursMap.TryGetValue(g.First().ShiftId, out var h) ? h : 8m
                );

            // Bước 2: Lấy AttendanceRecord hợp lệ (không Include navigation để tránh lỗi)
            var validRecords = await _context.AttendanceRecords
                .Where(a => a.EmployeeId == employeeId
                    && a.AttendanceDate >= period.StartDate
                    && a.AttendanceDate <= period.EndDate
                    && ((a.Status == "Present" || a.Status == "Late")
                        || a.ExplanationStatus == "Approved"))
                .Select(a => new
                {
                    a.AttendanceDate,
                    a.WorkingHours,
                    a.ShiftId,
                })
                .ToListAsync();

            // Lấy ShiftId từ AttendanceRecord (phòng trường hợp AttendanceRecord có ShiftId khác với Assignment)
            var attShiftIds = validRecords.Where(r => r.ShiftId.HasValue)
                                          .Select(r => r.ShiftId!.Value)
                                          .Distinct()
                                          .Except(shiftIds)
                                          .ToList();
            if (attShiftIds.Count > 0)
            {
                var extraShifts = await _context.Shifts
                    .Where(s => attShiftIds.Contains(s.ShiftId))
                    .ToDictionaryAsync(s => s.ShiftId, s => (decimal)s.WorkingHours);
                foreach (var kv in extraShifts) shiftHoursMap[kv.Key] = kv.Value;
            }

            // Bước 3: Quy đổi sang ngày công
            decimal actualDays = 0m;
            foreach (var rec in validRecords)
            {
                // Số giờ chuẩn: ưu tiên từ ShiftAssignment → ShiftId trên record → mặc định 8
                decimal stdHours = stdHoursByDate.TryGetValue(rec.AttendanceDate, out var sh1) ? sh1
                                 : (rec.ShiftId.HasValue && shiftHoursMap.TryGetValue(rec.ShiftId.Value, out var sh2)) ? sh2
                                 : 8m;

                // Số giờ thực làm
                decimal workedHours = rec.WorkingHours.HasValue && rec.WorkingHours.Value > 0
                    ? rec.WorkingHours.Value
                    : stdHours;

                actualDays += workedHours / stdHours;
            }

            // Bước 4: Cộng ngày nghỉ có lương
            actualDays += await CalculatePaidLeaveDaysAsync(employeeId, period);

            return actualDays;
        }

        /// <summary>
        /// Tính số ngày nghỉ có lương (LeaveRequest Approved + IsPaid) trong kỳ,
        /// bỏ qua cuối tuần.
        /// </summary>
        private async Task<decimal> CalculatePaidLeaveDaysAsync(int employeeId, PayrollPeriod period)
        {
            var requests = await _context.LeaveRequests
                .Include(l => l.LeaveType)
                .Where(l => l.EmployeeId == employeeId
                    && l.Status == "Approved"
                    && l.StartDate <= period.EndDate
                    && l.EndDate >= period.StartDate
                    && l.LeaveType.IsPaid)
                .ToListAsync();

            decimal paidLeaveDays = 0;
            foreach (var req in requests)
            {
                var start = req.StartDate > period.StartDate ? req.StartDate : period.StartDate;
                var end = req.EndDate < period.EndDate ? req.EndDate : period.EndDate;
                var current = start;
                while (current <= end)
                {
                    if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                        paidLeaveDays++;
                    current = current.AddDays(1);
                }
            }
            return paidLeaveDays;
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

            // Lương giờ = BaseSalary / (số ngày phân ca × 8 giờ/ngày)
            // Dùng workingDays (số ca phân công) × 8 làm mẫu số chuẩn
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
