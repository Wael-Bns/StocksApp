using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateBuyOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuyOrder",
                columns: table => new
                {
                    BuyOrderID = table.Column<Guid>(type: "uuid", nullable: false),
                    StockSymbol = table.Column<string>(type: "text", nullable: false),
                    StockName = table.Column<string>(type: "text", nullable: false),
                    DateAndTimeOfOrder = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyOrder", x => x.BuyOrderID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuyOrder");
        }
    }
}
