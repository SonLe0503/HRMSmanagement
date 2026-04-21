using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    [Microsoft.EntityFrameworkCore.Infrastructure.DbContext(typeof(HRManagement.Models.HrmsDbContext))]
    [Migration("20260421000001_FixAttendanceStatusConstraint")]
    public partial class FixAttendanceStatusConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE AttendanceRecords DROP CONSTRAINT CHK_Attendance_Status");

            migrationBuilder.Sql(
                "ALTER TABLE AttendanceRecords ADD CONSTRAINT CHK_Attendance_Status " +
                "CHECK (Status IN ('Present','Late','Absent','Incomplete','PaidLeave','UnpaidLeave'))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE AttendanceRecords DROP CONSTRAINT CHK_Attendance_Status");

            migrationBuilder.Sql(
                "ALTER TABLE AttendanceRecords ADD CONSTRAINT CHK_Attendance_Status " +
                "CHECK (Status IN ('Present','Late','Absent','PaidLeave','UnpaidLeave'))");
        }
    }
}
