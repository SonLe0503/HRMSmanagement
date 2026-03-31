using HRManagement.Models;

namespace HRManagement.DataAcess.Interfaces
{
    public interface IHRProcedureRepository
    {
        Task<IEnumerable<Hrprocedure>> GetAllAsync();
        Task<Hrprocedure?> GetByIdWithDetailsAsync(int procedureId);
        Task<Hrprocedure?> GetByIdAsync(int procedureId);
        Task<IEnumerable<Hrprocedure>> GetPendingProceduresAsync();
        Task<IEnumerable<Hrprocedure>> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<Hrprocedure>> GetByStatusAsync(string status);
        Task<bool> HasActiveProcedureAsync(int employeeId, string procedureType);
        Task<Hrprocedure> AddAsync(Hrprocedure procedure);
        Task<Hrprocedure> UpdateAsync(Hrprocedure procedure);
        Task<bool> DeleteAsync(int procedureId);
        Task<bool> ExistsAsync(int procedureId);
        Task<bool> EmployeeExistsAsync(int employeeId);
        Task<bool> DepartmentExistsAsync(int departmentId);
        Task<bool> PositionExistsAsync(int positionId);
    }
}
