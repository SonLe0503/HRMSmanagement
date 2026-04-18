using HRManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.DataAcess.Interfaces
{
    public interface IPayrollPeriodRepository
    {
        Task<PayrollPeriod?> GetByIdAsync(int periodId);
        Task<PayrollPeriod?> GetByMonthYearAsync(int month, int year);
        Task<List<PayrollPeriod>> GetAllAsync();
        Task<PayrollPeriod?> GetLatestOpenPeriodAsync();
        Task<PayrollPeriod> CreateAsync(PayrollPeriod period);
        Task<PayrollPeriod> UpdateAsync(PayrollPeriod period);
        Task<bool> ExistsAsync(int month, int year);
    }
}
