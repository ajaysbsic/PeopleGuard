using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeInvestigationSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixWarningLetterDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarningLetters_Investigations_InvestigationId",
                table: "WarningLetters");

            migrationBuilder.AddForeignKey(
                name: "FK_WarningLetters_Investigations_InvestigationId",
                table: "WarningLetters",
                column: "InvestigationId",
                principalTable: "Investigations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarningLetters_Investigations_InvestigationId",
                table: "WarningLetters");

            migrationBuilder.AddForeignKey(
                name: "FK_WarningLetters_Investigations_InvestigationId",
                table: "WarningLetters",
                column: "InvestigationId",
                principalTable: "Investigations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
