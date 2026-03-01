using System.Security.Claims;

namespace HRManagement.Services
{
    public class CurrentUserService :  ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var claim = user?.FindFirst(ClaimTypes.NameIdentifier);

                if (claim == null)
                    throw new UnauthorizedAccessException("User ID not found in token");

                return int.Parse(claim.Value);
            }
        }

        public string? UserName =>
            _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }
}
