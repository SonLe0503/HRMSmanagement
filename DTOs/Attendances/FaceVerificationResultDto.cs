namespace HRManagement.DTOs.Attendances
{
    public class FaceVerificationResultDto
    {
        public bool IsMatch { get; set; }
        public decimal? ConfidenceScore { get; set; }
        public decimal ThresholdUsed { get; set; }
        public bool? LivenessPassed { get; set; }
        public string? FailureReason { get; set; }
        public string? CapturedImagePath { get; set; }
    }
}
