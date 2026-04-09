namespace HRManagement.Configuration
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public string ApiSecret { get; set; } = null!;
        public string FolderName { get; set; } = "hrms/employee-documents";
        public bool CheckUrl { get; set; } = true;
    }
}
