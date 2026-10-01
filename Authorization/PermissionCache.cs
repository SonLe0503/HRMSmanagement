using System.Collections.Concurrent;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Authorization
{
    public interface IPermissionCache
    {
        Task<IReadOnlySet<string>> GetPermissionKeysAsync(int companyId, IEnumerable<string> roleNames);
        void Invalidate();
    }

    /// <summary>
    /// Caches granted permission keys per (company, role name): each company has its own roles,
    /// so the same role name can carry different permissions. Cleared whenever role permissions change,
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

        public async Task<IReadOnlySet<string>> GetPermissionKeysAsync(int companyId, IEnumerable<string> roleNames)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var roleName in roleNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var cacheKey = $"{companyId}:{roleName}";
                if (!_byRole.TryGetValue(cacheKey, out var keys))
                {
                    keys = await LoadAsync(companyId, roleName);
                    _byRole[cacheKey] = keys;
                }
                result.UnionWith(keys);
            }

            return result;
        }

        public void Invalidate() => _byRole.Clear();

        private async Task<IReadOnlySet<string>> LoadAsync(int companyId, string roleName)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HrmsDbContext>();

            // Company is passed explicitly instead of relying on the request's tenant filter
            var keys = await context.RolePermissions
                .IgnoreQueryFilters()
                .Where(rp => rp.Role.CompanyId == companyId && rp.Role.RoleName == roleName
                    && rp.Role.IsActive && rp.Permission.IsActive)
                .Select(rp => rp.Permission.PermissionKey)
                .ToListAsync();

            return keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
