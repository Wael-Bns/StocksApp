CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260211144037_CreateBuyOrderTable') THEN
    CREATE TABLE "BuyOrder" (
        "BuyOrderID" uuid NOT NULL,
        "StockSymbol" text NOT NULL,
        "StockName" text NOT NULL,
        "DateAndTimeOfOrder" timestamp with time zone NOT NULL,
        "Quantity" bigint NOT NULL,
        "Price" double precision NOT NULL,
        CONSTRAINT "PK_BuyOrder" PRIMARY KEY ("BuyOrderID")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260211144037_CreateBuyOrderTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260211144037_CreateBuyOrderTable', '8.0.23');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260211144123_CreateSellOrderTable') THEN
    CREATE TABLE "SellOrder" (
        "SellOrderID" uuid NOT NULL,
        "StockSymbol" text NOT NULL,
        "StockName" text NOT NULL,
        "DateAndTimeOfOrder" timestamp with time zone NOT NULL,
        "Quantity" bigint NOT NULL,
        "Price" double precision NOT NULL,
        CONSTRAINT "PK_SellOrder" PRIMARY KEY ("SellOrderID")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260211144123_CreateSellOrderTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260211144123_CreateSellOrderTable', '8.0.23');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091813_AddBuyOrderConstraints') THEN
    ALTER TABLE "SellOrder" DROP CONSTRAINT "PK_SellOrder";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091813_AddBuyOrderConstraints') THEN
    ALTER TABLE "SellOrder" RENAME TO "SellOrders";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091813_AddBuyOrderConstraints') THEN
    ALTER TABLE "BuyOrder" ALTER COLUMN "StockSymbol" TYPE character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091813_AddBuyOrderConstraints') THEN
    ALTER TABLE "BuyOrder" ALTER COLUMN "StockName" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091813_AddBuyOrderConstraints') THEN
    ALTER TABLE "BuyOrder" ALTER COLUMN "Price" TYPE numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091813_AddBuyOrderConstraints') THEN
    ALTER TABLE "SellOrders" ALTER COLUMN "StockSymbol" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091813_AddBuyOrderConstraints') THEN
    ALTER TABLE "SellOrders" ALTER COLUMN "StockName" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091813_AddBuyOrderConstraints') THEN
    ALTER TABLE "SellOrders" ADD CONSTRAINT "PK_SellOrders" PRIMARY KEY ("SellOrderID");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091813_AddBuyOrderConstraints') THEN
    ALTER TABLE "BuyOrder" ADD CONSTRAINT "CK_BuyOrder_Price" CHECK ("Price" > 0);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091813_AddBuyOrderConstraints') THEN
    ALTER TABLE "BuyOrder" ADD CONSTRAINT "CK_BuyOrder_Quantity" CHECK ("Quantity" > 0);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091813_AddBuyOrderConstraints') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260506091813_AddBuyOrderConstraints', '8.0.23');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091845_AddSellOrderConstraints') THEN
    ALTER TABLE "SellOrders" DROP CONSTRAINT "PK_SellOrders";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091845_AddSellOrderConstraints') THEN
    ALTER TABLE "SellOrders" RENAME TO "SellOrder";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091845_AddSellOrderConstraints') THEN
    ALTER TABLE "SellOrder" ALTER COLUMN "StockSymbol" TYPE character varying(10);
    UPDATE "SellOrder" SET "StockSymbol" = '' WHERE "StockSymbol" IS NULL;
    ALTER TABLE "SellOrder" ALTER COLUMN "StockSymbol" SET NOT NULL;
    ALTER TABLE "SellOrder" ALTER COLUMN "StockSymbol" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091845_AddSellOrderConstraints') THEN
    ALTER TABLE "SellOrder" ALTER COLUMN "Price" TYPE numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091845_AddSellOrderConstraints') THEN
    ALTER TABLE "SellOrder" ADD CONSTRAINT "PK_SellOrder" PRIMARY KEY ("SellOrderID");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091845_AddSellOrderConstraints') THEN
    ALTER TABLE "SellOrder" ADD CONSTRAINT "CK_SellOrder_Price" CHECK ("Price" > 0);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091845_AddSellOrderConstraints') THEN
    ALTER TABLE "SellOrder" ADD CONSTRAINT "CK_SellOrder_Quantity" CHECK ("Quantity" > 0);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506091845_AddSellOrderConstraints') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260506091845_AddSellOrderConstraints', '8.0.23');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506092123_AddUsersTable') THEN
    ALTER TABLE "SellOrder" ADD "UserId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506092123_AddUsersTable') THEN
    ALTER TABLE "BuyOrder" ADD "UserId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506092123_AddUsersTable') THEN
    CREATE TABLE "Users" (
        "UserId" uuid NOT NULL,
        "UserName" text,
        "Email" text,
        "PasswordHash" text,
        "CashBalance" numeric NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("UserId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506092123_AddUsersTable') THEN
    CREATE INDEX "IX_SellOrder_UserId" ON "SellOrder" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506092123_AddUsersTable') THEN
    CREATE INDEX "IX_BuyOrder_UserId" ON "BuyOrder" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506092123_AddUsersTable') THEN
    ALTER TABLE "BuyOrder" ADD CONSTRAINT "FK_BuyOrder_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506092123_AddUsersTable') THEN
    ALTER TABLE "SellOrder" ADD CONSTRAINT "FK_SellOrder_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506092123_AddUsersTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260506092123_AddUsersTable', '8.0.23');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    ALTER TABLE "BuyOrder" DROP CONSTRAINT "FK_BuyOrder_Users_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    ALTER TABLE "SellOrder" DROP CONSTRAINT "FK_SellOrder_Users_UserId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    ALTER TABLE "Users" DROP CONSTRAINT "PK_Users";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    ALTER TABLE "Users" RENAME TO "User";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    UPDATE "User" SET "UserName" = '' WHERE "UserName" IS NULL;
    ALTER TABLE "User" ALTER COLUMN "UserName" SET NOT NULL;
    ALTER TABLE "User" ALTER COLUMN "UserName" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    UPDATE "User" SET "PasswordHash" = '' WHERE "PasswordHash" IS NULL;
    ALTER TABLE "User" ALTER COLUMN "PasswordHash" SET NOT NULL;
    ALTER TABLE "User" ALTER COLUMN "PasswordHash" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    UPDATE "User" SET "Email" = '' WHERE "Email" IS NULL;
    ALTER TABLE "User" ALTER COLUMN "Email" SET NOT NULL;
    ALTER TABLE "User" ALTER COLUMN "Email" SET DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    ALTER TABLE "User" ALTER COLUMN "CashBalance" TYPE numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    ALTER TABLE "User" ADD CONSTRAINT "PK_User" PRIMARY KEY ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    ALTER TABLE "User" ADD CONSTRAINT "CK_User_CashBalance_NonNegative" CHECK ("CashBalance" >= 0);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    ALTER TABLE "BuyOrder" ADD CONSTRAINT "FK_BuyOrder_User_UserId" FOREIGN KEY ("UserId") REFERENCES "User" ("UserId") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    ALTER TABLE "SellOrder" ADD CONSTRAINT "FK_SellOrder_User_UserId" FOREIGN KEY ("UserId") REFERENCES "User" ("UserId") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506163118_AddUserTableConfiguration') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260506163118_AddUserTableConfiguration', '8.0.23');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506210622_AddRefreshTokenColumnInUserTable') THEN
    ALTER TABLE "User" DROP CONSTRAINT "CK_User_CashBalance_NonNegative";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506210622_AddRefreshTokenColumnInUserTable') THEN
    ALTER TABLE "User" ADD "RefreshToken" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506210622_AddRefreshTokenColumnInUserTable') THEN
    ALTER TABLE "User" ADD "RefreshTokenExpiry" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506210622_AddRefreshTokenColumnInUserTable') THEN
    ALTER TABLE "User" ADD CONSTRAINT "CK_User_CashBalance_NonNegative" CHECK ("CashBalance" >= 0);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260506210622_AddRefreshTokenColumnInUserTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260506210622_AddRefreshTokenColumnInUserTable', '8.0.23');
    END IF;
END $EF$;
COMMIT;

