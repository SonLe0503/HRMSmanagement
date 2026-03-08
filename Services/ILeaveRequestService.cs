using HRManagement.DTOs;
using HRManagement.DTOs.Common;
using HRManagement.Models;
using System.Security.Claims;

namespace HRManagement.Services
{
    public interface ILeaveRequestService
    {
        Task<IEnumerable<LeaveRequest>> GetAllAsync();
        Task<LeaveRequest?> GetByIdAsync(int id);
        Task<LeaveRequest> CreateAsync(LeaveRequest request);
        Task<bool> UpdateAsync(int id, LeaveRequest request);
        Task<bool> DeleteAsync(int id);
    }
}

