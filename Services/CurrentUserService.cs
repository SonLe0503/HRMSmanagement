using System;
using System.Security.Claims;

public class CurrentUserService : ICurrentUserService
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
            var value = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var result) ? result : 0;
        }
    }

    public int? EmployeeId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirst("EmployeeID")?.Value;
            return int.TryParse(value, out var result) ? result : null;
        }
    }

    public string RoleName =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}
