using HRManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.DataAcess.Interfaces
{
    public interface IPayrollRepository
    {
        Task<PayrollRecord?> GetByIdAsync(int payrollRecordId);
        Task<PayrollRecord?> GetByEmployeeAndPeriodAsync(int employeeId, int periodId);
        Task<List<PayrollRecord>> GetByPeriodAsync(int periodId);
        Task<List<PayrollRecord>> GetByPeriodWithDetailsAsync(int periodId);  // Include Allowances, Deductions
        Task<List<PayrollRecord>> GetByEmployeeAsync(int employeeId);
        Task<PayrollRecord> CreateAsync(PayrollRecord record);
        Task<PayrollRecord> UpdateAsync(PayrollRecord record);
        Task DeleteAsync(int payrollRecordId);
        Task<bool> ExistsAsync(int employeeId, int periodId);
        Task<int> GetCountByPeriodAsync(int periodId);
        Task<decimal> GetTotalNetPayByPeriodAsync(int periodId);
    }
}
