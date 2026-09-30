using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionsAndMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    MenuID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentID = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Route = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IconName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.MenuID);
                    table.ForeignKey(
                        name: "FK_Menus_Parent",
                        column: x => x.ParentID,
                        principalTable: "Menus",
                        principalColumn: "MenuID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionID);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenus",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    MenuID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenus", x => new { x.RoleID, x.MenuID });
                    table.ForeignKey(
                        name: "FK_RoleMenus_Menus",
                        column: x => x.MenuID,
                        principalTable: "Menus",
                        principalColumn: "MenuID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleMenus_Roles",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RolePermissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    PermissionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.RolePermissionID);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions",
                        column: x => x.PermissionID,
                        principalTable: "Permissions",
                        principalColumn: "PermissionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_Code",
                table: "Menus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menus_ParentID_DisplayOrder",
                table: "Menus",
                columns: new[] { "ParentID", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionKey",
                table: "Permissions",
                column: "PermissionKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenus_MenuID",
                table: "RoleMenus",
                column: "MenuID");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionID",
                table: "RolePermissions",
                column: "PermissionID");

            migrationBuilder.CreateIndex(
                name: "UQ_RolePermissions",
                table: "RolePermissions",
                columns: new[] { "RoleID", "PermissionID" },
                unique: true);

            // ---- Permissions (API-level only; menu visibility lives in Menus/RoleMenus) ----
            migrationBuilder.Sql(@"
INSERT INTO [Permissions] ([PermissionKey], [DisplayName], [Category], [DisplayOrder], [IsActive]) VALUES
    (N'Dashboard.Admin', N'Xem tổng quan Admin', N'Tổng quan', 1, 1),
    (N'Dashboard.Hr', N'Xem tổng quan Nhân sự', N'Tổng quan', 1, 1),
    (N'Dashboard.Manager', N'Xem tổng quan Quản lý', N'Tổng quan', 1, 1),
    (N'User.View', N'Xem danh sách người dùng', N'Người dùng & Vai trò', 2, 1),
    (N'User.Manage', N'Thêm / sửa / khóa người dùng', N'Người dùng & Vai trò', 2, 1),
    (N'Role.Manage', N'Quản lý vai trò', N'Người dùng & Vai trò', 2, 1),
    (N'Menu.Manage', N'Quản lý menu & phân menu theo vai trò', N'Người dùng & Vai trò', 2, 1),
    (N'SystemSettings.ManageGeneral', N'Cấu hình vị trí chấm công & người duyệt dự phòng', N'Cấu hình hệ thống', 3, 1),
    (N'SystemSettings.ManagePayroll', N'Cấu hình kỳ lương', N'Cấu hình hệ thống', 3, 1),
    (N'SystemSettings.ManageAdvanced', N'Cấu hình tính lương & thông tin công ty', N'Cấu hình hệ thống', 3, 1),
    (N'Employee.Manage', N'Thêm / sửa nhân viên, phân tích tuyến duyệt', N'Nhân viên', 4, 1),
    (N'Attendance.Manage', N'Khóa / mở khóa bản ghi chấm công', N'Chấm công', 5, 1),
    (N'HRProcedure.Submit', N'Tạo thủ tục nhân sự', N'Thủ tục nhân sự', 6, 1),
    (N'HRProcedure.Manage', N'Sửa / xóa thủ tục nhân sự', N'Thủ tục nhân sự', 6, 1),
    (N'HRProcedure.Approve', N'Duyệt / từ chối / áp dụng thủ tục', N'Thủ tục nhân sự', 6, 1),
    (N'Payroll.View', N'Xem kỳ lương & bảng lương', N'Lương', 7, 1),
    (N'Payroll.Process', N'Tạo kỳ lương, tính lương, phụ cấp, khấu trừ, phiếu lương', N'Lương', 7, 1),
    (N'Payroll.Approve', N'Duyệt / từ chối kỳ lương, khóa công', N'Lương', 7, 1),
    (N'Reports.Export', N'Xuất báo cáo', N'Báo cáo', 8, 1);

INSERT INTO [RolePermissions] ([RoleID], [PermissionID])
SELECT r.RoleId, p.PermissionId
FROM (VALUES
    (N'ADMIN', N'Dashboard.Admin'), (N'ADMIN', N'Dashboard.Hr'), (N'ADMIN', N'User.View'), (N'ADMIN', N'User.Manage'),
    (N'ADMIN', N'Role.Manage'), (N'ADMIN', N'Menu.Manage'), (N'ADMIN', N'SystemSettings.ManageGeneral'),
    (N'ADMIN', N'SystemSettings.ManageAdvanced'), (N'ADMIN', N'Employee.Manage'), (N'ADMIN', N'Attendance.Manage'),
    (N'ADMIN', N'HRProcedure.Manage'), (N'ADMIN', N'HRProcedure.Approve'), (N'ADMIN', N'Payroll.View'),
    (N'ADMIN', N'Payroll.Approve'), (N'ADMIN', N'Reports.Export'),
    (N'HR', N'Dashboard.Hr'), (N'HR', N'User.View'), (N'HR', N'SystemSettings.ManagePayroll'), (N'HR', N'Employee.Manage'),
    (N'HR', N'Attendance.Manage'), (N'HR', N'HRProcedure.Submit'), (N'HR', N'HRProcedure.Manage'), (N'HR', N'Payroll.View'),
    (N'HR', N'Payroll.Process'), (N'HR', N'Reports.Export'),
    (N'MANAGE', N'Dashboard.Manager'), (N'MANAGE', N'User.View'), (N'MANAGE', N'SystemSettings.ManageGeneral'),
    (N'MANAGE', N'Employee.Manage'), (N'MANAGE', N'Attendance.Manage'), (N'MANAGE', N'HRProcedure.Manage'),
    (N'MANAGE', N'HRProcedure.Approve'), (N'MANAGE', N'Payroll.View')
) AS g(RoleName, PermissionKey)
JOIN [Roles] r ON r.RoleName = g.RoleName
JOIN [Permissions] p ON p.PermissionKey = g.PermissionKey;
");

            // ---- Menu tree + per-role visibility (reproduces the previous hardcoded sidebar per role) ----
            migrationBuilder.Sql(@"
DECLARE @m TABLE (Code NVARCHAR(100), ParentCode NVARCHAR(100) NULL, Title NVARCHAR(200), Route NVARCHAR(300) NULL,
                  IconName NVARCHAR(100) NULL, DisplayOrder INT, Roles NVARCHAR(200));

INSERT INTO @m VALUES
    (N'dashboard.admin',  NULL, N'Tổng quan', N'/dashboard/admin',  N'AppstoreOutlined', 1, N'ADMIN'),
    (N'dashboard.hr',     NULL, N'Tổng quan', N'/dashboard/hr',     N'AppstoreOutlined', 2, N'HR'),
    (N'dashboard.manage', NULL, N'Tổng quan', N'/dashboard/manage', N'AppstoreOutlined', 3, N'MANAGE'),
    (N'self.attendance',  NULL, N'Chấm công của tôi', N'/attendance/my',  N'ClockCircleOutlined', 4, N'EMPLOYEE'),
    (N'self.leave',       NULL, N'Nghỉ phép của tôi', N'/leave/my',       N'CalendarOutlined',    5, N'EMPLOYEE'),
    (N'self.overtime',    NULL, N'Tăng ca của tôi',   N'/overtime/my',    N'ClockCircleOutlined', 6, N'EMPLOYEE'),
    (N'self.resignation', NULL, N'Đơn thôi việc',     N'/resignation/my', N'IdcardOutlined',      7, N'EMPLOYEE'),

    (N'system', NULL, N'Hệ thống', NULL, N'SettingOutlined', 10, N'ADMIN'),
    (N'system.users',       N'system', N'Quản lý người dùng', N'/admin/manage-user',       NULL, 1, N'ADMIN'),
    (N'system.roles',       N'system', N'Quản lý vai trò',    N'/admin/manage-role',       NULL, 2, N'ADMIN'),
    (N'system.permissions', N'system', N'Phân quyền',         N'/admin/manage-permission', NULL, 3, N'ADMIN'),
    (N'system.menus',       N'system', N'Quản lý menu',       N'/admin/manage-menu',       NULL, 4, N'ADMIN'),
    (N'system.settings',    N'system', N'Cấu hình hệ thống',  N'/admin/system-settings',   NULL, 5, N'ADMIN'),

    (N'org', NULL, N'Tổ chức', NULL, N'BankOutlined', 20, N'ADMIN,HR'),
    (N'org.departments', N'org', N'Quản lý phòng ban',  N'/hr/manage-department', NULL, 1, N'ADMIN,HR'),
    (N'org.positions',   N'org', N'Quản lý chức vụ',    N'/hr/manage-position',   NULL, 2, N'ADMIN,HR'),
    (N'org.employees',   N'org', N'Quản lý nhân viên',  N'/hr/manage-employee',   NULL, 3, N'ADMIN,HR'),

    (N'time', NULL, N'Chấm công & Ca làm', NULL, N'ClockCircleOutlined', 30, N'ADMIN,HR,MANAGE'),
    (N'time.my',          N'time', N'Chấm công của tôi',  N'/attendance/my',               NULL, 1, N'HR,MANAGE'),
    (N'time.manage',      N'time', N'Quản lý chấm công',  N'/attendance/manage',           NULL, 2, N'HR,MANAGE'),
    (N'time.face',        N'time', N'Đăng ký khuôn mặt',  N'/hr/manage-face-registration', NULL, 3, N'HR'),
    (N'time.shifts',      N'time', N'Quản lý ca',         N'/hr/manage-shift',             NULL, 4, N'ADMIN,HR'),
    (N'time.assignments', N'time', N'Phân ca làm việc',   N'/hr/manage-shift-assignment',  NULL, 5, N'ADMIN,HR'),

    (N'leave', NULL, N'Nghỉ phép & Tăng ca', NULL, N'CalendarOutlined', 40, N'ADMIN,HR,MANAGE'),
    (N'leave.my',              N'leave', N'Nghỉ phép của tôi',       N'/leave/my',            NULL, 1, N'HR,MANAGE'),
    (N'leave.manage',          N'leave', N'Duyệt nghỉ phép',         N'/leave/manage',        NULL, 2, N'ADMIN,HR,MANAGE'),
    (N'leave.config',          N'leave', N'Cấu hình nghỉ phép',      N'/leave/configuration', NULL, 3, N'ADMIN,HR'),
    (N'overtime.my',           N'leave', N'Tăng ca của tôi',         N'/overtime/my',         NULL, 4, N'HR,MANAGE'),
    (N'overtime.manage',       N'leave', N'Duyệt tăng ca',           N'/overtime/manage',     NULL, 5, N'ADMIN,MANAGE'),
    (N'resignation.my',        N'leave', N'Đơn thôi việc của tôi',   N'/resignation/my',      NULL, 6, N'MANAGE'),
    (N'resignation.manage',    N'leave', N'Duyệt đơn thôi việc',     N'/resignation/manage',  NULL, 7, N'ADMIN,MANAGE'),

    (N'hrservices', NULL, N'Dịch vụ Nhân sự', NULL, N'IdcardOutlined', 50, N'HR,MANAGE'),
    (N'hrservices.procedures',  N'hrservices', N'Quản lý thủ tục',        N'/hr/manage-procedure', NULL, 1, N'HR,MANAGE'),
    (N'hrservices.resignation', N'hrservices', N'Đơn thôi việc của tôi',  N'/resignation/my',      NULL, 2, N'HR'),

    (N'payroll', NULL, N'Lương & Thưởng', NULL, N'WalletOutlined', 60, N'ADMIN,HR,MANAGE,EMPLOYEE'),
    (N'payroll.periods',     N'payroll', N'Quản lý kỳ lương',    N'/payroll/periods',     NULL, 1, N'ADMIN,HR,MANAGE'),
    (N'payroll.mydraft',     N'payroll', N'Xem chấm công',       N'/payroll/my-draft',    NULL, 2, N'HR,MANAGE,EMPLOYEE'),
    (N'payroll.myslips',     N'payroll', N'Phiếu lương của tôi', N'/payroll/my-payslips', NULL, 3, N'HR,MANAGE,EMPLOYEE'),
    (N'payroll.report',      N'payroll', N'Báo cáo quỹ lương',   N'/payroll/report',      NULL, 4, N'HR'),
    (N'payroll.methodology', N'payroll', N'Quy tắc tính lương',  N'/payroll/methodology', NULL, 5, N'ADMIN,HR'),

    (N'tasks', NULL, N'Quản lý công việc', N'/manage-task', N'TeamOutlined', 80, N'ADMIN,HR,MANAGE,EMPLOYEE'),

    (N'config', NULL, N'Cấu hình', NULL, N'SettingOutlined', 90, N'HR'),
    (N'config.payroll', N'config', N'Cấu hình kỳ lương', N'/hr/payroll-settings', NULL, 1, N'HR');

INSERT INTO [Menus] ([ParentID], [Code], [Title], [Route], [IconName], [DisplayOrder], [IsActive])
SELECT NULL, Code, Title, Route, IconName, DisplayOrder, 1 FROM @m WHERE ParentCode IS NULL;

INSERT INTO [Menus] ([ParentID], [Code], [Title], [Route], [IconName], [DisplayOrder], [IsActive])
SELECT p.MenuID, m.Code, m.Title, m.Route, m.IconName, m.DisplayOrder, 1
FROM @m m JOIN [Menus] p ON p.Code = m.ParentCode
WHERE m.ParentCode IS NOT NULL;

INSERT INTO [RoleMenus] ([RoleID], [MenuID])
SELECT r.RoleId, mn.MenuID
FROM @m m
JOIN [Menus] mn ON mn.Code = m.Code
JOIN [Roles] r ON (N',' + m.Roles + N',') LIKE (N'%,' + r.RoleName + N',%');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleMenus");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "Permissions");
        }
    }
}
