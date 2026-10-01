using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <summary>
    /// Role, permission and menu management moved to the SuperAdmin: company roles lose those menus,
    /// and the permission keys that used to guard them are retired.
    /// </summary>
    public partial class MovePermissionManagementToSuperAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE rm FROM [RoleMenus] rm
JOIN [Menus] m ON m.MenuID = rm.MenuID
WHERE m.Code IN (N'system.roles', N'system.permissions', N'system.menus');

UPDATE [Permissions] SET IsActive = 0
WHERE PermissionKey IN (N'Role.Manage', N'Menu.Manage');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [Permissions] SET IsActive = 1
WHERE PermissionKey IN (N'Role.Manage', N'Menu.Manage');

INSERT INTO [RoleMenus] (RoleID, MenuID)
SELECT r.RoleID, m.MenuID
FROM [Roles] r
CROSS JOIN [Menus] m
WHERE r.RoleName = N'ADMIN'
  AND m.Code IN (N'system.roles', N'system.permissions', N'system.menus')
  AND NOT EXISTS (SELECT 1 FROM [RoleMenus] x WHERE x.RoleID = r.RoleID AND x.MenuID = m.MenuID);");
        }
    }
}
