using HRManagement.DTOs;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace HRManagement.Services.Users
{
    public class UserAccountValidationService : IUserAccountValidationService
    {
        private readonly HrmsDbContext _context;
        private readonly Approvals.IApprovalRouteService _approvalRouteService;

        public UserAccountValidationService(HrmsDbContext context, Approvals.IApprovalRouteService approvalRouteService)
        {
            _context = context;
            _approvalRouteService = approvalRouteService;
        }

        public async Task<RoleValidationResult> ValidateRoleSelectionAsync(IEnumerable<int>? roleIds)
        {
            var requestedRoleIds = roleIds?
                .Distinct()
                .ToList() ?? new List<int>();

            if (!requestedRoleIds.Any())
            {
                return new RoleValidationResult
                {
                    IsValid = true,
                    ValidRoles = new List<Role>(),
                    RoleNames = new List<string>()
                };
            }

            var roles = await _context.Roles
                .Where(r => requestedRoleIds.Contains(r.RoleId))
                .ToListAsync();

            if (roles.Count != requestedRoleIds.Count)
            {
                return new RoleValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "One or more selected roles are invalid.",
                    ValidRoles = new List<Role>(),
                    RoleNames = new List<string>()
                };
            }

            return new RoleValidationResult
            {
                IsValid = true,
                ValidRoles = roles,
                RoleNames = roles.Select(r => r.RoleName).ToList()
            };
        }

        public async Task<string?> ValidateApprovalRouteAsync(Employee employee, IEnumerable<string> roleNames)
        {
            var requiresApprovalRoute = roleNames.Any(roleName =>
                string.Equals(roleName, "EMPLOYEE", StringComparison.OrdinalIgnoreCase));

            if (!requiresApprovalRoute)
                return null;

            var result = await _approvalRouteService.CanSubmitRequestAsync(employee.EmployeeId);
            if (result.IsAuthorized)
                return null;

            return "Employee has no direct manager and no fallback approver configured. Please assign a manager or update approval settings.";
        }

        public string GenerateBaseUsername(string firstName, string lastName)
        {
            var raw = $"{lastName}{firstName}";
            raw = RemoveDiacritics(raw);
            raw = raw.Replace(" ", string.Empty).ToLowerInvariant();

            return raw;
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in text)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd')
                .Replace('Đ', 'D');
        }
    }
}
