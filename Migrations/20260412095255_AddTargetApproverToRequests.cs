using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetApproverToRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetApproverId",
                table: "OvertimeRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetApproverId",
                table: "LeaveRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequests_TargetApproverId",
                table: "OvertimeRequests",
                column: "TargetApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_TargetApproverId",
                table: "LeaveRequests",
                column: "TargetApproverId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_TargetApprover",
                table: "LeaveRequests",
                column: "TargetApproverId",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_OvertimeRequests_TargetApprover",
                table: "OvertimeRequests",
                column: "TargetApproverId",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_TargetApprover",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_OvertimeRequests_TargetApprover",
                table: "OvertimeRequests");

            migrationBuilder.DropIndex(
                name: "IX_OvertimeRequests_TargetApproverId",
                table: "OvertimeRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_TargetApproverId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "TargetApproverId",
                table: "OvertimeRequests");

            migrationBuilder.DropColumn(
                name: "TargetApproverId",
                table: "LeaveRequests");
        }
    }
}
