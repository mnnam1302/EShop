using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SkuId",
                table: "Inventories",
                newName: "ProductId");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAtUtc",
                table: "Inventories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Inventories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModifiedAtUtc",
                table: "Inventories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedByUserId",
                table: "Inventories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumStock",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReservedStock",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "Inventories",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "LastModifiedAtUtc",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "MinimumStock",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ReservedStock",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Inventories",
                newName: "SkuId");
        }
    }
}
