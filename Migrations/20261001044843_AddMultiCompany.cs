using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Companies must exist (with the default company) before CompanyID columns and FKs are added
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    CompanyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyID);
                });

            migrationBuilder.Sql(@"
SET IDENTITY_INSERT [Companies] ON;
INSERT INTO [Companies] ([CompanyID], [CompanyCode], [CompanyName], [IsActive])
VALUES (1, N'DEFAULT', N'Công ty mặc định', 1);
SET IDENTITY_INSERT [Companies] OFF;");

            migrationBuilder.DropIndex(
                name: "UQ__SystemSe__01E719AD19FD850C",
                table: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "UQ__Shifts__9377D5623409151A",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "UQ__Roles__8A2B6160EED0EB46",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_ResignationRequests_RequestNumber",
                table: "ResignationRequests");

            migrationBuilder.DropIndex(
                name: "UQ__Position__83745B02ABEA23B6",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "UQ__Payslips__38A71BAD66586385",
                table: "Payslips");

            migrationBuilder.DropIndex(
                name: "UQ__PayrollP__251851158851FA7E",
                table: "PayrollPolicies");

            migrationBuilder.DropIndex(
                name: "UQ_PayrollPeriods",
                table: "PayrollPeriods");

            migrationBuilder.DropIndex(
                name: "UQ__Overtime__9ADA6BE0AE2DCA21",
                table: "OvertimeRequests");

            migrationBuilder.DropIndex(
                name: "UQ__LeaveTyp__A264FAEECF215F7C",
                table: "LeaveTypes");

            migrationBuilder.DropIndex(
                name: "UQ__LeaveReq__9ADA6BE0F25CECE0",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "UQ__HRProced__AA41A753D36A3E49",
                table: "HRProcedures");

            migrationBuilder.DropIndex(
                name: "UQ__Employee__1F642548E0EDEFF5",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "UQ__Employee__A9D105349FE0388B",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "UQ__Employee__C51D43DADFFA6EF0",
                table: "EmployeeContracts");

            migrationBuilder.DropIndex(
                name: "UQ__Departme__6EA8896D0227EAE3",
                table: "Departments");

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "SystemSettings",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "ShiftAssignments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "ResignationRequests",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "Positions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "Payslips",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "PayrollRecords",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "PayrollPolicies",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "PayrollPeriods",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "PayrollFeedbacks",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "PayrollDeductions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "PayrollAllowances",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "OvertimeRequests",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "LeaveTypes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "LeaveBalances",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "HRProcedures",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "FaceVerificationLogs",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "FaceProfiles",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "EmployeeDocuments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "EmployeeContracts",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "AttendanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "AttendanceLogs",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Existing users all belong to the default company
            migrationBuilder.Sql("UPDATE [Users] SET [CompanyID] = 1 WHERE [CompanyID] IS NULL;");

            // The temporary DEFAULT 1 only served to backfill existing rows; drop it so a row that
            // reaches the database without a company fails instead of silently landing in company 1.
            migrationBuilder.Sql(@"
DECLARE @sql nvarchar(max) = N'';
SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(dc.parent_object_id)) + N'.'
             + QUOTENAME(OBJECT_NAME(dc.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(dc.name) + N';'
FROM sys.default_constraints dc
JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE c.name = N'CompanyID';
EXEC sp_executesql @sql;");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyID",
                table: "Users",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CompanyID",
                table: "Tasks",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "UQ__SystemSe__01E719AD19FD850C",
                table: "SystemSettings",
                columns: new[] { "CompanyID", "SettingKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Shifts__9377D5623409151A",
                table: "Shifts",
                columns: new[] { "CompanyID", "ShiftCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_CompanyID",
                table: "ShiftAssignments",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "UQ__Roles__8A2B6160EED0EB46",
                table: "Roles",
                columns: new[] { "CompanyID", "RoleName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResignationRequests_CompanyID_RequestNumber",
                table: "ResignationRequests",
                columns: new[] { "CompanyID", "RequestNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Position__83745B02ABEA23B6",
                table: "Positions",
                columns: new[] { "CompanyID", "PositionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Payslips__38A71BAD66586385",
                table: "Payslips",
                columns: new[] { "CompanyID", "PayslipNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_CompanyID",
                table: "PayrollRecords",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "UQ__PayrollP__251851158851FA7E",
                table: "PayrollPolicies",
                columns: new[] { "CompanyID", "PolicyName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_PayrollPeriods",
                table: "PayrollPeriods",
                columns: new[] { "CompanyID", "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollFeedbacks_CompanyID",
                table: "PayrollFeedbacks",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollDeductions_CompanyID",
                table: "PayrollDeductions",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAllowances_CompanyID",
                table: "PayrollAllowances",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "UQ__Overtime__9ADA6BE0AE2DCA21",
                table: "OvertimeRequests",
                columns: new[] { "CompanyID", "RequestNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CompanyID",
                table: "Notifications",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "UQ__LeaveTyp__A264FAEECF215F7C",
                table: "LeaveTypes",
                columns: new[] { "CompanyID", "LeaveTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__LeaveReq__9ADA6BE0F25CECE0",
                table: "LeaveRequests",
                columns: new[] { "CompanyID", "RequestNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_CompanyID",
                table: "LeaveBalances",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "UQ__HRProced__AA41A753D36A3E49",
                table: "HRProcedures",
                columns: new[] { "CompanyID", "ProcedureNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaceVerificationLogs_CompanyID",
                table: "FaceVerificationLogs",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_FaceProfiles_CompanyID",
                table: "FaceProfiles",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "UQ__Employee__1F642548E0EDEFF5",
                table: "Employees",
                columns: new[] { "CompanyID", "EmployeeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Employee__A9D105349FE0388B",
                table: "Employees",
                columns: new[] { "CompanyID", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_CompanyID",
                table: "EmployeeDocuments",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "UQ__Employee__C51D43DADFFA6EF0",
                table: "EmployeeContracts",
                columns: new[] { "CompanyID", "ContractNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Departme__6EA8896D0227EAE3",
                table: "Departments",
                columns: new[] { "CompanyID", "DepartmentCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CompanyID",
                table: "AuditLogs",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_CompanyID",
                table: "AttendanceRecords",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLogs_CompanyID",
                table: "AttendanceLogs",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CompanyCode",
                table: "Companies",
                column: "CompanyCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceLogs_Companies",
                table: "AttendanceLogs",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Companies",
                table: "AttendanceRecords",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Companies",
                table: "AuditLogs",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Companies",
                table: "Departments",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeContracts_Companies",
                table: "EmployeeContracts",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDocuments_Companies",
                table: "EmployeeDocuments",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Companies",
                table: "Employees",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FaceProfiles_Companies",
                table: "FaceProfiles",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FaceVerificationLogs_Companies",
                table: "FaceVerificationLogs",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HRProcedures_Companies",
                table: "HRProcedures",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveBalances_Companies",
                table: "LeaveBalances",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_Companies",
                table: "LeaveRequests",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveTypes_Companies",
                table: "LeaveTypes",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Companies",
                table: "Notifications",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OvertimeRequests_Companies",
                table: "OvertimeRequests",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollAllowances_Companies",
                table: "PayrollAllowances",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollDeductions_Companies",
                table: "PayrollDeductions",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollFeedbacks_Companies",
                table: "PayrollFeedbacks",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollPeriods_Companies",
                table: "PayrollPeriods",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollPolicies_Companies",
                table: "PayrollPolicies",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollRecords_Companies",
                table: "PayrollRecords",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payslips_Companies",
                table: "Payslips",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Companies",
                table: "Positions",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ResignationRequests_Companies",
                table: "ResignationRequests",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Companies",
                table: "Roles",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftAssignments_Companies",
                table: "ShiftAssignments",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Companies",
                table: "Shifts",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemSettings_Companies",
                table: "SystemSettings",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Companies",
                table: "Tasks",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Companies",
                table: "Users",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceLogs_Companies",
                table: "AttendanceLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Companies",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Companies",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Companies",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeContracts_Companies",
                table: "EmployeeContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDocuments_Companies",
                table: "EmployeeDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Companies",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_FaceProfiles_Companies",
                table: "FaceProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_FaceVerificationLogs_Companies",
                table: "FaceVerificationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_HRProcedures_Companies",
                table: "HRProcedures");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveBalances_Companies",
                table: "LeaveBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Companies",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveTypes_Companies",
                table: "LeaveTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Companies",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_OvertimeRequests_Companies",
                table: "OvertimeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollAllowances_Companies",
                table: "PayrollAllowances");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollDeductions_Companies",
                table: "PayrollDeductions");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollFeedbacks_Companies",
                table: "PayrollFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollPeriods_Companies",
                table: "PayrollPeriods");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollPolicies_Companies",
                table: "PayrollPolicies");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollRecords_Companies",
                table: "PayrollRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Payslips_Companies",
                table: "Payslips");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Companies",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_ResignationRequests_Companies",
                table: "ResignationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Companies",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_ShiftAssignments_Companies",
                table: "ShiftAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Companies",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemSettings_Companies",
                table: "SystemSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Companies",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Companies",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Users_CompanyID",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CompanyID",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "UQ__SystemSe__01E719AD19FD850C",
                table: "SystemSettings");

            migrationBuilder.DropIndex(
                name: "UQ__Shifts__9377D5623409151A",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_ShiftAssignments_CompanyID",
                table: "ShiftAssignments");

            migrationBuilder.DropIndex(
                name: "UQ__Roles__8A2B6160EED0EB46",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_ResignationRequests_CompanyID_RequestNumber",
                table: "ResignationRequests");

            migrationBuilder.DropIndex(
                name: "UQ__Position__83745B02ABEA23B6",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "UQ__Payslips__38A71BAD66586385",
                table: "Payslips");

            migrationBuilder.DropIndex(
                name: "IX_PayrollRecords_CompanyID",
                table: "PayrollRecords");

            migrationBuilder.DropIndex(
                name: "UQ__PayrollP__251851158851FA7E",
                table: "PayrollPolicies");

            migrationBuilder.DropIndex(
                name: "UQ_PayrollPeriods",
                table: "PayrollPeriods");

            migrationBuilder.DropIndex(
                name: "IX_PayrollFeedbacks_CompanyID",
                table: "PayrollFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_PayrollDeductions_CompanyID",
                table: "PayrollDeductions");

            migrationBuilder.DropIndex(
                name: "IX_PayrollAllowances_CompanyID",
                table: "PayrollAllowances");

            migrationBuilder.DropIndex(
                name: "UQ__Overtime__9ADA6BE0AE2DCA21",
                table: "OvertimeRequests");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CompanyID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "UQ__LeaveTyp__A264FAEECF215F7C",
                table: "LeaveTypes");

            migrationBuilder.DropIndex(
                name: "UQ__LeaveReq__9ADA6BE0F25CECE0",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveBalances_CompanyID",
                table: "LeaveBalances");

            migrationBuilder.DropIndex(
                name: "UQ__HRProced__AA41A753D36A3E49",
                table: "HRProcedures");

            migrationBuilder.DropIndex(
                name: "IX_FaceVerificationLogs_CompanyID",
                table: "FaceVerificationLogs");

            migrationBuilder.DropIndex(
                name: "IX_FaceProfiles_CompanyID",
                table: "FaceProfiles");

            migrationBuilder.DropIndex(
                name: "UQ__Employee__1F642548E0EDEFF5",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "UQ__Employee__A9D105349FE0388B",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDocuments_CompanyID",
                table: "EmployeeDocuments");

            migrationBuilder.DropIndex(
                name: "UQ__Employee__C51D43DADFFA6EF0",
                table: "EmployeeContracts");

            migrationBuilder.DropIndex(
                name: "UQ__Departme__6EA8896D0227EAE3",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CompanyID",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_CompanyID",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceLogs_CompanyID",
                table: "AttendanceLogs");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "ShiftAssignments");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "ResignationRequests");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "PayrollPolicies");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "PayrollFeedbacks");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "PayrollDeductions");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "PayrollAllowances");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "OvertimeRequests");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "LeaveBalances");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "HRProcedures");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "FaceVerificationLogs");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "FaceProfiles");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "EmployeeContracts");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "AttendanceLogs");

            migrationBuilder.CreateIndex(
                name: "UQ__SystemSe__01E719AD19FD850C",
                table: "SystemSettings",
                column: "SettingKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Shifts__9377D5623409151A",
                table: "Shifts",
                column: "ShiftCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Roles__8A2B6160EED0EB46",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResignationRequests_RequestNumber",
                table: "ResignationRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Position__83745B02ABEA23B6",
                table: "Positions",
                column: "PositionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Payslips__38A71BAD66586385",
                table: "Payslips",
                column: "PayslipNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__PayrollP__251851158851FA7E",
                table: "PayrollPolicies",
                column: "PolicyName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_PayrollPeriods",
                table: "PayrollPeriods",
                columns: new[] { "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Overtime__9ADA6BE0AE2DCA21",
                table: "OvertimeRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__LeaveTyp__A264FAEECF215F7C",
                table: "LeaveTypes",
                column: "LeaveTypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__LeaveReq__9ADA6BE0F25CECE0",
                table: "LeaveRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__HRProced__AA41A753D36A3E49",
                table: "HRProcedures",
                column: "ProcedureNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Employee__1F642548E0EDEFF5",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Employee__A9D105349FE0388B",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Employee__C51D43DADFFA6EF0",
                table: "EmployeeContracts",
                column: "ContractNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Departme__6EA8896D0227EAE3",
                table: "Departments",
                column: "DepartmentCode",
                unique: true);
        }
    }
}
