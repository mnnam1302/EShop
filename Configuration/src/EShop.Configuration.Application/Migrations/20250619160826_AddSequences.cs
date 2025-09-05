using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Configuration.Application.Migrations
{
    /// <inheritdoc />
    public partial class AddSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sequences",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    NextAvailableValue = table.Column<int>(type: "integer", nullable: false),
                    ConcurrencyToken = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sequences", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sequences");
        }
    }
}
