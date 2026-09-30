using Microsoft.AspNetCore.Authorization;

namespace HRManagement.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string[] Keys { get; }

        public PermissionRequirement(string[] keys)
        {
            Keys = keys;
        }
    }
}
