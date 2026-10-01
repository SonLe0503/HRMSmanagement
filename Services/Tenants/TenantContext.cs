using HRManagement.Authorization;

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
            var http = _httpContextAccessor.HttpContext;
            var value = http?.User?.FindFirst(ClaimType)?.Value;
            if (int.TryParse(value, out var companyId))
                return companyId;

            // SuperAdmin has no company of its own; on opted-in endpoints it picks one via header
            if (http != null
                && http.User.IsInRole(PlatformRoles.SuperAdmin)
                && http.GetEndpoint()?.Metadata.GetMetadata<SuperAdminCompanyScopeAttribute>() != null
                && int.TryParse(http.Request.Headers[SuperAdminCompanyScopeAttribute.HeaderName], out var scopedCompanyId))
            {
                return scopedCompanyId;
            }

            return null;
        }
    }
}
