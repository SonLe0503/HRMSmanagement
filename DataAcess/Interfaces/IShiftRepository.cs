using HRManagement.Models;

namespace HRManagement.DataAcess.Interfaces
{
    public interface IShiftRepository
    {
        Task<List<Shift>> GetAllShiftsAsync(bool? isActive);
        Task<Shift?> GetShiftByIdAsync(int shiftId);
        Task<Shift?> GetShiftByCodeAsync(string shiftCode);
        System.Threading.Tasks.Task AddShiftAsync(Shift shift);
        System.Threading.Tasks.Task UpdateShiftAsync(Shift shift);
        System.Threading.Tasks.Task SaveChangesAsync();
    }
}
