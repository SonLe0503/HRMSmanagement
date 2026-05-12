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

            var dto = _mapper.Map<PayrollPeriodDto>(period);
            dto.ReviewDeadline        = period.ReviewDeadline;
            dto.ReviewDeadlineExpired = period.ReviewDeadline.HasValue && DateTime.Now > period.ReviewDeadline.Value;

            var recordIds = period.PayrollRecords.Select(r => r.PayrollRecordId).ToList();
            if (recordIds.Count > 0)
            {
                var feedbacks = await _context.PayrollFeedbacks
                    .Where(f => recordIds.Contains(f.PayrollRecordId))
                    .OrderByDescending(f => f.SubmittedAt)
                    .ToListAsync();

                var latestByRecord = feedbacks
                    .GroupBy(f => f.PayrollRecordId)
                    .ToDictionary(g => g.Key, g => g.First());

                dto.AgreedCount = latestByRecord.Values.Count(f => f.IsAgreed);
                dto.AllAgreed   = latestByRecord.Count == recordIds.Count
                                  && latestByRecord.Values.All(f => f.IsAgreed);
            }

            return dto;
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

        // ── Helper: load cấu hình tính lương từ SystemSettings ───────────────
        private async Task<HRManagement.DTOs.SystemSettings.PayrollCalculationSettingsDto> LoadPayrollCalcSettingsAsync()
        {
            var keys = new[]
            {
                "Payroll.Calc.BhxhRate", "Payroll.Calc.BhytRate", "Payroll.Calc.BhtnRate",
                "Payroll.Calc.InsuranceCap", "Payroll.Calc.InsuranceBaseMode", "Payroll.Calc.InsuranceFixedBase",
                "Payroll.Calc.PersonalDeduction", "Payroll.Calc.DependentDeduction",
                "Payroll.Calc.OtWeekdayMultiplier", "Payroll.Calc.OtWeekendMultiplier", "Payroll.Calc.OtHolidayMultiplier"
            };
            var rows = await _context.SystemSettings
                .Where(s => keys.Contains(s.SettingKey))
                .ToListAsync();

            var cfg = new HRManagement.DTOs.SystemSettings.PayrollCalculationSettingsDto();
            foreach (var s in rows)
            {
                if (s.SettingKey == "Payroll.Calc.BhxhRate"              && decimal.TryParse(s.SettingValue, out var v1))  cfg.BhxhRate             = v1;
                if (s.SettingKey == "Payroll.Calc.BhytRate"              && decimal.TryParse(s.SettingValue, out var v2))  cfg.BhytRate             = v2;
                if (s.SettingKey == "Payroll.Calc.BhtnRate"              && decimal.TryParse(s.SettingValue, out var v3))  cfg.BhtnRate             = v3;
                if (s.SettingKey == "Payroll.Calc.InsuranceCap"          && decimal.TryParse(s.SettingValue, out var v4))  cfg.InsuranceCap         = v4;
                if (s.SettingKey == "Payroll.Calc.InsuranceBaseMode")                                                       cfg.InsuranceBaseMode    = s.SettingValue ?? "Gross";
                if (s.SettingKey == "Payroll.Calc.InsuranceFixedBase"    && decimal.TryParse(s.SettingValue, out var v6))  cfg.InsuranceFixedBase   = v6;
                if (s.SettingKey == "Payroll.Calc.PersonalDeduction"     && decimal.TryParse(s.SettingValue, out var v7))  cfg.PersonalDeduction    = v7;
                if (s.SettingKey == "Payroll.Calc.DependentDeduction"    && decimal.TryParse(s.SettingValue, out var v8))  cfg.DependentDeduction   = v8;
                if (s.SettingKey == "Payroll.Calc.OtWeekdayMultiplier"   && decimal.TryParse(s.SettingValue, out var v9))  cfg.OtWeekdayMultiplier  = v9;
                if (s.SettingKey == "Payroll.Calc.OtWeekendMultiplier"   && decimal.TryParse(s.SettingValue, out var v10)) cfg.OtWeekendMultiplier  = v10;
                if (s.SettingKey == "Payroll.Calc.OtHolidayMultiplier"   && decimal.TryParse(s.SettingValue, out var v11)) cfg.OtHolidayMultiplier  = v11;
            }
            return cfg;
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

            // Load cấu hình tính lương từ SystemSettings (có fallback về mặc định)
            var calcSettings = await LoadPayrollCalcSettingsAsync();

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
            var (overtimePay, otAllowances) = await CalculateOvertimeAsync(employeeId, period, baseSalary, workingDays, calcSettings);

            // 5. Thưởng & Khấu trừ thủ công từ record cũ (nếu có)
            var existingRecord = await _payrollRepo.GetByEmployeeAndPeriodAsync(employeeId, periodId);
            var bonusAmount = existingRecord?.BonusAmount ?? 0m;

            // TotalAllowances chỉ gồm phụ cấp chính sách (KHÔNG gộp OT)
            var policyAllowancesTotal = allowances.Sum(a => a.Amount);
            var grossPay = salariedAmount + policyAllowancesTotal + overtimePay + bonusAmount;

            // 6 & 7. Bảo hiểm + Thuế TNCN
            // Miễn BH và thuế khi:
            //   - Được phân ca < 14 ngày (nhân viên mới vào/nghỉ giữa tháng)
            //   - Hoặc vắng không lương >= 14 ngày (Điều 85 Luật BHXH 2014)
            var insuranceRate = (calcSettings.BhxhRate + calcSettings.BhytRate + calcSettings.BhtnRate) / 100m;
            var unpaidAbsenceDays = workingDays - (int)Math.Floor(actualWorkingDays);
            bool skipDeductions = workingDays < 14 || unpaidAbsenceDays >= 14;

            decimal insuranceAmount;
            decimal taxAmount;
            int taxBracket = 0;
            if (skipDeductions)
            {
                insuranceAmount = 0m;
                taxAmount = 0m;
            }
            else
            {
                decimal insuranceBase;
                if (employee.InsuranceSalary.HasValue && employee.InsuranceSalary.Value > 0)
                    insuranceBase = Math.Min(employee.InsuranceSalary.Value, calcSettings.InsuranceCap);
                else
                    insuranceBase = Math.Min(employee.BaseSalary ?? 0m, calcSettings.InsuranceCap);
                insuranceAmount = Math.Round(insuranceBase * insuranceRate, 0);

                var taxResult = _taxService.Calculate(
                    grossPay,
                    numberOfDependents: employee.NumberOfDependents,
                    isInsuranceApplicable: true,
                    insuranceAmount: insuranceAmount,
                    personalDeduction: calcSettings.PersonalDeduction,
                    dependentDeduction: calcSettings.DependentDeduction);
                taxAmount = taxResult.TaxAmount;
                taxBracket = taxResult.TaxBracket;
            }

            // 8. Phụ cấp & khấu trừ thủ công (giữ nguyên nếu tính lại)
            var manualAllowances = existingRecord?.PayrollAllowances
                .Where(a => a.AllowanceType == "Manual")
                .Select(a => new PayrollAllowance {
                    AllowanceType = a.AllowanceType,
                    AllowanceName = a.AllowanceName,
                    Amount = a.Amount,
                    Description = a.Description
                })
                .ToList() ?? new List<PayrollAllowance>();

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

            record.PayrollAllowances = allowances.Concat(otAllowances).Concat(manualAllowances).ToList();
            record.PayrollDeductions = manualDeductions;
            record.PayrollDeductions.Add(new PayrollDeduction
            {
                DeductionType = "Insurance",
                DeductionName = $"BHXH + BHYT + BHTN ({calcSettings.BhxhRate + calcSettings.BhytRate + calcSettings.BhtnRate}%)",
                Amount = insuranceAmount,
            });
            record.PayrollDeductions.Add(new PayrollDeduction
            {
                DeductionType = "Tax",
                DeductionName = taxBracket > 0 ? $"Thuế TNCN (Bậc {taxBracket})" : "Thuế TNCN",
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

            var results = await CalculateBatchAsync(periodId, employees);

            // Cập nhật trạng thái kỳ lương → Calculated
            // Dùng FindAsync (không Include) để tránh EF Core tracking conflict với PayrollRecords vừa được update
            var period = await _context.PayrollPeriods.FindAsync(periodId);
            if (period != null && period.Status != "Approved" && period.Status != "Closed")
            {
                period.Status = "Calculated";
                period.CalculatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return results;
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
        public async Task<List<PayrollRecordDto>> GetRecordsByPeriodAsync(int periodId, int? managerEmployeeId = null)
        {
            var records = await _payrollRepo.GetByPeriodWithDetailsAsync(periodId, managerEmployeeId);
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

            // Tự động tạo phiếu lương cho toàn bộ nhân viên trong kỳ
            var records = await _payrollRepo.GetByPeriodAsync(periodId);

            // Khoá toàn bộ bản ghi chấm công trong kỳ lương này
            var employeeIds = records.Select(r => r.EmployeeId).ToHashSet();
            var attendanceToLock = await _context.AttendanceRecords
                .Where(a => employeeIds.Contains(a.EmployeeId) &&
                            a.AttendanceDate >= period.StartDate &&
                            a.AttendanceDate <= period.EndDate)
                .ToListAsync();
            foreach (var att in attendanceToLock)
            {
                att.IsLocked = true;
                att.ModifiedDate = DateTime.Now;
            }
            foreach (var record in records)
            {
                var existing = await _context.Payslips
                    .FirstOrDefaultAsync(p => p.PayrollRecordId == record.PayrollRecordId);
                if (existing != null) continue; // Đã có rồi, bỏ qua

                _context.Payslips.Add(new Payslip
                {
                    PayrollRecordId = record.PayrollRecordId,
                    EmployeeId      = record.EmployeeId,
                    PeriodId        = periodId,
                    PayslipNumber   = $"PS-{period.Year}{period.Month:D2}-{record.PayrollRecordId:D5}",
                    GeneratedDate   = DateTime.Now,
                    IsViewed        = false,
                });
            }
            await _context.SaveChangesAsync();

            return _mapper.Map<PayrollPeriodDto>(period);
        }

        public async Task<int> LockAttendanceForAllApprovedPeriodsAsync()
        {
            var approvedPeriods = await _context.PayrollPeriods
                .Where(p => p.Status == "Approved" || p.Status == "Closed")
                .ToListAsync();

            int totalLocked = 0;
            foreach (var period in approvedPeriods)
            {
                var employeeIds = await _context.PayrollRecords
                    .Where(r => r.PeriodId == period.PeriodId)
                    .Select(r => r.EmployeeId)
                    .ToListAsync();

                var toLock = await _context.AttendanceRecords
                    .Where(a => employeeIds.Contains(a.EmployeeId) &&
                                a.AttendanceDate >= period.StartDate &&
                                a.AttendanceDate <= period.EndDate &&
                                (a.IsLocked == null || a.IsLocked == false))
                    .ToListAsync();

                foreach (var att in toLock)
                {
                    att.IsLocked = true;
                    att.ModifiedDate = DateTime.Now;
                }
                totalLocked += toLock.Count;
            }

            await _context.SaveChangesAsync();
            return totalLocked;
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

        public async Task<int> GeneratePayslipsForPeriodAsync(int periodId)
        {
            var period = await _periodRepo.GetByIdAsync(periodId)
                ?? throw new KeyNotFoundException("Không tìm thấy kỳ lương.");

            var records = await _payrollRepo.GetByPeriodAsync(periodId);
            int created = 0;

            foreach (var record in records)
            {
                var existing = await _context.Payslips
                    .FirstOrDefaultAsync(p => p.PayrollRecordId == record.PayrollRecordId);
                if (existing != null) continue;

                _context.Payslips.Add(new Payslip
                {
                    PayrollRecordId = record.PayrollRecordId,
                    EmployeeId      = record.EmployeeId,
                    PeriodId        = periodId,
                    PayslipNumber   = $"PS-{period.Year}{period.Month:D2}-{record.PayrollRecordId:D5}",
                    GeneratedDate   = DateTime.Now,
                    IsViewed        = false,
                });
                created++;
            }

            await _context.SaveChangesAsync();
            return created;
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

            // Lấy thông tin công ty từ SystemSettings
            var companySettings = new HRManagement.DTOs.SystemSettings.CompanySettingsDto
            {
                CompanyName = "CÔNG TY CỔ PHẦN HR SYSTEM",
                Address = "",
                Phone = "",
                Email = ""
            };
            var settingKeys = new[] { "Company.Name", "Company.Address", "Company.Phone", "Company.Email" };
            var rawSettings = await _context.SystemSettings
                .Where(s => settingKeys.Contains(s.SettingKey))
                .ToListAsync();
            foreach (var s in rawSettings)
            {
                if (s.SettingKey == "Company.Name")    companySettings.CompanyName = s.SettingValue;
                if (s.SettingKey == "Company.Address") companySettings.Address     = s.SettingValue;
                if (s.SettingKey == "Company.Phone")   companySettings.Phone       = s.SettingValue;
                if (s.SettingKey == "Company.Email")   companySettings.Email       = s.SettingValue;
            }

            var pdfService = new PayslipPdfService();
            return pdfService.GeneratePdf(payslip.PayrollRecord, payslip.Period, companySettings);
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
                    .ThenInclude(r => r.PayrollDeductions)
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

            return assignedDays;
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
            CalculateOvertimeAsync(int employeeId, PayrollPeriod period, decimal baseSalary, decimal workingDays,
                HRManagement.DTOs.SystemSettings.PayrollCalculationSettingsDto cfg)
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
                    DayOfWeek.Saturday or DayOfWeek.Sunday => cfg.OtWeekendMultiplier,
                    _ => cfg.OtWeekdayMultiplier,
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

        // ── UnderReview ────────────────────────────────────────────────────────

        public async Task<PayrollPeriodDto> PublishForReviewAsync(int periodId, int reviewDays)
        {
            var period = await _periodRepo.GetByIdAsync(periodId)
                ?? throw new KeyNotFoundException("Không tìm thấy kỳ lương.");

            if (period.Status != "Calculated")
                throw new InvalidOperationException("Chỉ có thể gửi NV xem khi kỳ lương ở trạng thái Đã tính lương.");

            period.Status         = "UnderReview";
            period.ReviewDeadline = DateTime.Now.AddDays(reviewDays);
            await _periodRepo.UpdateAsync(period);

            var dto = _mapper.Map<PayrollPeriodDto>(period);
            dto.ReviewDeadline        = period.ReviewDeadline;
            dto.ReviewDeadlineExpired = false;
            return dto;
        }

        public async Task<PayrollRecordDto> GetMyRecordInPeriodAsync(int userId, int periodId)
        {
            var employeeId = await _context.Users
                .Where(u => u.UserId == userId && u.IsActive)
                .Select(u => u.EmployeeId)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Tài khoản chưa liên kết nhân viên.");

            var period = await _context.PayrollPeriods.FindAsync(periodId)
                ?? throw new KeyNotFoundException("Không tìm thấy kỳ lương.");

            if (period.Status == "Open" || period.Status == "Aggregated" || period.Status == "Calculated")
                throw new InvalidOperationException("Phiếu lương chưa được phát để xem.");

            var record = await _context.PayrollRecords
                .Include(r => r.PayrollAllowances)
                .Include(r => r.PayrollDeductions)
                .Include(r => r.Employee).ThenInclude(e => e.Department)
                .Include(r => r.Employee).ThenInclude(e => e.Position)
                .FirstOrDefaultAsync(r => r.EmployeeId == employeeId && r.PeriodId == periodId)
                ?? throw new KeyNotFoundException("Không tìm thấy bản ghi lương của bạn trong kỳ này.");

            var dto = _mapper.Map<PayrollRecordDto>(record);
            dto.PeriodStatus = period.Status;
            return dto;
        }

        public async Task<List<PayrollPeriodDto>> GetPeriodsForEmployeeAsync(int userId)
        {
            var employeeId = await _context.Users
                .Where(u => u.UserId == userId && u.IsActive)
                .Select(u => u.EmployeeId)
                .FirstOrDefaultAsync();

            if (employeeId == null) return new List<PayrollPeriodDto>();

            var periodIds = await _context.PayrollRecords
                .Where(r => r.EmployeeId == employeeId)
                .Select(r => r.PeriodId)
                .Distinct()
                .ToListAsync();

            var periods = await _context.PayrollPeriods
                .Where(p => periodIds.Contains(p.PeriodId))
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                .ToListAsync();

            return _mapper.Map<List<PayrollPeriodDto>>(periods);
        }

        public async Task<AttendanceSummaryDto> GetMyAttendanceSummaryAsync(int userId, int periodId)
        {
            var employeeId = await _context.Users
                .Where(u => u.UserId == userId && u.IsActive)
                .Select(u => u.EmployeeId)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Tài khoản chưa liên kết nhân viên.");

            var period = await _context.PayrollPeriods.FindAsync(periodId)
                ?? throw new KeyNotFoundException("Không tìm thấy kỳ lương.");

            var startDt = period.StartDate.ToDateTime(TimeOnly.MinValue);
            var endDt   = period.EndDate.ToDateTime(TimeOnly.MaxValue);

            // Attendance records
            var attendanceRecords = await _context.AttendanceRecords
                .Where(a => a.EmployeeId == employeeId
                    && a.AttendanceDate >= period.StartDate
                    && a.AttendanceDate <= period.EndDate)
                .OrderBy(a => a.AttendanceDate)
                .ToListAsync();

            // Approved leave requests
            var leaveRequests = await _context.LeaveRequests
                .Include(l => l.LeaveType)
                .Where(l => l.EmployeeId == employeeId
                    && l.Status == "Approved"
                    && l.StartDate <= period.EndDate
                    && l.EndDate >= period.StartDate)
                .OrderBy(l => l.StartDate)
                .ToListAsync();

            // Approved overtime requests
            var overtimeRequests = await _context.OvertimeRequests
                .Where(o => o.EmployeeId == employeeId
                    && o.Status == "Approved"
                    && o.OvertimeDate >= period.StartDate
                    && o.OvertimeDate <= period.EndDate)
                .OrderBy(o => o.OvertimeDate)
                .ToListAsync();

            int presentDays = attendanceRecords.Count(a => a.Status == "Present");
            int lateDays    = attendanceRecords.Count(a => a.Status == "Late");
            int absentDays  = attendanceRecords.Count(a => a.Status == "Absent" && a.ExplanationStatus != "Approved");
            int explanationApprovedDays = attendanceRecords.Count(a => a.ExplanationStatus == "Approved");
            decimal overtimeHours = overtimeRequests.Sum(o => o.TotalHours);

            // Tính paid leave days (weekdays only, within period)
            decimal paidLeaveDays = 0;
            foreach (var leave in leaveRequests.Where(l => l.LeaveType?.IsPaid == true))
            {
                var s = leave.StartDate > period.StartDate ? leave.StartDate : period.StartDate;
                var e = leave.EndDate < period.EndDate ? leave.EndDate : period.EndDate;
                for (var d = s; d <= e; d = d.AddDays(1))
                {
                    if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                        paidLeaveDays++;
                }
            }

            var shiftCount = await _context.ShiftAssignments
                .Where(sa => sa.EmployeeId == employeeId
                    && sa.AssignmentDate >= period.StartDate
                    && sa.AssignmentDate <= period.EndDate)
                .CountAsync();

            var totalActualDays = (decimal)(presentDays + lateDays + explanationApprovedDays) + paidLeaveDays;

            return new AttendanceSummaryDto
            {
                Records = attendanceRecords.Select(a => new AttendanceItemDto
                {
                    Date                 = a.AttendanceDate.ToString("yyyy-MM-dd"),
                    Status               = a.Status ?? "",
                    WorkingHours         = (decimal)(a.WorkingHours ?? 0),
                    IsExplanationApproved = a.ExplanationStatus == "Approved",
                }).ToList(),
                ApprovedLeaves = leaveRequests.Select(l => new LeaveItemDto
                {
                    StartDate     = l.StartDate.ToString("yyyy-MM-dd"),
                    EndDate       = l.EndDate.ToString("yyyy-MM-dd"),
                    LeaveTypeName = l.LeaveType?.LeaveTypeName ?? "",
                    IsPaid        = l.LeaveType?.IsPaid ?? false,
                    Days          = (decimal)(l.EndDate.DayNumber - l.StartDate.DayNumber + 1),
                }).ToList(),
                ApprovedOvertime = overtimeRequests.Select(o => new OvertimeItemDto
                {
                    Date  = o.OvertimeDate.ToString("yyyy-MM-dd"),
                    Hours = o.TotalHours,
                }).ToList(),
                Totals = new AttendanceTotalsDto
                {
                    PresentDays              = presentDays,
                    LateDays                 = lateDays,
                    AbsentDays               = absentDays,
                    ExplanationApprovedDays  = explanationApprovedDays,
                    PaidLeaveDays            = paidLeaveDays,
                    OvertimeHours            = overtimeHours,
                    TotalActualDays          = totalActualDays,
                },
            };
        }

        // ── Phản hồi phiếu lương ──────────────────────────────────────────────

        public async Task<PayrollFeedbackDto> SubmitFeedbackAsync(int payrollRecordId, int userId, CreatePayrollFeedbackDto dto)
        {
            var employeeId = await _context.Users
                .Where(u => u.UserId == userId && u.IsActive)
                .Select(u => u.EmployeeId)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Tài khoản chưa liên kết nhân viên.");

            var record = await _context.PayrollRecords
                .Include(r => r.Period)
                .FirstOrDefaultAsync(r => r.PayrollRecordId == payrollRecordId)
                ?? throw new KeyNotFoundException("Không tìm thấy bản ghi lương.");

            if (record.Period.Status != "UnderReview")
                throw new InvalidOperationException("Chỉ có thể gửi phản hồi khi kỳ lương đang ở trạng thái Chờ xem xét.");

            if (record.EmployeeId != employeeId)
                throw new UnauthorizedAccessException("Bạn không có quyền gửi phản hồi cho bản ghi này.");

            var feedback = new PayrollFeedback
            {
                PayrollRecordId = payrollRecordId,
                EmployeeId      = employeeId,
                Content         = dto.Content,
                IsAgreed        = dto.IsAgreed,
                SubmittedAt     = DateTime.Now,
                Status          = dto.IsAgreed ? "Resolved" : "Pending",
            };

            _context.PayrollFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            await _context.Entry(feedback).Reference(f => f.Employee).LoadAsync();
            await _context.Entry(feedback.Employee).Reference(e => e.Department).LoadAsync();

            return BuildFeedbackDto(feedback, record);
        }

        public async Task<List<PayrollFeedbackDto>> GetFeedbacksByPeriodAsync(int periodId)
        {
            var feedbacks = await _context.PayrollFeedbacks
                .Include(f => f.Employee).ThenInclude(e => e.Department)
                .Include(f => f.ResolvedByUser).ThenInclude(u => u.Employee)
                .Include(f => f.PayrollRecord).ThenInclude(r => r.Period)
                .Where(f => f.PayrollRecord.PeriodId == periodId)
                .OrderBy(f => f.Status == "Pending" ? 0 : 1)
                .ThenByDescending(f => f.SubmittedAt)
                .ToListAsync();

            return feedbacks.Select(f => BuildFeedbackDto(f, f.PayrollRecord)).ToList();
        }

        public async Task<PayrollFeedbackDto> ResolveFeedbackAsync(int feedbackId, int resolvedByUserId, ResolveFeedbackDto dto)
        {
            if (dto.Status != "Resolved" && dto.Status != "Dismissed")
                throw new ArgumentException("Trạng thái không hợp lệ. Chỉ chấp nhận 'Resolved' hoặc 'Dismissed'.");

            var feedback = await _context.PayrollFeedbacks
                .Include(f => f.Employee).ThenInclude(e => e.Department)
                .Include(f => f.ResolvedByUser).ThenInclude(u => u.Employee)
                .Include(f => f.PayrollRecord).ThenInclude(r => r.Period)
                .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId)
                ?? throw new KeyNotFoundException("Không tìm thấy phản hồi.");

            feedback.Status          = dto.Status;
            feedback.HrResponse      = dto.HrResponse;
            feedback.ResolvedAt      = DateTime.Now;
            feedback.ResolvedByUserId = resolvedByUserId;

            await _context.SaveChangesAsync();

            // Reload ResolvedByUser nếu chưa có
            if (feedback.ResolvedByUser == null)
                await _context.Entry(feedback).Reference(f => f.ResolvedByUser).LoadAsync();

            return BuildFeedbackDto(feedback, feedback.PayrollRecord);
        }

        public async Task<List<PayrollFeedbackDto>> GetMyFeedbacksAsync(int userId)
        {
            var employeeId = await _context.Users
                .Where(u => u.UserId == userId && u.IsActive)
                .Select(u => u.EmployeeId)
                .FirstOrDefaultAsync();

            if (employeeId == null) return new List<PayrollFeedbackDto>();

            var feedbacks = await _context.PayrollFeedbacks
                .Include(f => f.Employee).ThenInclude(e => e.Department)
                .Include(f => f.ResolvedByUser).ThenInclude(u => u.Employee)
                .Include(f => f.PayrollRecord).ThenInclude(r => r.Period)
                .Where(f => f.EmployeeId == employeeId)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();

            return feedbacks.Select(f => BuildFeedbackDto(f, f.PayrollRecord)).ToList();
        }

        private PayrollFeedbackDto BuildFeedbackDto(PayrollFeedback f, PayrollRecord record)
        {
            var resolvedByName = f.ResolvedByUser?.Employee?.FullName ?? f.ResolvedByUser?.Username;
            return new PayrollFeedbackDto
            {
                FeedbackId      = f.FeedbackId,
                PayrollRecordId = f.PayrollRecordId,
                EmployeeId      = f.EmployeeId,
                EmployeeName    = f.Employee?.FullName ?? "",
                EmployeeCode    = f.Employee?.EmployeeCode ?? "",
                DepartmentName  = f.Employee?.Department?.DepartmentName ?? "",
                Content         = f.Content,
                IsAgreed        = f.IsAgreed,
                SubmittedAt     = f.SubmittedAt,
                Status          = f.Status,
                HrResponse      = f.HrResponse,
                ResolvedAt      = f.ResolvedAt,
                ResolvedByName  = resolvedByName,
                NetPay          = record?.NetPay ?? 0,
                PeriodLabel     = record?.Period != null
                    ? $"Tháng {record.Period.Month}/{record.Period.Year}"
                    : "",
            };
        }
    }
}
