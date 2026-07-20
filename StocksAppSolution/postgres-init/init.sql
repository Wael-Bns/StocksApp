CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "BuyOrder" (
    "BuyOrderID" uuid NOT NULL,
    "StockSymbol" text NOT NULL,
    "StockName" text NOT NULL,
    "DateAndTimeOfOrder" timestamp with time zone NOT NULL,
    "Quantity" bigint NOT NULL,
    "Price" double precision NOT NULL,
    CONSTRAINT "PK_BuyOrder" PRIMARY KEY ("BuyOrderID")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260211144037_CreateBuyOrderTable', '8.0.23');

COMMIT;

START TRANSACTION;

CREATE TABLE "SellOrder" (
    "SellOrderID" uuid NOT NULL,
    "StockSymbol" text NOT NULL,
    "StockName" text NOT NULL,
    "DateAndTimeOfOrder" timestamp with time zone NOT NULL,
    "Quantity" bigint NOT NULL,
    "Price" double precision NOT NULL,
    CONSTRAINT "PK_SellOrder" PRIMARY KEY ("SellOrderID")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260211144123_CreateSellOrderTable', '8.0.23');

COMMIT;

START TRANSACTION;

ALTER TABLE "SellOrder" DROP CONSTRAINT "PK_SellOrder";

ALTER TABLE "SellOrder" RENAME TO "SellOrders";

ALTER TABLE "BuyOrder" ALTER COLUMN "StockSymbol" TYPE character varying(10);

ALTER TABLE "BuyOrder" ALTER COLUMN "StockName" DROP NOT NULL;

ALTER TABLE "BuyOrder" ALTER COLUMN "Price" TYPE numeric(18,2);

ALTER TABLE "SellOrders" ALTER COLUMN "StockSymbol" DROP NOT NULL;

ALTER TABLE "SellOrders" ALTER COLUMN "StockName" DROP NOT NULL;

ALTER TABLE "SellOrders" ADD CONSTRAINT "PK_SellOrders" PRIMARY KEY ("SellOrderID");

ALTER TABLE "BuyOrder" ADD CONSTRAINT "CK_BuyOrder_Price" CHECK ("Price" > 0);

ALTER TABLE "BuyOrder" ADD CONSTRAINT "CK_BuyOrder_Quantity" CHECK ("Quantity" > 0);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260506091813_AddBuyOrderConstraints', '8.0.23');

COMMIT;

START TRANSACTION;

ALTER TABLE "SellOrders" DROP CONSTRAINT "PK_SellOrders";

ALTER TABLE "SellOrders" RENAME TO "SellOrder";

ALTER TABLE "SellOrder" ALTER COLUMN "StockSymbol" TYPE character varying(10);
UPDATE "SellOrder" SET "StockSymbol" = '' WHERE "StockSymbol" IS NULL;
ALTER TABLE "SellOrder" ALTER COLUMN "StockSymbol" SET NOT NULL;
ALTER TABLE "SellOrder" ALTER COLUMN "StockSymbol" SET DEFAULT '';

ALTER TABLE "SellOrder" ALTER COLUMN "Price" TYPE numeric(18,2);

ALTER TABLE "SellOrder" ADD CONSTRAINT "PK_SellOrder" PRIMARY KEY ("SellOrderID");

ALTER TABLE "SellOrder" ADD CONSTRAINT "CK_SellOrder_Price" CHECK ("Price" > 0);

ALTER TABLE "SellOrder" ADD CONSTRAINT "CK_SellOrder_Quantity" CHECK ("Quantity" > 0);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260506091845_AddSellOrderConstraints', '8.0.23');

COMMIT;

START TRANSACTION;

ALTER TABLE "SellOrder" ADD "UserId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE "BuyOrder" ADD "UserId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

CREATE TABLE "Users" (
    "UserId" uuid NOT NULL,
    "UserName" text,
    "Email" text,
    "PasswordHash" text,
    "CashBalance" numeric NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("UserId")
);

CREATE INDEX "IX_SellOrder_UserId" ON "SellOrder" ("UserId");

CREATE INDEX "IX_BuyOrder_UserId" ON "BuyOrder" ("UserId");

ALTER TABLE "BuyOrder" ADD CONSTRAINT "FK_BuyOrder_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE;

ALTER TABLE "SellOrder" ADD CONSTRAINT "FK_SellOrder_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260506092123_AddUsersTable', '8.0.23');

COMMIT;

START TRANSACTION;

ALTER TABLE "BuyOrder" DROP CONSTRAINT "FK_BuyOrder_Users_UserId";

ALTER TABLE "SellOrder" DROP CONSTRAINT "FK_SellOrder_Users_UserId";

ALTER TABLE "Users" DROP CONSTRAINT "PK_Users";

ALTER TABLE "Users" RENAME TO "User";

UPDATE "User" SET "UserName" = '' WHERE "UserName" IS NULL;
ALTER TABLE "User" ALTER COLUMN "UserName" SET NOT NULL;
ALTER TABLE "User" ALTER COLUMN "UserName" SET DEFAULT '';

UPDATE "User" SET "PasswordHash" = '' WHERE "PasswordHash" IS NULL;
ALTER TABLE "User" ALTER COLUMN "PasswordHash" SET NOT NULL;
ALTER TABLE "User" ALTER COLUMN "PasswordHash" SET DEFAULT '';

UPDATE "User" SET "Email" = '' WHERE "Email" IS NULL;
ALTER TABLE "User" ALTER COLUMN "Email" SET NOT NULL;
ALTER TABLE "User" ALTER COLUMN "Email" SET DEFAULT '';

ALTER TABLE "User" ALTER COLUMN "CashBalance" TYPE numeric(18,2);

ALTER TABLE "User" ADD CONSTRAINT "PK_User" PRIMARY KEY ("UserId");

ALTER TABLE "User" ADD CONSTRAINT "CK_User_CashBalance_NonNegative" CHECK ("CashBalance" >= 0);

ALTER TABLE "BuyOrder" ADD CONSTRAINT "FK_BuyOrder_User_UserId" FOREIGN KEY ("UserId") REFERENCES "User" ("UserId") ON DELETE CASCADE;

ALTER TABLE "SellOrder" ADD CONSTRAINT "FK_SellOrder_User_UserId" FOREIGN KEY ("UserId") REFERENCES "User" ("UserId") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260506163118_AddUserTableConfiguration', '8.0.23');

COMMIT;

START TRANSACTION;

ALTER TABLE "User" DROP CONSTRAINT "CK_User_CashBalance_NonNegative";

ALTER TABLE "User" ADD "RefreshToken" text;

ALTER TABLE "User" ADD "RefreshTokenExpiry" timestamp with time zone;

ALTER TABLE "User" ADD CONSTRAINT "CK_User_CashBalance_NonNegative" CHECK ("CashBalance" >= 0);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260506210622_AddRefreshTokenColumnInUserTable', '8.0.23');

COMMIT;

START TRANSACTION;

ALTER TABLE "SellOrder" ADD "Status" integer NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260529090659_AddStatusColumnInSellOrder', '8.0.23');

COMMIT;

START TRANSACTION;

CREATE TABLE "Outbox" (
    "OutboxId" uuid NOT NULL,
    "EventType" text NOT NULL,
    "Payload" jsonb NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    "ProcessedAt" timestamp with time zone,
    CONSTRAINT "PK_Outbox" PRIMARY KEY ("OutboxId")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260716114224_AddOutboxTable', '8.0.23');

COMMIT;

START TRANSACTION;


                CREATE OR REPLACE FUNCTION notify_outbox_insert()
                RETURNS trigger AS $$
                BEGIN
                    PERFORM pg_notify(
                        'outbox_inserted',
                        json_build_object(
                            'outbox_id', NEW."OutboxId",
                            'event_type', NEW."EventType",
                            'payload', NEW."Payload",
                            'created_at', NEW."CreatedAt"
                        )::text
                    );
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            


                CREATE TRIGGER outbox_insert_trigger
                AFTER INSERT ON "Outbox"
                FOR EACH ROW
                EXECUTE FUNCTION notify_outbox_insert();
            

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260716121120_AddOutboxNotificationTriggerOnInsert', '8.0.23');

COMMIT;

