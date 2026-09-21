/**
 * The API base address, exactly as provided.
 * Override with VITE_API_BASE_URL in a .env file if the backend runs elsewhere.
 */
export const API_BASE_URL: string =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:8088/api";

export const API_ROUTES = {
  register: "/Auth/register",
  login: "/Auth/login",
  refreshToken: "/Auth/generate-new-access-token",
  tradeInfo: (stockSymbol: string) =>
    `/Trade/trade-info/${encodeURIComponent(stockSymbol)}`,
  buyOrder: "/Trade/buyorder",
  sellOrder: "/Trade/sellorder",
  allBuyOrders: "/Trade/allbuyorders",
  allSellOrders: "/Trade/allsellorders",
} as const;

/**
 * The SignalR hub is mapped server-side as `app.MapHub<StocksHub>("stocksHub")`,
 * and the hosted service pushes price updates to per-symbol groups via
 * `Clients.Group(trade.StockSymbol).SendAsync("ReceivePriceUpdate", trade.Price)`.
 *
 * The hub's client-callable method for *joining* a symbol's group isn't part
 * of the provided code, so its name is the one assumption in this file —
 * adjust SUBSCRIBE/UNSUBSCRIBE below to match your actual StocksHub method
 * names if they differ.
 */
export const SIGNALR_HUB_URL = `${API_BASE_URL}/stocksHub`;

export const SIGNALR_EVENTS = {
  receivePriceUpdate: "ReceivePriceUpdate",
} as const;

export const SIGNALR_METHODS = {
  subscribe: "SubscribeToSymbol",
  unsubscribe: "UnsubscribeFromSymbol",
} as const;

export const AUTH_STORAGE_KEY = "stocksapp.auth";
