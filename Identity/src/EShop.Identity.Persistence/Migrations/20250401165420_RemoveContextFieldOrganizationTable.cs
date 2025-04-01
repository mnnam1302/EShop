using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveContextFieldOrganizationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Context_Path",
                table: "Organizations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Context_Path",
                table: "Organizations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
