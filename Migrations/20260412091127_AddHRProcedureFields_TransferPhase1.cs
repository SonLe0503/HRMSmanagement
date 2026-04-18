using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddHRProcedureFields_TransferPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppliedBy",
                table: "HRProcedures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppliedDate",
                table: "HRProcedures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NewManagerId",
                table: "HRProcedures",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HRProcedures_AppliedBy",
                table: "HRProcedures",
                column: "AppliedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HRProcedures_NewManagerId",
                table: "HRProcedures",
                column: "NewManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_HRProcedures_AppliedBy",
                table: "HRProcedures",
                column: "AppliedBy",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HRProcedures_NewManager",
                table: "HRProcedures",
                column: "NewManagerId",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HRProcedures_AppliedBy",
                table: "HRProcedures");

            migrationBuilder.DropForeignKey(
                name: "FK_HRProcedures_NewManager",
                table: "HRProcedures");

            migrationBuilder.DropIndex(
                name: "IX_HRProcedures_AppliedBy",
                table: "HRProcedures");

            migrationBuilder.DropIndex(
                name: "IX_HRProcedures_NewManagerId",
                table: "HRProcedures");

            migrationBuilder.DropColumn(
                name: "AppliedBy",
                table: "HRProcedures");

            migrationBuilder.DropColumn(
                name: "AppliedDate",
                table: "HRProcedures");

            migrationBuilder.DropColumn(
                name: "NewManagerId",
                table: "HRProcedures");
        }
    }
}
