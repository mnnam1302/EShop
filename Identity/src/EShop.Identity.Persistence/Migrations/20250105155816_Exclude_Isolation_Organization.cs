using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Exclude_Isolation_Organization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Organizations");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Organizations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Organizations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "Organizations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Organizations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
