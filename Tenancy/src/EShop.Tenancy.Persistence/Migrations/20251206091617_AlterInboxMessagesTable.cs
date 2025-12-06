using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Tenancy.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlterInboxMessagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MessageId",
                table: "InboxMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_MessageId_ConsumerId",
                table: "InboxMessages",
                columns: new[] { "MessageId", "ConsumerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InboxMessages_MessageId_ConsumerId",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "InboxMessages");
        }
    }
}
