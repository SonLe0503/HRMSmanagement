namespace HRManagement.DTOs.Attendances
{
    public class CheckInRequestDto
    {
        public string? Location { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Remarks { get; set; }

        public string? FaceImageBase64 { get; set; }

    }
}
