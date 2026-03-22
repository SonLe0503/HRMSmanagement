namespace HRManagement.DTOs.Attendances
{
    public class CheckOutRequestDto
    {
        public string? Location { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? Remarks { get; set; }
    }
}
