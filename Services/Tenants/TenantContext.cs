namespace HRManagement.Services.Tenants
{
    /// <summary>
    /// Công ty mà request / job hiện tại đang làm việc. HrmsDbContext dùng giá trị này để lọc dữ liệu.
    /// </summary>
    public interface ITenantContext
    {
        /// <summary>Null khi chưa xác định được công ty (chưa đăng nhập, tài khoản hệ thống): khi đó không đọc được dữ liệu công ty nào.</summary>
        int? CompanyId { get; }

        /// <summary>Chỉ định công ty cho phần còn lại của scope (dùng trong background job và ngay sau khi đăng nhập).</summary>
        void Use(int? companyId);
    }

    public class TenantContext : ITenantContext
    {
        /// <summary>Claim trong JWT chứa CompanyId; giá trị rỗng nghĩa là tài khoản hệ thống.</summary>
        public const string ClaimType = "companyId";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private bool _isOverridden;
        private int? _companyId;

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? CompanyId => _isOverridden ? _companyId : ReadFromClaims();

        public void Use(int? companyId)
        {
            _companyId = companyId;
            _isOverridden = true;
        }

        private int? ReadFromClaims()
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimType)?.Value;
            return int.TryParse(value, out var companyId) ? companyId : null;
        }
    }
}
