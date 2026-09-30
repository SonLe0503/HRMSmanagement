using Microsoft.AspNetCore.Authorization;

namespace HRManagement.Authorization
{
    /// <summary>
    /// Grants access if the current user's role has been granted ANY of the given permission keys
    /// (matches the OR semantics of the old [Authorize(Roles = "A,B")] it replaces).
    /// </summary>
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public const string PolicyPrefix = "Permission:";

        public RequirePermissionAttribute(params string[] keys)
        {
            Policy = PolicyPrefix + string.Join('|', keys);
        }
    }
}
