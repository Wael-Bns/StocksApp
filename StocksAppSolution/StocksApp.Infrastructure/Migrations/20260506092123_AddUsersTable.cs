using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    CashBalance = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SellOrder_UserId",
                table: "SellOrder",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyOrder_UserId",
                table: "BuyOrder",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuyOrder_Users_UserId",
                table: "BuyOrder",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SellOrder_Users_UserId",
                table: "SellOrder",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuyOrder_Users_UserId",
                table: "BuyOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_SellOrder_Users_UserId",
                table: "SellOrder");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_SellOrder_UserId",
                table: "SellOrder");

            migrationBuilder.DropIndex(
                name: "IX_BuyOrder_UserId",
                table: "BuyOrder");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SellOrder");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BuyOrder");
        }
    }
}
