using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRManagement.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEvaluationFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvaluationRatings");

            migrationBuilder.DropTable(
                name: "EvaluationCriteria");

            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "EvaluationCycles");

            migrationBuilder.DropTable(
                name: "EvaluationTemplates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvaluationCycles",
                columns: table => new
                {
                    CycleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicableDepartments = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    CycleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CycleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EvaluationPeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    EvaluationPeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    ManagerEvaluationEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    ManagerEvaluationStart = table.Column<DateOnly>(type: "date", nullable: false),
                    ReviewMeetingEnd = table.Column<DateOnly>(type: "date", nullable: true),
                    ReviewMeetingStart = table.Column<DateOnly>(type: "date", nullable: true),
                    SelfEvaluationEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    SelfEvaluationStart = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Evaluati__077B24D9A63CD15C", x => x.CycleID);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationTemplates",
                columns: table => new
                {
                    TemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Evaluati__F87ADD07F22F93BD", x => x.TemplateID);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationCriteria",
                columns: table => new
                {
                    CriteriaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateID = table.Column<int>(type: "int", nullable: false),
                    CriteriaCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CriteriaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Weightage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Evaluati__FE6ADB2D58C14D2A", x => x.CriteriaID);
                    table.ForeignKey(
                        name: "FK_EvaluationCriteria_Templates",
                        column: x => x.TemplateID,
                        principalTable: "EvaluationTemplates",
                        principalColumn: "TemplateID");
                });

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    EvaluationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CycleID = table.Column<int>(type: "int", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    PrimaryEvaluatorID = table.Column<int>(type: "int", nullable: true),
                    SecondaryEvaluatorID = table.Column<int>(type: "int", nullable: true),
                    TemplateID = table.Column<int>(type: "int", nullable: false),
                    AcknowledgedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    AcknowledgementComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverallRating = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Not Started"),
                    SubmittedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Evaluati__36AE68D3349E1779", x => x.EvaluationID);
                    table.ForeignKey(
                        name: "FK_Evaluations_Cycles",
                        column: x => x.CycleID,
                        principalTable: "EvaluationCycles",
                        principalColumn: "CycleID");
                    table.ForeignKey(
                        name: "FK_Evaluations_Employees",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_Evaluations_PrimaryEvaluator",
                        column: x => x.PrimaryEvaluatorID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_Evaluations_SecondaryEvaluator",
                        column: x => x.SecondaryEvaluatorID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_Evaluations_Templates",
                        column: x => x.TemplateID,
                        principalTable: "EvaluationTemplates",
                        principalColumn: "TemplateID");
                });

            migrationBuilder.CreateTable(
                name: "EvaluationRatings",
                columns: table => new
                {
                    RatingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CriteriaID = table.Column<int>(type: "int", nullable: false),
                    EvaluationID = table.Column<int>(type: "int", nullable: false),
                    Evidence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagerComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagerRating = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    SelfComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelfRating = table.Column<decimal>(type: "decimal(3,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Evaluati__FCCDF85C69565A87", x => x.RatingID);
                    table.ForeignKey(
                        name: "FK_EvaluationRatings_Criteria",
                        column: x => x.CriteriaID,
                        principalTable: "EvaluationCriteria",
                        principalColumn: "CriteriaID");
                    table.ForeignKey(
                        name: "FK_EvaluationRatings_Evaluations",
                        column: x => x.EvaluationID,
                        principalTable: "Evaluations",
                        principalColumn: "EvaluationID");
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Criteria",
                table: "EvaluationCriteria",
                columns: new[] { "TemplateID", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Evaluati__E08EC4DB369895DC",
                table: "EvaluationCycles",
                column: "CycleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationRatings_CriteriaID",
                table: "EvaluationRatings",
                column: "CriteriaID");

            migrationBuilder.CreateIndex(
                name: "UQ_EvaluationRatings",
                table: "EvaluationRatings",
                columns: new[] { "EvaluationID", "CriteriaID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_EmployeeID",
                table: "Evaluations",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_PrimaryEvaluatorID",
                table: "Evaluations",
                column: "PrimaryEvaluatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_SecondaryEvaluatorID",
                table: "Evaluations",
                column: "SecondaryEvaluatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_TemplateID",
                table: "Evaluations",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "UQ_Evaluations",
                table: "Evaluations",
                columns: new[] { "CycleID", "EmployeeID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Evaluati__A6C2DA66154EF91D",
                table: "EvaluationTemplates",
                column: "TemplateName",
                unique: true);
        }
    }
}
