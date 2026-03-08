using HRManagement.DTOs;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly HrmsDbContext _context;

        public PayrollService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<PayrollAggregationSummaryDTO?> AggregatePayrollData(int periodId)
        {
            var period = await _context.PayrollPeriods.FindAsync(periodId);

            if (period == null)
                return null; // để controller xử lý 404

            var employees = await _context.Employees
                .Select(e => e.EmployeeId)
                .ToListAsync();

            // lấy các record đã tồn tại trong period để tránh query trong loop
            var existingEmployeeIds = await _context.PayrollRecords
                .Where(r => r.PeriodId == periodId)
                .Select(r => r.EmployeeId)
                .ToListAsync();

            int created = 0;
            int existed = 0;

            foreach (var empId in employees)
            {
                if (existingEmployeeIds.Contains(empId))
                {
                    existed++;
                    continue;
                }

                var payrollRecord = new PayrollRecord
                {
                    EmployeeId = empId,
                    PeriodId = periodId,
                    BaseSalary = 0,
                    WorkingDays = 22,
                    ActualWorkingDays = 0,
                    TotalAllowances = 0,
                    TotalDeductions = 0,
                    OvertimePay = 0,
                    BonusAmount = 0,
                    TaxAmount = 0,
                    InsuranceAmount = 0,
                    Status = "Draft"
                };

                _context.PayrollRecords.Add(payrollRecord);
                created++;
            }

            period.Status = "Aggregated";
            period.AggregatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PayrollAggregationSummaryDTO
            {
                TotalEmployeesProcessed = employees.Count,
                RecordsCreated = created,
                ExistingRecords = existed,
                Message = "Payroll data aggregated successfully"
            };
        }
        public async Task<PayrollCalculationSummaryDTO?> CalculatePayroll(int periodId)
        {
            var period = await _context.PayrollPeriods.FindAsync(periodId);

            if (period == null)
                return null;

            if (period.Status != "Aggregated")
                return new PayrollCalculationSummaryDTO
                {
                    Message = "Payroll must be aggregated before calculation"
                };

            var records = await _context.PayrollRecords
                .Where(r => r.PeriodId == periodId)
                .ToListAsync();

            int calculated = 0;
            int errors = 0;
            decimal totalPayroll = 0;

            foreach (var record in records)
            {
                try
                {
                    decimal workingPay = 0;

                    if (record.WorkingDays > 0)
                        workingPay = record.BaseSalary / record.WorkingDays * record.ActualWorkingDays;

                    decimal gross = workingPay + record.TotalAllowances + record.OvertimePay + record.BonusAmount;

                    decimal tax = gross * 0.1m;
                    decimal insurance = gross * 0.05m;

                    record.TaxAmount = tax;
                    record.InsuranceAmount = insurance;

                    record.Status = "Calculated";
                    record.CalculatedDate = DateTime.UtcNow;

                    totalPayroll += gross - record.TotalDeductions - tax - insurance;

                    calculated++;
                }
                catch
                {
                    errors++;
                }
            }

            period.Status = "Calculated";
            period.CalculatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PayrollCalculationSummaryDTO
            {
                TotalEmployees = records.Count,
                CalculatedEmployees = calculated,
                ErrorEmployees = errors,
                TotalPayrollAmount = totalPayroll,
                Message = "Payroll calculated successfully"
            };
        }
        public async Task<PayrollSummaryDTO?> GetPayrollSummary(int periodId)
        {
            var records = await _context.PayrollRecords
                .Where(x => x.PeriodId == periodId)
                .ToListAsync();

            if (!records.Any())
                return null;

            return new PayrollSummaryDTO
            {
                TotalEmployees = records.Count,
                TotalGrossPay = (decimal)records.Sum(x => x.GrossPay),
                TotalDeductions = (decimal)records.Sum(x => x.TotalDeductions + x.TaxAmount + x.InsuranceAmount),
                TotalNetPay = (decimal)records.Sum(x => x.NetPay),
                AverageNetPay = (decimal)records.Average(x => x.NetPay),
                MaxNetPay = (decimal)records.Max(x => x.NetPay),
                MinNetPay = (decimal)records.Min(x => x.NetPay)
            };
        }
        public async Task<bool> ApprovePayroll(int periodId, int approvedBy)
        {
            var period = await _context.PayrollPeriods.FindAsync(periodId);

            if (period == null)
                throw new KeyNotFoundException("Payroll period not found");

            if (period.Status != "Calculated")
                throw new InvalidOperationException("Payroll must be calculated before approval");

            period.Status = "Approved";
            period.ApprovedDate = DateTime.UtcNow;
            period.ApprovedBy = approvedBy;

            var records = await _context.PayrollRecords
                .Where(x => x.PeriodId == periodId)
                .ToListAsync();

            foreach (var record in records)
            {
                record.Status = "Approved";
                record.ApprovedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> SendBackForCorrection(int periodId, string note)
        {
            var period = await _context.PayrollPeriods.FindAsync(periodId);

            if (period == null)
                throw new KeyNotFoundException("Payroll period not found");

            if (period.Status != "Approved")
                throw new InvalidOperationException("Only approved payroll can be sent back");

            period.Status = "Open";

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PayslipGenerationSummaryDTO> GeneratePayslips(int periodId, string deliveryMethod)
        {
            var period = await _context.PayrollPeriods.FindAsync(periodId);

            if (period == null)
                throw new KeyNotFoundException("Payroll period not found");

            if (period.Status != "Approved")
                throw new InvalidOperationException("Payroll must be approved before generating payslips");

            var records = await _context.PayrollRecords
                .Where(r => r.PeriodId == periodId)
                .ToListAsync();

            var existingPayslips = await _context.Payslips
                .Where(p => p.PeriodId == periodId)
                .Select(p => p.PayrollRecordId)
                .ToListAsync();

            int generated = 0;
            int failed = 0;

            foreach (var record in records)
            {
                try
                {
                    if (existingPayslips.Contains(record.PayrollRecordId))
                        continue;

                    var payslip = new Payslip
                    {
                        PayrollRecordId = record.PayrollRecordId,
                        EmployeeId = record.EmployeeId,
                        PeriodId = periodId,
                        PayslipNumber = $"PS-{period.Year}{period.Month}-{record.EmployeeId}",
                        GeneratedDate = DateTime.UtcNow,
                        IsViewed = false
                    };

                    _context.Payslips.Add(payslip);

                    generated++;

                    if (deliveryMethod == "System" || deliveryMethod == "Both")
                    {
                        Console.WriteLine($"Notification sent to employee {record.EmployeeId}");
                    }

                    if (deliveryMethod == "Email" || deliveryMethod == "Both")
                    {
                        Console.WriteLine($"Email sent to employee {record.EmployeeId}");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine(ex.Message);
                }
            }

            period.Status = "Paid";

            await _context.SaveChangesAsync();

            return new PayslipGenerationSummaryDTO
            {
                TotalEmployees = records.Count,
                GeneratedPayslips = generated,
                FailedPayslips = failed,
                Message = "Payslips generated successfully"
            };
        }
        public async Task<List<PayslipListDTO>> GetEmployeePayslips(int employeeId)
        {
            var payslips = await _context.Payslips
                .Where(p => p.EmployeeId == employeeId)
                .Join(_context.PayrollRecords,
                    p => p.PayrollRecordId,
                    r => r.PayrollRecordId,
                    (p, r) => new PayslipListDTO
                    {
                        PayslipId = p.PayslipId,
                        PeriodId = p.PeriodId,
                        GeneratedDate = p.GeneratedDate,
                        NetPay = (decimal)r.NetPay,
                        Status = p.IsViewed ? "Viewed" : "New"
                    })
                .ToListAsync();

            return payslips;
        }
        public async Task<PayslipDetailDTO> ViewPayslip(int payslipId)
        {
            var payslip = await _context.Payslips
                .Include(p => p.PayrollRecord)
                .FirstOrDefaultAsync(p => p.PayslipId == payslipId);

            if (payslip == null)
                throw new KeyNotFoundException("Payslip not found");

            if (payslip.PayrollRecord == null)
                throw new KeyNotFoundException("Payroll record not found");

            var record = payslip.PayrollRecord;

            // Update view status
            payslip.IsViewed = true;
            payslip.ViewedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PayslipDetailDTO
            {
                PayslipId = payslip.PayslipId,
                EmployeeId = payslip.EmployeeId,
                BaseSalary = record.BaseSalary,
                TotalAllowances = record.TotalAllowances,
                OvertimePay = record.OvertimePay,
                BonusAmount = record.BonusAmount,
                TotalDeductions = record.TotalDeductions,
                TaxAmount = record.TaxAmount,
                InsuranceAmount = record.InsuranceAmount,
                NetPay = record.NetPay ?? 0
            };
        }
    }
}
