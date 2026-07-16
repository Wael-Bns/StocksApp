using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusColumnInSellOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SellOrder",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "SellOrder");
        }
    }
}
