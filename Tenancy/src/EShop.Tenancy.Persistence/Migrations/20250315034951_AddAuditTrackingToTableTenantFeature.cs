using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Tenancy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTrackingToTableTenantFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TenantFeatures",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "TenantFeatures",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TenantFeatures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModifiedOnUtc",
                table: "TenantFeatures",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TenantFeatures");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "TenantFeatures");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TenantFeatures");

            migrationBuilder.DropColumn(
                name: "LastModifiedOnUtc",
                table: "TenantFeatures");
        }
    }
}
