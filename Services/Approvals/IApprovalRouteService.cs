using System.Threading.Tasks;

namespace HRManagement.Services.Approvals
{
    public interface IApprovalRouteService
    {
        /// <summary>
        /// Gets the direct approver UserID for an employee based on hierarchy or fallback rules.
        /// </summary>
        Task<int?> GetApproverIdAsync(int employeeId);
        
        /// <summary>
        /// Validates if an employee is authorized to submit a request (has a valid manager or is top-level).
        /// </summary>
        Task<(bool IsAuthorized, string? Message)> CanSubmitRequestAsync(int employeeId);
    }
}
