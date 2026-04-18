using HRManagement.DataAcess.Interfaces;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.DataAcess.Implementations
{
    public class PayrollRepository : IPayrollRepository
    {
        private readonly HrmsDbContext _context;

        public PayrollRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<PayrollRecord?> GetByIdAsync(int payrollRecordId)
            => await _context.PayrollRecords
                .Include(r => r.PayrollAllowances)
                .Include(r => r.PayrollDeductions)
                .Include(r => r.Employee).ThenInclude(e => e.Department)
                .Include(r => r.Employee).ThenInclude(e => e.Position)
                .FirstOrDefaultAsync(r => r.PayrollRecordId == payrollRecordId);

        public async Task<PayrollRecord?> GetByEmployeeAndPeriodAsync(int employeeId, int periodId)
            => await _context.PayrollRecords
                .Include(r => r.PayrollAllowances)
                .Include(r => r.PayrollDeductions)
                .FirstOrDefaultAsync(r => r.EmployeeId == employeeId && r.PeriodId == periodId);

        public async Task<List<PayrollRecord>> GetByPeriodAsync(int periodId)
            => await _context.PayrollRecords
                .Where(r => r.PeriodId == periodId)
                .Include(r => r.Employee).ThenInclude(e => e.Department)
                .Include(r => r.Employee).ThenInclude(e => e.Position)
                .OrderBy(r => r.Employee.FullName)
                .ToListAsync();

        public async Task<List<PayrollRecord>> GetByPeriodWithDetailsAsync(int periodId)
            => await _context.PayrollRecords
                .Where(r => r.PeriodId == periodId)
                .Include(r => r.PayrollAllowances)
                .Include(r => r.PayrollDeductions)
                .Include(r => r.Employee).ThenInclude(e => e.Department)
                .Include(r => r.Employee).ThenInclude(e => e.Position)
                .ToListAsync();

        public async Task<List<PayrollRecord>> GetByEmployeeAsync(int employeeId)
            => await _context.PayrollRecords
                .Where(r => r.EmployeeId == employeeId)
                .Include(r => r.Period)
                .OrderByDescending(r => r.Period.Year).ThenByDescending(r => r.Period.Month)
                .ToListAsync();

        public async Task<PayrollRecord> CreateAsync(PayrollRecord record)
        {
            _context.PayrollRecords.Add(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<PayrollRecord> UpdateAsync(PayrollRecord record)
        {
            _context.PayrollRecords.Update(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task DeleteAsync(int payrollRecordId)
        {
            var record = await _context.PayrollRecords.FindAsync(payrollRecordId);
            if (record != null)
            {
                _context.PayrollRecords.Remove(record);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int employeeId, int periodId)
            => await _context.PayrollRecords
                .AnyAsync(r => r.EmployeeId == employeeId && r.PeriodId == periodId);

        public async Task<int> GetCountByPeriodAsync(int periodId)
            => await _context.PayrollRecords.CountAsync(r => r.PeriodId == periodId);

        public async Task<decimal> GetTotalNetPayByPeriodAsync(int periodId)
            => await _context.PayrollRecords
                .Where(r => r.PeriodId == periodId)
                .SumAsync(r => r.NetPay ?? 0m);
    }
}
