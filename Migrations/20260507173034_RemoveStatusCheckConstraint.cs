using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStatusCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop any existing CHECK constraint on PayrollPeriods.Status
            // so that 'UnderReview' is a valid value.
            migrationBuilder.Sql(@"
                DECLARE @constraintName NVARCHAR(256)
                SELECT @constraintName = cc.name
                FROM sys.check_constraints cc
                INNER JOIN sys.columns c
                    ON cc.parent_object_id = c.object_id
                   AND cc.parent_column_id = c.column_id
                WHERE cc.parent_object_id = OBJECT_ID('dbo.PayrollPeriods')
                  AND c.name = 'Status'

                IF @constraintName IS NOT NULL
                    EXEC('ALTER TABLE dbo.PayrollPeriods DROP CONSTRAINT [' + @constraintName + ']')
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.check_constraints cc
                    INNER JOIN sys.columns c
                        ON cc.parent_object_id = c.object_id AND cc.parent_column_id = c.column_id
                    WHERE cc.parent_object_id = OBJECT_ID('dbo.PayrollPeriods') AND c.name = 'Status'
                )
                ALTER TABLE dbo.PayrollPeriods
                ADD CONSTRAINT CK_PayrollPeriods_Status
                CHECK (Status IN ('Open', 'Aggregated', 'Calculated', 'Approved', 'Closed'))
            ");
        }
    }
}
