using HRManagement.DTOs;
using HRManagement.Models;

namespace HRManagement.Services.Users
{
    public interface IUserAccountValidationService
    {
        Task<RoleValidationResult> ValidateRoleSelectionAsync(IEnumerable<int>? roleIds);
        string? ValidateDirectManagerRequirement(Employee employee, IEnumerable<string> roleNames);
        string GenerateBaseUsername(string firstName, string lastName);
    }
}
