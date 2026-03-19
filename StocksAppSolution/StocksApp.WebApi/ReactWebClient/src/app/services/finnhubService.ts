import { StockProfile, StockQuote } from "../types/trading";

// Use the provided API token or replace with your own from https://finnhub.io/dashboard
const FINNHUB_TOKEN = "cc676uaad3i9rj8tb1s0";

export const finnhubService = {
  async getCompanyProfile(stockSymbol: string): Promise<StockProfile | null> {
    try {
      const response = await fetch(
        `https://finnhub.io/api/v1/stock/profile2?symbol=${stockSymbol}&token=${FINNHUB_TOKEN}`
      );
      if (!response.ok) {
        throw new Error("Failed to fetch company profile");
      }
      const data = await response.json();
      return data;
    } catch (error) {
      console.error("Error fetching company profile:", error);
      return null;
    }
  },

  async getStockPriceQuote(stockSymbol: string): Promise<StockQuote | null> {
    try {
      const response = await fetch(
        `https://finnhub.io/api/v1/quote?symbol=${stockSymbol}&token=${FINNHUB_TOKEN}`
      );
      if (!response.ok) {
        throw new Error("Failed to fetch stock quote");
      }
      const data = await response.json();
      return data;
    } catch (error) {
      console.error("Error fetching stock quote:", error);
      return null;
    }
  },

  getWebSocketUrl(): string {
    return `wss://ws.finnhub.io?token=${FINNHUB_TOKEN}`;
  },
};
