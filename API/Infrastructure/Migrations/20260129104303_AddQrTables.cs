using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeInvestigationSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQrTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investigations_Employees_EmployeeId",
                table: "Investigations");

            migrationBuilder.DropForeignKey(
                name: "FK_WarningLetters_Employees_EmployeeId",
                table: "WarningLetters");

            migrationBuilder.CreateTable(
                name: "QrTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    QrPngPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QrTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QrTokens_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QrSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmitterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmitterEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmitterPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentUrls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedInvestigationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QrSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QrSubmissions_Investigations_RelatedInvestigationId",
                        column: x => x.RelatedInvestigationId,
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QrSubmissions_QrTokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "QrTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QrSubmissions_CreatedAt",
                table: "QrSubmissions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QrSubmissions_RelatedInvestigationId",
                table: "QrSubmissions",
                column: "RelatedInvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_QrSubmissions_TokenId",
                table: "QrSubmissions",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_QrTokens_CreatedBy",
                table: "QrTokens",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_QrTokens_ExpiresAt",
                table: "QrTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_QrTokens_Token",
                table: "QrTokens",
                column: "Token",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Investigations_Employees_EmployeeId",
                table: "Investigations",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WarningLetters_Employees_EmployeeId",
                table: "WarningLetters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investigations_Employees_EmployeeId",
                table: "Investigations");

            migrationBuilder.DropForeignKey(
                name: "FK_WarningLetters_Employees_EmployeeId",
                table: "WarningLetters");

            migrationBuilder.DropTable(
                name: "QrSubmissions");

            migrationBuilder.DropTable(
                name: "QrTokens");

            migrationBuilder.AddForeignKey(
                name: "FK_Investigations_Employees_EmployeeId",
                table: "Investigations",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WarningLetters_Employees_EmployeeId",
                table: "WarningLetters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
