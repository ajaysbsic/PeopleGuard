using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeInvestigationSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWarningLetterContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarningLetters_Employees_EmployeeId",
                table: "WarningLetters");

            migrationBuilder.DropForeignKey(
                name: "FK_WarningLetters_Investigations_InvestigationId",
                table: "WarningLetters");

            migrationBuilder.DropIndex(
                name: "IX_WarningLetters_InvestigationId",
                table: "WarningLetters");

            migrationBuilder.AddColumn<string>(
                name: "HtmlContent",
                table: "WarningLetters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Template",
                table: "WarningLetters",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CaseHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvestigationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseHistory_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CaseHistory_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WarningLetters_InvestigationId",
                table: "WarningLetters",
                column: "InvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseHistory_CreatedAt",
                table: "CaseHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CaseHistory_InvestigationId",
                table: "CaseHistory",
                column: "InvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseHistory_UserId",
                table: "CaseHistory",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WarningLetters_Employees_EmployeeId",
                table: "WarningLetters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WarningLetters_Investigations_InvestigationId",
                table: "WarningLetters",
                column: "InvestigationId",
                principalTable: "Investigations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarningLetters_Employees_EmployeeId",
                table: "WarningLetters");

            migrationBuilder.DropForeignKey(
                name: "FK_WarningLetters_Investigations_InvestigationId",
                table: "WarningLetters");

            migrationBuilder.DropTable(
                name: "CaseHistory");

            migrationBuilder.DropIndex(
                name: "IX_WarningLetters_InvestigationId",
                table: "WarningLetters");

            migrationBuilder.DropColumn(
                name: "HtmlContent",
                table: "WarningLetters");

            migrationBuilder.DropColumn(
                name: "Template",
                table: "WarningLetters");

            migrationBuilder.CreateIndex(
                name: "IX_WarningLetters_InvestigationId",
                table: "WarningLetters",
                column: "InvestigationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WarningLetters_Employees_EmployeeId",
                table: "WarningLetters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WarningLetters_Investigations_InvestigationId",
                table: "WarningLetters",
                column: "InvestigationId",
                principalTable: "Investigations",
                principalColumn: "Id");
        }
    }
}
