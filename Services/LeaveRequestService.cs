using HRManagement.DTOs;
using HRManagement.DTOs.Common;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRManagement.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly HrmsDbContext _context;

        public LeaveRequestService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllAsync()
        {
            return await _context.LeaveRequests.ToListAsync();
        }

        public async Task<LeaveRequest?> GetByIdAsync(int id)
        {
            return await _context.LeaveRequests.FindAsync(id);
        }

        public async Task<LeaveRequest> CreateAsync(LeaveRequest request)
        {
            _context.LeaveRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<bool> UpdateAsync(int id, LeaveRequest request)
        {
            var existing = await _context.LeaveRequests.FindAsync(id);
            if (existing == null) return false;

            existing.StartDate = request.StartDate;
            existing.EndDate = request.EndDate;
            existing.Reason = request.Reason;
            existing.Status = request.Status;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.LeaveRequests.FindAsync(id);
            if (existing == null) return false;

            _context.LeaveRequests.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}