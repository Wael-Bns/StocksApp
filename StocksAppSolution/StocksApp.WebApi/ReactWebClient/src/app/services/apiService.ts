import { BuyOrder, SellOrder, StockTrade, BuyOrderRequest, SellOrderRequest, StockSearchResult } from "../types/trading";
import { API_ENDPOINTS } from "../config/api";

export const apiService = {
  async searchStocks(query: string): Promise<StockSearchResult | null> {
    try {
      const response = await fetch(API_ENDPOINTS.searchStocks(query));
      if (!response.ok) {
        throw new Error("Failed to search stocks");
      }
      const data = await response.json();
      return data;
    } catch (error) {
      console.error("Error searching stocks:", error);
      return null;
    }
  },

  async getTradeInfo(stockSymbol?: string): Promise<StockTrade | null> {
    try {
      const response = await fetch(API_ENDPOINTS.getTradeInfo(stockSymbol));
      if (!response.ok) {
        throw new Error("Failed to fetch trade info");
      }
      const data = await response.json();
      return data;
    } catch (error) {
      console.error("Error fetching trade info:", error);
      return null;
    }
  },

  async createBuyOrder(buyOrderRequest: BuyOrderRequest): Promise<BuyOrder | null> {
    try {
      const response = await fetch(
        API_ENDPOINTS.buyOrder,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(buyOrderRequest),
        }
      );
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Failed to create buy order: ${errorText}`);
      }
      const data = await response.json();
      return data;
    } catch (error) {
      console.error("Error creating buy order:", error);
      throw error;
    }
  },

  async createSellOrder(sellOrderRequest: SellOrderRequest): Promise<SellOrder | null> {
    try {
      const response = await fetch(
        API_ENDPOINTS.sellOrder,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(sellOrderRequest),
        }
      );
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Failed to create sell order: ${errorText}`);
      }
      const data = await response.json();
      return data;
    } catch (error) {
      console.error("Error creating sell order:", error);
      throw error;
    }
  },

  async getAllBuyOrders(): Promise<BuyOrder[]> {
    try {
      const response = await fetch(API_ENDPOINTS.getAllBuyOrders);
      if (!response.ok) {
        throw new Error("Failed to fetch buy orders");
      }
      const data = await response.json();
      return data;
    } catch (error) {
      console.error("Error fetching buy orders:", error);
      return [];
    }
  },

  async getAllSellOrders(): Promise<SellOrder[]> {
    try {
      const response = await fetch(API_ENDPOINTS.getAllSellOrders);
      if (!response.ok) {
        throw new Error("Failed to fetch sell orders");
      }
      const data = await response.json();
      return data;
    } catch (error) {
      console.error("Error fetching sell orders:", error);
      return [];
    }
  },
};