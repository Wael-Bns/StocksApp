using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxNotificationTriggerOnInsert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION notify_outbox_insert()
                RETURNS trigger AS $$
                BEGIN
                    PERFORM pg_notify(
                        'outbox_inserted',
                        json_build_object(
                            'outbox_id', NEW.""OutboxId"",
                            'event_type', NEW.""EventType"",
                            'payload', NEW.""Payload"",
                            'created_at', NEW.""CreatedAt""
                        )::text
                    );
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER outbox_insert_trigger
                AFTER INSERT ON ""Outbox""
                FOR EACH ROW
                EXECUTE FUNCTION notify_outbox_insert();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS outbox_insert_trigger ON ""Outbox"";
            ");

            migrationBuilder.Sql(@"
                DROP FUNCTION IF EXISTS notify_outbox_insert();
            ");
        }
    }
}
