using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs
{
    public class CompanyListItemDTO
    {
        public int CompanyId { get; set; }
        public string CompanyCode { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string? TaxCode { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int EmployeeCount { get; set; }
        public int UserCount { get; set; }
        /// <summary>Các tài khoản ADMIN của công ty.</summary>
        public List<CompanyAdminDTO> Admins { get; set; } = new();
    }

    public class CompanyAdminDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    public class ResetAdminPasswordResultDTO
    {
        public string Username { get; set; } = null!;
        /// <summary>Mật khẩu tạm mới, chỉ trả về một lần (đồng thời được gửi qua email).</summary>
        public string TemporaryPassword { get; set; } = null!;
    }

    public class CompanyWriteDTO
    {
        [Required]
        [MaxLength(20)]
        [RegularExpression("^[A-Za-z0-9_-]+$", ErrorMessage = "Mã công ty chỉ gồm chữ, số, '-' và '_'.")]
        public string CompanyCode { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; } = null!;

        [MaxLength(20)]
        public string? TaxCode { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }
    }

    public class CreateCompanyAdminDTO
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }

    public class CreateCompanyDTO : CompanyWriteDTO
    {
        [Required]
        public CreateCompanyAdminDTO Admin { get; set; } = null!;
    }

    public class CreateCompanyResultDTO
    {
        public int CompanyId { get; set; }
        public string AdminUsername { get; set; } = null!;
        /// <summary>Mật khẩu tạm của admin, chỉ trả về một lần lúc tạo (đồng thời được gửi qua email).</summary>
        public string TemporaryPassword { get; set; } = null!;
    }

    public class UpdateCompanyStatusDTO
    {
        public bool IsActive { get; set; }
    }
}
