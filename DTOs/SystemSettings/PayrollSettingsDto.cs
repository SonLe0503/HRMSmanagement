namespace HRManagement.DTOs.SystemSettings
{
    public class PayrollSettingsDto
    {
        /// <summary>
        /// Ngày chốt lương hàng tháng (1-28).
        /// Ví dụ: 5 => kỳ lương từ ngày 5 tháng này đến ngày 4 tháng sau.
        /// </summary>
        public int PayrollCutOffDay { get; set; } = 1;

        /// <summary>
        /// Số ngày mặc định nhân viên được phép review chấm công sau ngày chốt.
        /// </summary>
        public int DefaultReviewWindowDays { get; set; } = 5;
    }
}
