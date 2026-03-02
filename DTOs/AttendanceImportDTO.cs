public class AttendanceImportResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public string Message { get; set; } = null!;
}