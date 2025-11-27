using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EShop.Catalog.Application.Migrations
{
    /// <inheritdoc />
    public partial class AddEventStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ConcurrencyToken",
                table: "Sequences",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "EventStores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Version = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Event = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventStores_AggregateId_Version",
                table: "EventStores",
                columns: new[] { "AggregateId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventStores");

            migrationBuilder.AlterColumn<string>(
                name: "ConcurrencyToken",
                table: "Sequences",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
