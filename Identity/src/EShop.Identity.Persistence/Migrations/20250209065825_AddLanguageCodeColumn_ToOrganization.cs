using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageCodeColumn_ToOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationId1",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "UserRoles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserRoles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Roles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "RolePermissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)");

            migrationBuilder.AddColumn<string>(
                name: "LanguageCode",
                table: "Organizations",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId1",
                table: "Users",
                column: "OrganizationId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId1",
                table: "Users",
                column: "OrganizationId1",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId1",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId1",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId1",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LanguageCode",
                table: "Organizations");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "UserRoles",
                type: "character varying(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserRoles",
                type: "character varying(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Roles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "RolePermissions",
                type: "character varying(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
