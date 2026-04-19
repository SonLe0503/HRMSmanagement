namespace HRManagement.DTOs.SystemSettings
{
    /// <summary>
    /// Cấu hình các tỷ lệ tính lương — bảo hiểm và thuế TNCN.
    /// Lưu trong SystemSettings (key-value), có thể chỉnh sửa qua giao diện quản trị.
    /// </summary>
    public class PayrollCalculationSettingsDto
    {
        // ── Bảo hiểm (phần NLĐ đóng) ──────────────────────────────────────────
        /// <summary>Tỷ lệ BHXH do NLĐ đóng (%). Mặc định: 8%</summary>
        public decimal BhxhRate { get; set; } = 8m;

        /// <summary>Tỷ lệ BHYT do NLĐ đóng (%). Mặc định: 1.5%</summary>
        public decimal BhytRate { get; set; } = 1.5m;

        /// <summary>Tỷ lệ BHTN do NLĐ đóng (%). Mặc định: 1%</summary>
        public decimal BhtnRate { get; set; } = 1m;

        /// <summary>
        /// Mức trần lương đóng bảo hiểm (đồng). Mặc định: 46,800,000 (20 × LTT vùng I).
        /// Chỉ áp dụng khi InsuranceBaseMode = "Gross".
        /// </summary>
        public decimal InsuranceCap { get; set; } = 46_800_000m;

        /// <summary>
        /// Cách xác định mức lương làm căn cứ tính bảo hiểm:
        /// "Gross"  → dùng lương gộp thực tế (capped tại InsuranceCap) — đúng theo luật
        /// "Fixed"  → dùng một mức cố định (InsuranceFixedBase) bất kể lương thực tế
        /// </summary>
        public string InsuranceBaseMode { get; set; } = "Gross";

        /// <summary>
        /// Mức lương cố định làm căn cứ đóng BH (đồng).
        /// Chỉ có hiệu lực khi InsuranceBaseMode = "Fixed".
        /// Ví dụ: 7,500,000 (lương tối thiểu vùng I) hoặc mức công ty tự khai báo.
        /// </summary>
        public decimal InsuranceFixedBase { get; set; } = 0m;

        // ── Giảm trừ thuế TNCN ────────────────────────────────────────────────
        /// <summary>Giảm trừ bản thân (đồng/tháng). Mặc định: 11,000,000</summary>
        public decimal PersonalDeduction { get; set; } = 11_000_000m;

        /// <summary>Giảm trừ mỗi người phụ thuộc (đồng/tháng). Mặc định: 4,400,000</summary>
        public decimal DependentDeduction { get; set; } = 4_400_000m;
    }
}
