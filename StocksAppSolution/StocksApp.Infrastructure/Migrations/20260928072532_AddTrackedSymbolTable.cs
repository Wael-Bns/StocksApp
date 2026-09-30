using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackedSymbolTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackedSymbols",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedSymbols", x => x.Symbol);
                    table.CheckConstraint("CK_TrackedSymbols_Symbol_Upper", "\"Symbol\" = upper(\"Symbol\")");
                });
            migrationBuilder.Sql(@"
                CREATE FUNCTION notify_tracked_symbols_changed() RETURNS trigger AS $$
                BEGIN
                    PERFORM pg_notify('tracked_symbols_changed', '');
                    RETURN NULL;
                END; $$ LANGUAGE plpgsql;");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_tracked_symbols_changed
                AFTER INSERT OR UPDATE OR DELETE ON ""TrackedSymbols""
                FOR EACH STATEMENT EXECUTE FUNCTION notify_tracked_symbols_changed();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_tracked_symbols_changed ON ""TrackedSymbols"";");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS notify_tracked_symbols_changed();");
            migrationBuilder.DropTable(
                name: "TrackedSymbols");
        }
    }
}
