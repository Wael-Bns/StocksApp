using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBuyOrderConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SellOrder",
                table: "SellOrder");

            migrationBuilder.RenameTable(
                name: "SellOrder",
                newName: "SellOrders");

            migrationBuilder.AlterColumn<string>(
                name: "StockSymbol",
                table: "BuyOrder",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "StockName",
                table: "BuyOrder",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "BuyOrder",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<string>(
                name: "StockSymbol",
                table: "SellOrders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "StockName",
                table: "SellOrders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SellOrders",
                table: "SellOrders",
                column: "SellOrderID");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BuyOrder_Price",
                table: "BuyOrder",
                sql: "\"Price\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BuyOrder_Quantity",
                table: "BuyOrder",
                sql: "\"Quantity\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BuyOrder_Price",
                table: "BuyOrder");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BuyOrder_Quantity",
                table: "BuyOrder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SellOrders",
                table: "SellOrders");

            migrationBuilder.RenameTable(
                name: "SellOrders",
                newName: "SellOrder");

            migrationBuilder.AlterColumn<string>(
                name: "StockSymbol",
                table: "BuyOrder",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "StockName",
                table: "BuyOrder",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "BuyOrder",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "StockSymbol",
                table: "SellOrder",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StockName",
                table: "SellOrder",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SellOrder",
                table: "SellOrder",
                column: "SellOrderID");
        }
    }
}
