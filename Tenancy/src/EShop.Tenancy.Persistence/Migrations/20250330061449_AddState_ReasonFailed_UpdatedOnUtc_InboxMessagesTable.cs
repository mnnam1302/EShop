using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Tenancy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddState_ReasonFailed_UpdatedOnUtc_InboxMessagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReasonFailed",
                table: "InboxMessages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "InboxMessages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedOnUtc",
                table: "InboxMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReasonFailed",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "State",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "UpdatedOnUtc",
                table: "InboxMessages");
        }
    }
}
