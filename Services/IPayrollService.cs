using HRManagement.DTOs;

namespace HRManagement.Services
{
    public interface IPayrollService
    {
        Task<PayrollAggregationSummaryDTO> AggregatePayrollData(int periodId);
        Task<PayrollCalculationSummaryDTO> CalculatePayroll(int periodId);

        Task<PayrollSummaryDTO> GetPayrollSummary(int periodId);

        Task<bool> ApprovePayroll(int periodId, int approvedBy);

        Task<bool> SendBackForCorrection(int periodId, string note);

        Task<PayslipGenerationSummaryDTO> GeneratePayslips(int periodId, string deliveryMethod);

        Task<List<PayslipListDTO>> GetEmployeePayslips(int employeeId);

        Task<PayslipDetailDTO> ViewPayslip(int payslipId);
    }
}
