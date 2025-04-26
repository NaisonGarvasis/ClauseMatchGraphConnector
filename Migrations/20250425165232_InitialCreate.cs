using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClauseMatchGraphConnector.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "TEXT", nullable: false),
                    LatestTitle = table.Column<string>(type: "TEXT", nullable: false),
                    LatestVersion = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentClass = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    LastPublishedAt = table.Column<string>(type: "TEXT", nullable: true),
                    Categories = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentUrl = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: "LatestCategory",
                columns: table => new
                {
                    CategoryId = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryName = table.Column<string>(type: "TEXT", nullable: false),
                    ClausematchDocumentDocumentId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LatestCategory", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_LatestCategory_Documents_ClausematchDocumentDocumentId",
                        column: x => x.ClausematchDocumentDocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LatestCategory_ClausematchDocumentDocumentId",
                table: "LatestCategory",
                column: "ClausematchDocumentDocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LatestCategory");

            migrationBuilder.DropTable(
                name: "Documents");
        }
    }
}
