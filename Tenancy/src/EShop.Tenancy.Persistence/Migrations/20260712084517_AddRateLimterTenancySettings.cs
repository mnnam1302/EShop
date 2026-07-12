using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Tenancy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRateLimterTenancySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RateLimitPolicy",
                table: "TenantSettings",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RateLimitPolicy",
                table: "TenantSettings");
        }
    }
}
