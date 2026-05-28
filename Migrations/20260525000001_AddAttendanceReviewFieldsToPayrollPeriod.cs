using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceReviewFieldsToPayrollPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "AttendanceCutoffDate",
                table: "PayrollPeriods",
                type: "date",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<int>(
                name: "ReviewWindowDays",
                table: "PayrollPeriods",
                type: "int",
                nullable: false,
                defaultValue: 5);

            // Xóa cột AggregatedDate không còn dùng
            migrationBuilder.DropColumn(
                name: "AggregatedDate",
                table: "PayrollPeriods");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceCutoffDate",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "ReviewWindowDays",
                table: "PayrollPeriods");

            migrationBuilder.AddColumn<DateTime>(
                name: "AggregatedDate",
                table: "PayrollPeriods",
                type: "datetime2",
                nullable: true);
        }
    }
}
