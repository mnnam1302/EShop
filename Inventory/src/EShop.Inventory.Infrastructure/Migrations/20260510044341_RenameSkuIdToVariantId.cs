using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameSkuIdToVariantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SkuId",
                table: "Inventories",
                newName: "VariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VariantId",
                table: "Inventories",
                newName: "SkuId");
        }
    }
}
