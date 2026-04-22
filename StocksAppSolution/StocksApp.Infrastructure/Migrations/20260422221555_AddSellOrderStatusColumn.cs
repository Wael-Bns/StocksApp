using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSellOrderStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SellOrderStatus",
                table: "SellOrder",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "SellOrder",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "BuyOrder",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BuyOrderId",
                table: "ApplicationUser",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SellOrderId",
                table: "ApplicationUser",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SellOrder_UserId",
                table: "SellOrder",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyOrder_UserId",
                table: "BuyOrder",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuyOrder_ApplicationUser_UserId",
                table: "BuyOrder",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SellOrder_ApplicationUser_UserId",
                table: "SellOrder",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuyOrder_ApplicationUser_UserId",
                table: "BuyOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_SellOrder_ApplicationUser_UserId",
                table: "SellOrder");

            migrationBuilder.DropIndex(
                name: "IX_SellOrder_UserId",
                table: "SellOrder");

            migrationBuilder.DropIndex(
                name: "IX_BuyOrder_UserId",
                table: "BuyOrder");

            migrationBuilder.DropColumn(
                name: "SellOrderStatus",
                table: "SellOrder");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SellOrder");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BuyOrder");

            migrationBuilder.DropColumn(
                name: "BuyOrderId",
                table: "ApplicationUser");

            migrationBuilder.DropColumn(
                name: "SellOrderId",
                table: "ApplicationUser");
        }
    }
}
