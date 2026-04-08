namespace HRManagement.DTOs
{
    public class ExportRequestDTO
    {
        public string Module { get; set; } = string.Empty; // dashboard, workforce, cost, payroll, leave, attendance
        public string Format { get; set; } = string.Empty; // csv, excel, pdf, powerpoint
        public string Scope { get; set; } = "current";     // current, all, custom
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public bool IncludeCharts { get; set; }
        public bool IncludeRawData { get; set; } = true;
        public bool AddCompanyBranding { get; set; }
        public bool PasswordProtect { get; set; }

        public bool SendToEmail { get; set; }
        public string? EmailAddress { get; set; }

        public string SourceScreen { get; set; } = "SR-18";
    }
}
