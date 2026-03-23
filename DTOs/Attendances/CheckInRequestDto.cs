namespace HRManagement.DTOs.Attendances
{
    public class CheckInRequestDto
    {
        public string? Location { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? Remarks { get; set; }

        public string? FaceImageBase64 { get; set; }

    }
}
