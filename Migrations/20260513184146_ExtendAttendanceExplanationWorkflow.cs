using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class ExtendAttendanceExplanationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExplanationLeaveTypeID",
                table: "AttendanceRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ExplanationRequestedCheckInTime",
                table: "AttendanceRecords",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ExplanationRequestedCheckOutTime",
                table: "AttendanceRecords",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExplanationType",
                table: "AttendanceRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_ExplanationLeaveTypeID",
                table: "AttendanceRecords",
                column: "ExplanationLeaveTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_ExplanationLeaveTypes",
                table: "AttendanceRecords",
                column: "ExplanationLeaveTypeID",
                principalTable: "LeaveTypes",
                principalColumn: "LeaveTypeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_ExplanationLeaveTypes",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_ExplanationLeaveTypeID",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ExplanationLeaveTypeID",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ExplanationRequestedCheckInTime",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ExplanationRequestedCheckOutTime",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ExplanationType",
                table: "AttendanceRecords");
        }
    }
}
