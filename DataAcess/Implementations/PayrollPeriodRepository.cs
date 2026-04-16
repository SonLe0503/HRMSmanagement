using HRManagement.DataAcess.Interfaces;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRManagement.DataAcess.Implementations
{
    public class PayrollPeriodRepository : IPayrollPeriodRepository
    {
        private readonly HrmsDbContext _context;

        public PayrollPeriodRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<PayrollPeriod?> GetByIdAsync(int periodId)
            => await _context.PayrollPeriods
                .Include(p => p.PayrollRecords)
                .FirstOrDefaultAsync(p => p.PeriodId == periodId);

        public async Task<PayrollPeriod?> GetByMonthYearAsync(int month, int year)
            => await _context.PayrollPeriods
                .FirstOrDefaultAsync(p => p.Month == month && p.Year == year);

        public async Task<List<PayrollPeriod>> GetAllAsync()
            => await _context.PayrollPeriods
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                .ToListAsync();

        public async Task<PayrollPeriod?> GetLatestOpenPeriodAsync()
            => await _context.PayrollPeriods
                .Where(p => p.Status == "Open")
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                .FirstOrDefaultAsync();

        public async Task<PayrollPeriod> CreateAsync(PayrollPeriod period)
        {
            _context.PayrollPeriods.Add(period);
            await _context.SaveChangesAsync();
            return period;
        }

        public async Task<PayrollPeriod> UpdateAsync(PayrollPeriod period)
        {
            _context.PayrollPeriods.Update(period);
            await _context.SaveChangesAsync();
            return period;
        }

        public async Task<bool> ExistsAsync(int month, int year)
            => await _context.PayrollPeriods.AnyAsync(p => p.Month == month && p.Year == year);
    }
}
