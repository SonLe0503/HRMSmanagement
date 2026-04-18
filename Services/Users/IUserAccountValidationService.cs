using HRManagement.DTOs;
using HRManagement.Models;

namespace HRManagement.Services.Users
{
    public interface IUserAccountValidationService
    {
        Task<RoleValidationResult> ValidateRoleSelectionAsync(IEnumerable<int>? roleIds);
        Task<string?> ValidateApprovalRouteAsync(Employee employee, IEnumerable<string> roleNames);
        string GenerateBaseUsername(string firstName, string lastName);
    }
}
