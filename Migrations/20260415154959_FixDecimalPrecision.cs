using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // WorkingHours: decimal(4,2) → decimal(6,2)  [max 99.99 → 9999.99]
            migrationBuilder.AlterColumn<decimal>(
                name: "WorkingHours",
                table: "AttendanceRecords",
                type: "decimal(6,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(4, 2)",
                oldNullable: true);

            // OvertimeHours: decimal(4,2) → decimal(6,2)
            migrationBuilder.AlterColumn<decimal>(
                name: "OvertimeHours",
                table: "AttendanceRecords",
                type: "decimal(6,2)",
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(4, 2)",
                oldNullable: true,
                oldDefaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "WorkingHours",
                table: "AttendanceRecords",
                type: "decimal(4, 2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OvertimeHours",
                table: "AttendanceRecords",
                type: "decimal(4, 2)",
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldNullable: true,
                oldDefaultValue: 0m);
        }
    }
}
