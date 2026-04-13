using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.OvertimeRequest
{
    /// <summary>
    /// OTType: NormalDay | DayOff | Holiday  (auto-detected by backend — client can leave null)
    /// OTMode: AfterShift | BeforeShift | FullRange
    ///   AfterShift  → client sends EndTime only; StartTime = ShiftEnd (computed by backend)
    ///   BeforeShift → client sends StartTime only; EndTime = ShiftStart (computed by backend)
    ///   FullRange   → client sends both StartTime and EndTime (used on DayOff / Holiday)
    /// </summary>
    public class CreateOvertimeRequestDTO
    {
        [Required]
        public DateOnly OvertimeDate { get; set; }

        /// <summary>AfterShift or FullRange: the end time of overtime.</summary>
        public TimeOnly? EndTime { get; set; }

        /// <summary>BeforeShift or FullRange: the start time of overtime.</summary>
        public TimeOnly? StartTime { get; set; }

        /// <summary>
        /// AfterShift | BeforeShift | FullRange
        /// Defaults to FullRange when no shift exists on that day.
        /// </summary>
        public string OTMode { get; set; } = "FullRange";

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? TaskDescription { get; set; }
    }
}
