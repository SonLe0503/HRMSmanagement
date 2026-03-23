using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class update_faceprofile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckInVerificationMethod",
                table: "AttendanceRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CheckInVerified",
                table: "AttendanceRecords",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckOutVerificationMethod",
                table: "AttendanceRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CheckOutVerified",
                table: "AttendanceRecords",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaceVerificationId",
                table: "AttendanceLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationMethod",
                table: "AttendanceLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationStatus",
                table: "AttendanceLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FaceProfiles",
                columns: table => new
                {
                    FaceProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ReferenceImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FaceEmbedding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceProfiles", x => x.FaceProfileId);
                    table.ForeignKey(
                        name: "FK_FaceProfiles_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FaceVerificationLogs",
                columns: table => new
                {
                    VerificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    AttendanceLogId = table.Column<int>(type: "int", nullable: true),
                    VerificationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CapturedImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ThresholdUsed = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsMatch = table.Column<bool>(type: "bit", nullable: false),
                    LivenessPassed = table.Column<bool>(type: "bit", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceVerificationLogs", x => x.VerificationId);
                    table.ForeignKey(
                        name: "FK_FaceVerificationLogs_AttendanceLogs_AttendanceLogId",
                        column: x => x.AttendanceLogId,
                        principalTable: "AttendanceLogs",
                        principalColumn: "LogID");
                    table.ForeignKey(
                        name: "FK_FaceVerificationLogs_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaceProfiles_EmployeeId",
                table: "FaceProfiles",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FaceVerificationLogs_AttendanceLogId",
                table: "FaceVerificationLogs",
                column: "AttendanceLogId");

            migrationBuilder.CreateIndex(
                name: "IX_FaceVerificationLogs_EmployeeId",
                table: "FaceVerificationLogs",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaceProfiles");

            migrationBuilder.DropTable(
                name: "FaceVerificationLogs");

            migrationBuilder.DropColumn(
                name: "CheckInVerificationMethod",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "CheckInVerified",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "CheckOutVerificationMethod",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "CheckOutVerified",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "FaceVerificationId",
                table: "AttendanceLogs");

            migrationBuilder.DropColumn(
                name: "VerificationMethod",
                table: "AttendanceLogs");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "AttendanceLogs");
        }
    }
}
