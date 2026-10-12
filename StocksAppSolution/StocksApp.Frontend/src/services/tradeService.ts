import { API_ROUTES } from "../config/constants";
import { httpClient } from "./httpClient";
import type {
  BuyOrderAddRequest,
  OrderResponse,
  SellOrderAddRequest,
  TradeInfoResponse,
  CandleHydrationResponse,
  CandleInterval,
  MarketPrice,
} from "../types/trade.types";

export const tradeService = {
  async getTradeInfo(stockSymbol: string): Promise<TradeInfoResponse> {
    const { data } = await httpClient.get<TradeInfoResponse>(
      API_ROUTES.tradeInfo(stockSymbol),
    );
    return data;
  },

  async placeBuyOrder(payload: BuyOrderAddRequest): Promise<OrderResponse> {
    const { data } = await httpClient.post<OrderResponse>(
      API_ROUTES.buyOrder,
      payload,
    );
    return data;
  },

  async placeSellOrder(payload: SellOrderAddRequest): Promise<OrderResponse> {
    const { data } = await httpClient.post<OrderResponse>(
      API_ROUTES.sellOrder,
      payload,
    );
    return data;
  },

  async getAllBuyOrders(): Promise<OrderResponse[]> {
    const { data } = await httpClient.get<OrderResponse[]>(
      API_ROUTES.allBuyOrders,
    );
    return data;
  },

  async getAllSellOrders(): Promise<OrderResponse[]> {
    const { data } = await httpClient.get<OrderResponse[]>(
      API_ROUTES.allSellOrders,
    );
    return data;
  },

  async getMarketPrices(symbols: string[]): Promise<MarketPrice[]> {
    const { data } = await httpClient.get<MarketPrice[]>(
      API_ROUTES.marketPrices,
      { params: symbols.length ? { symbols: symbols.join(",") } : undefined },
    );
    return data;
  },

  async getCandleHydration(
    symbol: string,
    interval: CandleInterval = "1m",
  ): Promise<CandleHydrationResponse> {
    const { data } = await httpClient.get<CandleHydrationResponse>(
      API_ROUTES.marketCandles(symbol),
      { params: { interval } },
    );
    return data;
  },
};
