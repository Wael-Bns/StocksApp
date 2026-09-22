using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEventColumnNameInOutboxTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventType",
                table: "Outbox",
                newName: "Event");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Event",
                table: "Outbox",
                newName: "EventType");
        }
    }
}
