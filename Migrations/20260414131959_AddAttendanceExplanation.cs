using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceExplanation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExplanationMessage",
                table: "AttendanceRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExplanationResponse",
                table: "AttendanceRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExplanationStatus",
                table: "AttendanceRecords",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExplanationMessage",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ExplanationResponse",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ExplanationStatus",
                table: "AttendanceRecords");
        }
    }
}
