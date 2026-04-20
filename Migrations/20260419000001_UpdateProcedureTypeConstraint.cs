using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    [Microsoft.EntityFrameworkCore.Infrastructure.DbContext(typeof(HRManagement.Models.HrmsDbContext))]
    [Migration("20260419000001_UpdateProcedureTypeConstraint")]
    public partial class UpdateProcedureTypeConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE [dbo].[HRProcedures] DROP CONSTRAINT [CHK_HRProcedures_Type]");

            migrationBuilder.Sql(
                "ALTER TABLE [dbo].[HRProcedures] ADD CONSTRAINT [CHK_HRProcedures_Type] " +
                "CHECK ([ProcedureType] IN ('Appointment', 'Transfer', 'Demotion', 'Termination'))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE [dbo].[HRProcedures] DROP CONSTRAINT [CHK_HRProcedures_Type]");

            migrationBuilder.Sql(
                "ALTER TABLE [dbo].[HRProcedures] ADD CONSTRAINT [CHK_HRProcedures_Type] " +
                "CHECK ([ProcedureType] IN ('Appointment', 'Transfer', 'Promotion', 'Resignation', 'Termination'))");
        }
    }
}
