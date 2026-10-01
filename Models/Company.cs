namespace HRManagement.Models;

/// <summary>
/// Một công ty (tenant) sử dụng hệ thống. Mọi dữ liệu nghiệp vụ đều thuộc về đúng một công ty.
/// </summary>
public partial class Company
{
    /// <summary>Công ty được tạo sẵn khi chuyển sang multi-tenant; toàn bộ dữ liệu cũ thuộc về công ty này.</summary>
    public const int DefaultCompanyId = 1;

    public int CompanyId { get; set; }

    public string CompanyCode { get; set; } = null!;

    public string CompanyName { get; set; } = null!;

    public string? TaxCode { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
