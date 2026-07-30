# TRADIFY — StocksApp Frontend

A React + TypeScript client for `StocksApp.WebApi`, built with Vite. Implements only
the endpoints declared in the provided OpenAPI document, plus the live price
stream described by `StockPricesHostedService` / `StocksHub`.

## Stack

- **React 19 + TypeScript**, Vite build
- **React Router** for client-side routing (`/auth`, `/dashboard`, `/orders`)
- **Axios** for HTTP, with a request interceptor that attaches the JWT and a
  response interceptor that refreshes it once on a 401
- **@microsoft/signalr** for the real-time price stream
- **Tailwind CSS**, configured from the "Obsidian Refined" design tokens

## Getting started

```bash
npm install
cp .env.example .env   # adjust VITE_API_BASE_URL if the API isn't on :8089
npm run dev
```

The API base address defaults to `https://localhost:8089` (matching the brief).
Because that's HTTPS on localhost, your browser will need to trust the
backend's dev certificate, or you'll see network errors on every request —
run `dotnet dev-certs https --trust` on the API project if you haven't.

## Architecture

```
src/
  types/        DTOs mirrored 1:1 from the OpenAPI schemas
  config/       Base URL, route paths, SignalR hub/event names
  services/     httpClient (axios + interceptors), authService, tradeService,
                signalRService — the only files that talk to the network
  utils/        jwt decoding, localStorage session/watchlist persistence
  context/      AuthContext (session state), ToastContext (notifications)
  hooks/        useAuth, useToast, useStockPrice, useSymbolTicks, useHubStatus
  components/
    common/     Button, Input, Card, Toast, StatusPill, EmptyState
    layout/     AppShell (background), Sidebar (nav), TopBar, ProtectedRoute
    auth/       LoginForm, RegisterForm
    trade/      MarketSearch, Watchlist, MarketPanel, PriceChart, TradePanel,
                ActivityTable, OrdersTable
  pages/        AuthPage, MarketsPage, OrdersPage
```

`MarketsPage` is the trading terminal: a left watchlist, a center market
panel (price header + live chart) and activity table, and a right-hand
Buy/Sell execute panel — laid out to match the terminal-style reference
design. Everything on it is backed by a real endpoint or derived from one:

- **Watchlist** — a fixed reference list of the top 50 large-cap tickers
  (`src/config/topSymbols.ts`), since the API has no "top symbols" or
  watchlist endpoint. Every row opens its own live subscription on
  `stocksHub` (via `signalRService`), so all 50 stream concurrently and
  update in place — the symbol list itself is static, but every price next
  to it is real, pushed from your `StockPricesHostedService`. A filter box
  narrows the list client-side.
- **Search** — a quick lookup (`GET /trade-info/{symbol}`) for a symbol
  outside the top 50; resolving one sets it as the active symbol.
- **Live chart** — the OpenAPI document has no historical OHLC endpoint, so
  the chart is built purely from ticks received over SignalR during the
  current session; it starts empty on every symbol change.
- **Est. Total / session change %** are computed client-side from real
  numbers (price × qty, latest vs. first tick) — nothing here is invented
  data like a fake balance or a fabricated daily P&L.

## UI present but intentionally not wired up

The reference design includes several controls with no backing endpoint in
the OpenAPI document. Rather than fake their behavior, these are rendered
for visual completeness but left inert — `disabled`, non-clickable, or
labeled "Soon" — with a `title` tooltip explaining why:

- Sidebar: **Portfolio / History / Alerts** (no holdings, trade-history, or
  notifications endpoints)
- Top bar: **Deposit**, notification/settings/help icons (no account,
  notifications, or settings endpoints)
- Trade panel: **Order Type** dropdown locked to "Market Order" (the API
  only takes a fixed price + quantity, no order-type field exists to
  switch), **25/50/75/MAX** quick-size buttons, and the **Trading Fee** /
  **Available Balance** rows (no account/balance/fee endpoint)
- Activity panel: **Trade History** and **Positions** tabs (the API only
  exposes the full open buy/sell order lists, not a filled-history or
  holdings view — "Active Orders" is the one tab actually wired up)

Each API resource has one service file; components never call `axios`
directly. Response shapes the OpenAPI document doesn't specify (trade-info,
order listings) are typed permissively and rendered generically (key/value
grid, dynamic table columns) rather than assuming fields that aren't
documented.

## Endpoints implemented

| Method | Path | Used by |
|---|---|---|
| POST | `/api/Auth/register` | Register form |
| POST | `/api/Auth/login` | Login form |
| POST | `/api/Auth/generate-new-access-token` | Automatic 401 retry in `httpClient` |
| GET | `/api/Trade/trade-info/{stockSymbol}` | Stock Lookup card |
| POST | `/api/Trade/buyorder` | Buy Order form |
| POST | `/api/Trade/sellorder` | Sell Order form |
| GET | `/api/Trade/allbuyorders` | Orders page, "Buy" tab |
| GET | `/api/Trade/allsellorders` | Orders page, "Sell" tab |

## Real-time prices

`StockPricesHostedService` pushes `ReceivePriceUpdate(price)` to
`Clients.Group(stockSymbol)` on the `stocksHub` hub. `signalRService.ts`
opens one shared connection (auth'd with the JWT via `accessTokenFactory`)
and, per subscribed symbol, listens for that event and fans it out to any
component using `useStockPrice(symbol)` (currently the `PriceTicker` on the
dashboard).

**One assumption**: the code you provided shows the server *pushing* updates
to a group, but not the hub method clients call to *join* that group. This
project calls `connection.invoke("SubscribeToSymbol", symbol)` /
`"UnsubscribeFromSymbol"` — update `SIGNALR_METHODS` in
`src/config/constants.ts` if your `StocksHub` names those methods
differently.

## Known API/UI constraint

`BuyOrderAddRequest` requires a `userId` (uuid), but neither `/login` nor
`/register` returns one in `AuthenticationResponse`. This client decodes the
JWT's `nameid`/`sub` claim client-side to recover it (`src/utils/jwt.ts`) —
if your access token doesn't carry the user id under one of those claim
types, buy orders will fail with a toast asking you to check the session.
