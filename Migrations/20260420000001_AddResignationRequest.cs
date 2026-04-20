using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    [Microsoft.EntityFrameworkCore.Infrastructure.DbContext(typeof(HRManagement.Models.HrmsDbContext))]
    [Migration("20260420000001_AddResignationRequest")]
    public partial class AddResignationRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResignationRequests",
                columns: table => new
                {
                    ResignationRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ExpectedLastWorkingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HandoverNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HandoverToEmployeeId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewerComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    ReviewedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    TargetApproverId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResignationRequests", x => x.ResignationRequestId);
                    table.ForeignKey(
                        name: "FK_ResignationRequests_Employees",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_ResignationRequests_HandoverEmployee",
                        column: x => x.HandoverToEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_ResignationRequests_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_ResignationRequests_TargetApprover",
                        column: x => x.TargetApproverId,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_ResignationRequests_RequestNumber",
                table: "ResignationRequests",
                column: "RequestNumber",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ResignationRequests");
        }
    }
}
