# StocksApp - Project Documentation

## 1. Purpose

StocksApp is a .NET 8 learning project that models a small stock-trading backend. It lets an authenticated user:

- register and sign in;
- retrieve a company profile and current quote;
- record buy orders;
- create limit-style sell orders; and
- receive live stock-price updates through SignalR.

The important business scenario is automated sell-order execution. A sell order is saved as **Pending**. When the market price for that symbol reaches or exceeds the order's requested price, a background worker marks the order **Executed** and credits the sale value to the user's cash balance.

This is not a production brokerage system. It does not model a portfolio/holdings ledger, reserve shares, integrate with a broker, or settle real trades. It is designed to demonstrate layered architecture, background processing, reliable messaging, real-time updates, authentication, and integration testing.

## 2. Solution at a glance

| Area | Technology / responsibility |
| --- | --- |
| Runtime | .NET 8, C# |
| HTTP API | ASP.NET Core Web API, controllers, Swagger in Development |
| Real-time client updates | SignalR (`/stocksHub`) |
| Persistence | EF Core with PostgreSQL and migrations |
| Market-data provider | Finnhub REST API and WebSocket feed |
| Async messaging | RabbitMQ through MassTransit |
| Reliable event handoff | Transactional outbox + PostgreSQL `LISTEN`/`NOTIFY` + polling fallback |
| Logs | Serilog, with Seq in Docker Compose |
| Tests | xUnit, Moq, FluentAssertions; integration tests use Testcontainers PostgreSQL |

## 3. Architecture

The solution uses a layered structure, with two worker processes alongside the API.

```text
Browser/client
   | HTTP + JWT                         SignalR
   v                                      ^
StocksApp.WebApi -------------------------|  live price notifications
   |                  |
   |                  +--> Finnhub REST (quote and company profile)
   |
   v
PostgreSQL <--- EF Core / repositories
   |
   | Outbox insert trigger: PostgreSQL NOTIFY
   v
StocksApp.OutboxDispatcher ---> RabbitMQ (SellOrderCreatedCommand)
                                      |
                                      v
                               StocksApp.OrdersWorker <--- Finnhub WebSocket
                                      |
                                      v
                            execute eligible sell orders in PostgreSQL
```

### Layer responsibilities

| Project | Responsibility |
| --- | --- |
| `StocksApp.Domain` | Business entities, sell-order status, domain event, repository interfaces, and query specifications. No framework dependencies. |
| `StocksApp.Core` | Use cases and application contracts: authentication, users, stock/order operations, DTOs, validation, JWT service, and abstractions for external systems. |
| `StocksApp.Infrastructure` | EF Core database context/configurations, repositories/unit of work, BCrypt password hashing, Finnhub clients, and MassTransit/RabbitMQ implementation. |
| `StocksApp.WebApi` | HTTP host: authentication/authorization, controllers, exception middleware, Swagger, SignalR hub, and a market-price broadcast service. |
| `StocksApp.OutboxDispatcher` | Worker that converts durable outbox records to RabbitMQ commands. |
| `StocksApp.OrdersWorker` | Worker that holds pending sell orders in memory, consumes created-order commands, watches market prices, and executes eligible orders. |
| `StocksApp.Test` / `StocksApp.Tests.Common` | Unit tests and shared test builders. |
| `StocksApp.IntegrationsTests` | API integration tests, using a PostgreSQL test container. |
| `postgres-init` | Database initialization script containing the migration history/schema and the outbox notification trigger. |

`StocksApp.Frontend` currently contains only local frontend environment/dependency artifacts; no frontend application source is included in this solution.

## 4. Main components and data model

### Entities

| Entity | Important fields | Meaning |
| --- | --- | --- |
| `User` | `UserId`, `UserName`, `Email`, `PasswordHash`, `CashBalance`, refresh-token fields | The account owner. A new account starts with a cash balance of `100000`. |
| `BuyOrder` | symbol, name, timestamp, quantity, price, `UserId` | A recorded buy order. It does not currently debit cash or create a holding. |
| `SellOrder` | symbol, name, timestamp, quantity, price, `Status`, `UserId` | A sell instruction. Status values are `Pending`, `Cancelled`, and `Executed`. |
| `Outbox` | event type, JSON payload, creation/processed timestamps | A durable message waiting to be delivered to RabbitMQ. |

Each buy/sell order belongs to one user. Database rules require a positive quantity and price; user cash balance cannot be negative. EF Core maps monetary columns as `decimal(18,2)`, although the corresponding entity properties are currently `double`.

### Domain event

`SellOrderCreatedCommand` contains the sell-order identifier, symbol, requested price, and creation time. It is placed in the outbox whenever a sell order is created, then later routed to the worker through RabbitMQ.

## 5. Request API

The API runs at `http://localhost:8088` when started through Docker Compose. In Development, Swagger is available at `http://localhost:8088/swagger`.

All trade endpoints require `Authorization: Bearer <access-token>`.

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/Auth/register` | Creates a user and returns an access token plus a refresh token. |
| `POST` | `/api/Auth/login` | Validates the email/password and returns a new token pair. |
| `POST` | `/api/Auth/generate-new-access-token` | Validates the supplied access/refresh token pair, rotates the refresh token, and returns a new pair. |
| `GET` | `/api/Trade/trade-info/{stockSymbol}` | Gets a Finnhub quote and company profile. Defaults to `MSFT` if no symbol is provided. |
| `POST` | `/api/Trade/buyorder` | Stores a buy-order record for the authenticated user. |
| `POST` | `/api/Trade/sellorder` | Stores a pending sell order and starts the asynchronous execution flow. |
| `GET` | `/api/Trade/allbuyorders` | Lists the current user's buy orders. |
| `GET` | `/api/Trade/allsellorders` | Lists the current user's sell orders, including status. |

### Typical payloads

Register:

```json
{
  "userName": "Ada",
  "email": "ada@example.com",
  "password": "at-least-six-characters"
}
```

Login:

```json
{
  "email": "ada@example.com",
  "password": "at-least-six-characters"
}
```

Buy or sell order (the server replaces any supplied user identity with the ID in the JWT):

```json
{
  "stockSymbol": "MSFT",
  "stockName": "Microsoft Corp",
  "dateAndTimeOfOrder": "2026-09-18T09:30:00Z",
  "quantity": 10,
  "price": 450.00
}
```

Input validation requires a symbol/name, a date after 2000-01-01, a quantity from 1 to 10,000, and a price from 1 to 10,000. The exception middleware returns client errors as `{ "message": "..." }`; unhandled failures return HTTP 500 with a generic message.

## 6. Authentication and authorization

- Passwords are hashed with BCrypt; plain-text passwords are not stored.
- JWT access tokens include the user's ID in the `NameIdentifier` claim. `ApiControllerBase` uses that claim to scope order operations to the current user.
- The configured access-token lifetime is 10 minutes and refresh-token lifetime is 30 minutes.
- On registration, login, and refresh, a new refresh token is generated and persisted. Refreshing rotates the previous token.
- JWT issuer, audience, signing key, Finnhub API key, and database connection settings are configuration values. Do not commit real secret values; use user secrets or environment variables.

## 7. Live prices and SignalR

`StockPricesHostedService` owns a Finnhub WebSocket client in the API process. It receives Finnhub trade messages and broadcasts every update to the SignalR group named after the stock symbol.

Clients connect to `/stocksHub?symbol=MSFT` and listen for:

```text
ReceivePriceUpdate(price)
```

`StocksHub` adds the connection to the symbol's group. It maintains an in-process subscriber count and subscribes to Finnhub when the first local client asks for a symbol; it unsubscribes when the last local client disconnects. CORS currently permits `http://localhost:5173` and `http://localhost:3000` with credentials.

## 8. Sell-order execution flow

The design separates durable order acceptance from delivery and execution. This avoids losing an accepted sell order if RabbitMQ is briefly unavailable.

1. The client sends `POST /api/Trade/sellorder`.
2. `StockService` validates the request and starts a database transaction.
3. It stores the pending `SellOrder` and a serialized `SellOrderCreatedCommand` in `Outbox`, then commits.
4. PostgreSQL's `outbox_insert_trigger` sends `NOTIFY outbox_inserted`.
5. `OutboxDispatcher` receives the notification, deserializes the event, sends the command to RabbitMQ's `orders` direct exchange with routing key `sellorder.created`, then marks the outbox row processed.
6. If the notification path is missed, the dispatcher periodically queries unprocessed outbox rows and retries delivery.
7. `SellOrderCreatedConsumer` in `OrdersWorker` puts the command on an internal channel.
8. The worker stores the pending order in a per-symbol sorted set and subscribes to that symbol's Finnhub WebSocket stream.
9. Each price update enters the same internal channel. For a symbol, orders whose target price is less than or equal to the market price are removed from the in-memory store.
10. `OrdersExecutionService` loads those orders in a transaction, marks them `Executed`, and increments each owner's cash balance by `price * quantity`.

On worker startup, `PendingOrdersInitializer` reloads pending orders from PostgreSQL and subscribes to their symbols. This lets the worker rebuild its in-memory queue after a restart.

## 9. Running the system locally

### Prerequisites

- .NET 8 SDK for local builds/tests.
- Docker Desktop for the full composed environment.
- A Finnhub API key.

### Configuration

At minimum configure these values before starting the API or worker outside Docker:

```text
ConnectionStrings__DefaultConnection=Host=localhost;Port=5434;Database=stocksappdb;Username=postgres;Password=password
FinnhubApiKey=<your-finnhub-key>
JWT__Key=<long-random-signing-key>
JWT__Issuer=StocksApp.WebApi
JWT__Audience=StocksApp.WebApi
RabbitMQ__HostName=localhost
RabbitMQ__Port=5672
RabbitMQ__UserName=admin
RabbitMQ__Password=admin
OutboxOptions__ConnectionString=Host=localhost;Port=5434;Database=stocksappdb;Username=postgres;Password=password
```

In Docker Compose, the containers use the service host names `postgres-container` and `rabbitmq-container`. The compose file supplies the database/RabbitMQ/Seq settings, but the Finnhub API key and JWT key still need to be provided safely (for example through environment variables or user secrets).

### Start with Docker Compose

```powershell
docker compose up --build
```

| Service | Host access |
| --- | --- |
| Web API | `http://localhost:8088` |
| Swagger (Development) | `http://localhost:8088/swagger` |
| PostgreSQL | `localhost:5434` |
| RabbitMQ management | `http://localhost:15672` (`admin` / `admin`) |
| Seq | `http://localhost:9090` |

`postgres-init/init.sql` initializes a new database volume. It contains the schema and the outbox trigger. If the existing PostgreSQL volume was initialized before a schema change, recreate that specific local development volume only after confirming that its data may be discarded.

### Build and test

```powershell
dotnet build .\StocksAppSolution.sln
dotnet test .\StocksApp.Test\StocksApp.Test.csproj
dotnet test .\StocksApp.IntegrationsTests\StocksApp.IntegrationsTests.csproj
```

Integration tests require Docker because they use Testcontainers PostgreSQL.

## 10. Observability and failure handling

- The Web API and Orders Worker configure Serilog. Docker Compose sends logs to Seq at `http://seq:5341`.
- The API logs HTTP requests and uses custom exception middleware for consistent error responses.
- The outbox listener reconnects after a database-notification connection failure and has a periodic polling fallback for unprocessed records.
- The outbox processor logs failed events and leaves them unprocessed, allowing a later polling cycle to retry them.
- The order worker logs errors while processing individual messages so one bad message does not stop its internal channel loop.

## 11. Implementation notes and current limitations

These are useful constraints for anyone extending the project:

- A `BuyOrder` is only recorded. It does not reduce cash balance or create inventory.
- A sell order is not checked against owned shares and no shares are reserved while it is pending.
- Execution credits the **order's requested price**, not the actual triggering market price.
- There is no API to cancel a sell order, even though `Cancelled` exists in the enum.
- The worker's pending-order store and subscription set are in memory. Startup reload restores database-pending orders, but multiple worker instances would each maintain their own local store and need additional coordination/idempotency design.
- The generic repository's `AddAsync` saves immediately. The sell-order service wraps its two adds in a transaction, so they are still committed or rolled back together, but this repository behavior should be considered before adding more multi-step use cases.
- The API price broadcaster and the order worker each establish their own Finnhub WebSocket connection; this is intentional process separation but means duplicate provider connections/subscriptions.
- CORS origins and Docker credentials are development defaults. Restrict origins, use secret storage, secure RabbitMQ/Seq, and enable TLS before deployment.
- There is no rate limiting, health-check endpoint, readiness handling, retry/backoff for Finnhub connections, or dead-letter policy visible in this solution.

## 12. Suggested extension points

For a fuller trading simulation, the next logical additions are:

1. Add a holdings/portfolio ledger and validate sell quantity against owned shares.
2. Make order acceptance and execution idempotent, then add cancellation and expiration.
3. Persist or coordinate worker state for safe horizontal scaling.
4. Add health checks for PostgreSQL, RabbitMQ, and Finnhub; use structured correlation IDs and metrics.
5. Secure deployment configuration: secrets manager, HTTPS, restricted CORS, real database credentials, and authenticated monitoring.
6. Add API versioning, pagination/filtering for order history, and OpenAPI request/response examples.
