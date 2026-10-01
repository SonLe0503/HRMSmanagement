using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <summary>
    /// Company info (name, address, phone, email) moves from the Company.* SystemSettings rows
    /// onto each company's Companies row, so there is a single source shared with the SuperAdmin screen.
    /// </summary>
    public partial class MoveCompanyInfoToCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE c SET
    c.CompanyName = COALESCE(NULLIF(LTRIM(RTRIM(n.SettingValue)), N''), c.CompanyName),
    c.Address     = COALESCE(NULLIF(a.SettingValue, N''), c.Address),
    c.Phone       = COALESCE(NULLIF(LEFT(p.SettingValue, 20), N''), c.Phone),
    c.Email       = COALESCE(NULLIF(LEFT(e.SettingValue, 100), N''), c.Email)
FROM [Companies] c
LEFT JOIN [SystemSettings] n ON n.CompanyID = c.CompanyID AND n.SettingKey = N'Company.Name'
LEFT JOIN [SystemSettings] a ON a.CompanyID = c.CompanyID AND a.SettingKey = N'Company.Address'
LEFT JOIN [SystemSettings] p ON p.CompanyID = c.CompanyID AND p.SettingKey = N'Company.Phone'
LEFT JOIN [SystemSettings] e ON e.CompanyID = c.CompanyID AND e.SettingKey = N'Company.Email';

DELETE FROM [SystemSettings]
WHERE SettingKey IN (N'Company.Name', N'Company.Address', N'Company.Phone', N'Company.Email');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO [SystemSettings] (SettingKey, SettingValue, SettingCategory, CompanyID, ModifiedDate)
SELECT v.SettingKey, v.SettingValue, N'Company', c.CompanyID, GETDATE()
FROM [Companies] c
CROSS APPLY (VALUES
    (N'Company.Name',    c.CompanyName),
    (N'Company.Address', c.Address),
    (N'Company.Phone',   c.Phone),
    (N'Company.Email',   c.Email)
) v(SettingKey, SettingValue)
WHERE v.SettingValue IS NOT NULL;");
        }
    }
}
