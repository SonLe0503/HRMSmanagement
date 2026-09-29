using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: AggregatedDate -> AttendanceCutoffDate/ReviewWindowDays was already applied to all
            // real environments via a direct SQL change (see commit 79f1256); the model/snapshot already
            // reflects that end state, so no column DDL is scaffolded here — only the seed data below.

            migrationBuilder.Sql(@"
DECLARE @now DATETIME = GETDATE();

INSERT INTO [Roles] ([RoleName], [Description], [IsActive], [CreatedDate]) VALUES
    (N'ADMIN', N'Quan tri he thong', 1, @now),
    (N'HR', N'Nhan su', 1, @now),
    (N'MANAGE', N'Quan ly', 1, @now),
    (N'EMPLOYEE', N'Nhan vien', 1, @now);

INSERT INTO [Employees] ([EmployeeCode], [FirstName], [LastName], [Email], [JoinDate], [EmploymentStatus], [EmploymentType], [NumberOfDependents], [CreatedDate]) VALUES
    (N'EMP0001', N'Admin', N'System', N'admin@hrms.local', @now, N'Active', N'Full-Time', 0, @now),
    (N'EMP0002', N'Hoa', N'Nguyen Thi', N'hr@hrms.local', @now, N'Active', N'Full-Time', 0, @now),
    (N'EMP0003', N'Nam', N'Tran Van', N'manager@hrms.local', @now, N'Active', N'Full-Time', 0, @now),
    (N'EMP0004', N'Lan', N'Le Thi', N'employee@hrms.local', @now, N'Active', N'Full-Time', 0, @now);

INSERT INTO [Users] ([Username], [Email], [PasswordHash], [EmployeeId], [IsActive], [CreatedDate]) VALUES
    (N'admin', N'admin@hrms.local', N'$2a$11$CvQG3Xihfph2FHirEX7QIeEAMV0MwZyR2h8vS3/CXVM.NERxYBbfC', (SELECT EmployeeId FROM Employees WHERE EmployeeCode = N'EMP0001'), 1, @now),
    (N'hr', N'hr@hrms.local', N'$2a$11$jbwqVmZSqXA6Pn7tvaSuLOo03.tUMveClfTvgN6jn2.TDi89vGoti', (SELECT EmployeeId FROM Employees WHERE EmployeeCode = N'EMP0002'), 1, @now),
    (N'manager', N'manager@hrms.local', N'$2a$11$YWlOr9WQWaDO28veyQSC9uJHDylTFgWHbOLlR8yXX5qJa1PLZol9S', (SELECT EmployeeId FROM Employees WHERE EmployeeCode = N'EMP0003'), 1, @now),
    (N'employee', N'employee@hrms.local', N'$2a$11$DxXSiqG.KPIDEYiSc2B.0e1PooHTnVF.QNmqhVP9kFaBzX/XUEY06', (SELECT EmployeeId FROM Employees WHERE EmployeeCode = N'EMP0004'), 1, @now);

INSERT INTO [UserRoles] ([UserId], [RoleId], [AssignedDate]) VALUES
    ((SELECT UserId FROM Users WHERE Username = N'admin'), (SELECT RoleId FROM Roles WHERE RoleName = N'ADMIN'), @now),
    ((SELECT UserId FROM Users WHERE Username = N'hr'), (SELECT RoleId FROM Roles WHERE RoleName = N'HR'), @now),
    ((SELECT UserId FROM Users WHERE Username = N'manager'), (SELECT RoleId FROM Roles WHERE RoleName = N'MANAGE'), @now),
    ((SELECT UserId FROM Users WHERE Username = N'employee'), (SELECT RoleId FROM Roles WHERE RoleName = N'EMPLOYEE'), @now);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [UserRoles] WHERE [UserId] IN (SELECT UserId FROM Users WHERE Username IN (N'admin', N'hr', N'manager', N'employee'));
DELETE FROM [Users] WHERE Username IN (N'admin', N'hr', N'manager', N'employee');
DELETE FROM [Employees] WHERE EmployeeCode IN (N'EMP0001', N'EMP0002', N'EMP0003', N'EMP0004');
DELETE FROM [Roles] WHERE RoleName IN (N'ADMIN', N'HR', N'MANAGE', N'EMPLOYEE');
");
        }
    }
}
