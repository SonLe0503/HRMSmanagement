namespace HRManagement.DTOs.SystemSettings
{
    public class LocationSettingsDto
    {
        public double OfficeLatitude { get; set; }
        public double OfficeLongitude { get; set; }
        public double AttendanceAllowedRadius { get; set; }
        // "Location" | "IP" | "Either"
        public string CheckInMethod { get; set; } = "Location";
        public string? AllowedIpAddresses { get; set; }
    }
}
