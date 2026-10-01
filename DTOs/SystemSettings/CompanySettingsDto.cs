using HRManagement.Models;

namespace HRManagement.DTOs.SystemSettings
{
    public class CompanySettingsDto
    {
        public string CompanyName { get; set; } = "CÔNG TY CỔ PHẦN HR SYSTEM";
        public string Address    { get; set; } = "";
        public string Phone      { get; set; } = "";
        public string Email      { get; set; } = "";
        public string? TaxCode   { get; set; }

        public static CompanySettingsDto FromCompany(Company company) => new()
        {
            CompanyName = company.CompanyName,
            Address     = company.Address ?? "",
            Phone       = company.Phone ?? "",
            Email       = company.Email ?? "",
            TaxCode     = company.TaxCode
        };
    }
}
