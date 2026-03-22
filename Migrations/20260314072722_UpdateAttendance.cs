using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EarlyCheckInMinutes",
                table: "Shifts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOvernight",
                table: "Shifts",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LateGraceMinutes",
                table: "Shifts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LatestCheckInMinutes",
                table: "Shifts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LatestCheckOutMinutes",
                table: "Shifts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "AttendanceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "AttendanceRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "AttendanceRecords",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManualAdjusted",
                table: "AttendanceRecords",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "AttendanceRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttendanceLogs",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    ShiftID = table.Column<int>(type: "int", nullable: true),
                    LogTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    LogType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Web"),
                    DeviceInfo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsValid = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Attendan__5E5499A8", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_AttendanceLogs_Employees",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_AttendanceLogs_Shifts",
                        column: x => x.ShiftID,
                        principalTable: "Shifts",
                        principalColumn: "ShiftID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLogs_EmployeeID",
                table: "AttendanceLogs",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLogs_ShiftID",
                table: "AttendanceLogs",
                column: "ShiftID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceLogs");

            migrationBuilder.DropColumn(
                name: "EarlyCheckInMinutes",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "IsOvernight",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "LateGraceMinutes",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "LatestCheckInMinutes",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "LatestCheckOutMinutes",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "IsManualAdjusted",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "AttendanceRecords");
        }
    }
}
