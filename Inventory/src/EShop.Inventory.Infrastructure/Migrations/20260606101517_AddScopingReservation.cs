using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScopingReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "Reservations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Reservations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Reservations");
        }
    }
}
