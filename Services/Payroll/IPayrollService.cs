using HRManagement.DTOs.Payroll;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.Services.Payroll
{
    public interface IPayrollService
    {
        // --- Quản lý kỳ lương ---
        Task<PayrollPeriodDto> CreatePeriodAsync(CreatePayrollPeriodDto dto);
        Task<List<PayrollPeriodDto>> GetAllPeriodsAsync();
        Task<PayrollPeriodDto> GetPeriodByIdAsync(int periodId);
        Task<PayrollSummaryDto> GetPeriodSummaryAsync(int periodId);

        // --- Tính lương ---
        Task<PayrollRecordDto> CalculateForEmployeeAsync(int employeeId, int periodId);
        Task<List<PayrollRecordDto>> CalculateForAllEmployeesAsync(int periodId);
        Task<List<PayrollRecordDto>> CalculateBatchAsync(int periodId, List<int> employeeIds);

        // --- Lấy dữ liệu ---
        Task<List<PayrollRecordDto>> GetRecordsByPeriodAsync(int periodId);
        Task<PayrollRecordDto> GetRecordByIdAsync(int payrollRecordId);
        Task<List<PayrollRecordDto>> GetRecordsByEmployeeAsync(int employeeId);

        // --- Điều chỉnh thủ công ---
        Task<PayrollRecordDto> AddAllowanceAsync(int payrollRecordId, CreatePayrollAllowanceDto dto);
        Task<PayrollRecordDto> RemoveAllowanceAsync(int payrollRecordId, int allowanceId);
        Task<PayrollRecordDto> AddDeductionAsync(int payrollRecordId, CreatePayrollDeductionDto dto);
        Task<PayrollRecordDto> RemoveDeductionAsync(int payrollRecordId, int deductionId);
        Task<PayrollRecordDto> UpdateBonusAsync(int payrollRecordId, decimal bonusAmount);

        // --- Phê duyệt ---
        Task<PayrollPeriodDto> ApprovePeriodAsync(int periodId, int approvedByUserId);
        Task<PayrollRecordDto> ApproveRecordAsync(int payrollRecordId, int approvedByUserId);
        Task<int> LockAttendanceForAllApprovedPeriodsAsync();

        // --- Phiếu lương ---
        Task<int> GeneratePayslipsForPeriodAsync(int periodId);
        Task<PayslipDto> GeneratePayslipAsync(int payrollRecordId);
        Task<List<PayslipDto>> GetPayslipsByEmployeeAsync(int employeeId);
        Task<byte[]> GetPayslipPdfAsync(int payslipId);
        Task<byte[]> ExportPayrollExcelAsync(int periodId);

        // --- Tính thuế ---
        Task<TaxCalculationResultDto> CalculateTaxAsync(TaxCalculationRequestDto request);

        // --- UnderReview: Phát phiếu tạm cho NV xem ---
        Task<PayrollPeriodDto> PublishForReviewAsync(int periodId, int reviewDays);
        Task<PayrollRecordDto> GetMyRecordInPeriodAsync(int userId, int periodId);
        Task<List<PayrollPeriodDto>> GetPeriodsForEmployeeAsync(int userId);
        Task<AttendanceSummaryDto> GetMyAttendanceSummaryAsync(int userId, int periodId);

        // --- Phản hồi phiếu lương ---
        Task<PayrollFeedbackDto> SubmitFeedbackAsync(int payrollRecordId, int userId, CreatePayrollFeedbackDto dto);
        Task<List<PayrollFeedbackDto>> GetFeedbacksByPeriodAsync(int periodId);
        Task<PayrollFeedbackDto> ResolveFeedbackAsync(int feedbackId, int resolvedByUserId, ResolveFeedbackDto dto);
        Task<List<PayrollFeedbackDto>> GetMyFeedbacksAsync(int userId);
    }
}
