using HRManagement.DTOs.LeaveRequest;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services
{
    public class LeaveTypeService : ILeaveTypeService
    {
        private readonly HrmsDbContext _context;

        public LeaveTypeService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LeaveTypeDTO>> GetActiveLeaveTypesAsync()
        {
            return await _context.LeaveTypes
                .Where(lt => lt.IsActive)
                .Select(lt => new LeaveTypeDTO
                {
                    LeaveTypeId = lt.LeaveTypeId,
                    LeaveTypeCode = lt.LeaveTypeCode,
                    LeaveTypeName = lt.LeaveTypeName
                })
                .ToListAsync();
        }
    }
}