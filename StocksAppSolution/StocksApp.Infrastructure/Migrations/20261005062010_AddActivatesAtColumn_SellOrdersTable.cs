using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivatesAtColumn_SellOrdersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatesAt",
                table: "SellOrder",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "ix_sellorders_pending_symbol",
                table: "SellOrder",
                columns: new[] { "StockSymbol", "ActivatesAt" },
                filter: "\"Status\" = 'Pending'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sellorders_pending_symbol",
                table: "SellOrder");

            migrationBuilder.DropColumn(
                name: "ActivatesAt",
                table: "SellOrder");
        }
    }
}
