/**
 * Types mirroring the Trade schemas in the StocksApp.WebApi OpenAPI document.
 *
 * NOTE: the OpenAPI document only declares request schemas for the Trade
 * endpoints (`BuyOrderAddRequest`, `SellOrderAddRequest`). The response
 * bodies for GET /trade-info/{symbol}, GET /allbuyorders and
 * GET /allsellorders are documented only as "200 OK" with no schema, so the
 * response types below are intentionally permissive (known likely fields as
 * optional, plus an index signature) and the UI renders whatever keys the
 * API actually returns rather than assuming a fixed shape.
 */

/** POST /api/Trade/buyorder body */
export interface BuyOrderAddRequest {
  stockSymbol: string;
  stockName: string;
  dateAndTimeOfOrder?: string;
  /** min: 1, max: 10000 */
  quantity: number;
  /** min: 1, max: 10000 */
  price: number;
  /** uuid — required by the API, not returned by /login or /register */
  userId: string;
}

/** POST /api/Trade/sellorder body */
export interface SellOrderAddRequest {
  stockSymbol: string;
  stockName: string;
  dateAndTimeOfOrder?: string;
  /** min: 1, max: 10000 */
  quantity: number;
  /** min: 1, max: 10000 */
  price: number;
}

/** Unspecified response shape for GET /trade-info/{stockSymbol} */
export interface TradeInfoResponse {
  stockSymbol?: string;
  stockName?: string;
  price?: number;
  [key: string]: unknown;
}

/** Unspecified response shape for a single row in /allbuyorders or /allsellorders */
export interface OrderResponse {
  stockSymbol?: string;
  stockName?: string;
  dateAndTimeOfOrder?: string;
  quantity?: number;
  price?: number;
  userId?: string;
  [key: string]: unknown;
}
