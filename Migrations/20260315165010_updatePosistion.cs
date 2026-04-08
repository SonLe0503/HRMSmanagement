using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class updatePosistion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerID",
                table: "Departments",
                column: "ManagerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_ManagerID",
                table: "Departments",
                column: "ManagerID",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_ManagerID",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_ManagerID",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Positions");
        }
    }
}
