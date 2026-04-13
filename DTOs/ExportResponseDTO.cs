namespace HRManagement.DTOs
{
    public class ExportResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public byte[]? FileBytes { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;

        public bool SentToEmail { get; set; }
    }
}
