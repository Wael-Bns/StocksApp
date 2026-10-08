using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSellOrderMatching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "candle_match_flush_backup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    BucketStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Open = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    High = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    Low = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    Close = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false),
                    TradeCount = table.Column<int>(type: "integer", nullable: false),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candle_match_flush_backup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "candle_matches_5s",
                columns: table => new
                {
                    symbol = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    bucket_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    open = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    high = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    low = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    close = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    volume = table.Column<long>(type: "bigint", nullable: false),
                    trade_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candle_matches_5s", x => new { x.symbol, x.bucket_start });
                });

            migrationBuilder.Sql(@"SELECT create_hypertable('candle_matches_5s', 'bucket_start', if_not_exists => TRUE);");
            migrationBuilder.Sql(@"SELECT add_retention_policy('candle_matches_5s', INTERVAL '7 days', if_not_exists => TRUE);");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"SELECT remove_retention_policy('candle_matches_5s', if_exists => TRUE);");

            migrationBuilder.DropTable(
                name: "candle_match_flush_backup");

            migrationBuilder.DropTable(
                name: "candle_matches_5s");
        }
    }
}
