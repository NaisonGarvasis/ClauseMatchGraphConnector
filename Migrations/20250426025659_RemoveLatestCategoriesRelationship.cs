using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClauseMatchGraphConnector.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLatestCategoriesRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LatestCategory");

            migrationBuilder.AlterColumn<string>(
                name: "LastPublishedAt",
                table: "Documents",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastPublishedAt",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

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
    }
}
