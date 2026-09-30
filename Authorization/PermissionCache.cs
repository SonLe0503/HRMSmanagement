using System.Collections.Concurrent;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Authorization
{
    public interface IPermissionCache
    {
        Task<IReadOnlySet<string>> GetPermissionKeysAsync(IEnumerable<string> roleNames);
        void Invalidate();
    }

    /// <summary>
    /// Caches granted permission keys per role name. Cleared whenever role permissions change,
    /// so edits take effect on the very next request without users having to re-login.
    /// </summary>
    public class PermissionCache : IPermissionCache
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<string, IReadOnlySet<string>> _byRole = new(StringComparer.OrdinalIgnoreCase);

        public PermissionCache(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<IReadOnlySet<string>> GetPermissionKeysAsync(IEnumerable<string> roleNames)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var roleName in roleNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!_byRole.TryGetValue(roleName, out var keys))
                {
                    keys = await LoadAsync(roleName);
                    _byRole[roleName] = keys;
                }
                result.UnionWith(keys);
            }

            return result;
        }

        public void Invalidate() => _byRole.Clear();

        private async Task<IReadOnlySet<string>> LoadAsync(string roleName)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HrmsDbContext>();

            var keys = await context.RolePermissions
                .Where(rp => rp.Role.RoleName == roleName && rp.Role.IsActive && rp.Permission.IsActive)
                .Select(rp => rp.Permission.PermissionKey)
                .ToListAsync();

            return keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
