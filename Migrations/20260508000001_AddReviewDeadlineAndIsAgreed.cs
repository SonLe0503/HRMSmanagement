using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    public partial class AddReviewDeadlineAndIsAgreed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ReviewDeadline to PayrollPeriods
            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewDeadline",
                table: "PayrollPeriods",
                type: "datetime2",
                nullable: true);

            // Add IsAgreed to PayrollFeedbacks (default false)
            migrationBuilder.AddColumn<bool>(
                name: "IsAgreed",
                table: "PayrollFeedbacks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Make Content nullable
            migrationBuilder.Sql("ALTER TABLE [dbo].[PayrollFeedbacks] ALTER COLUMN [Content] NVARCHAR(500) NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ReviewDeadline", table: "PayrollPeriods");
            migrationBuilder.DropColumn(name: "IsAgreed",       table: "PayrollFeedbacks");
            migrationBuilder.Sql("ALTER TABLE [dbo].[PayrollFeedbacks] ALTER COLUMN [Content] NVARCHAR(500) NOT NULL");
        }
    }
}
