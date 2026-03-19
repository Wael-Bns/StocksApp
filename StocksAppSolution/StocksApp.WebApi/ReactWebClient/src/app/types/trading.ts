// Order types based on the assignment requirements

export interface BuyOrder {
  buyOrderID: string;
  stockSymbol: string;
  stockName: string;
  dateAndTimeOfOrder: Date;
  quantity: number;
  price: number;
  tradeAmount: number;
}

export interface SellOrder {
  sellOrderID: string;
  stockSymbol: string;
  stockName: string;
  dateAndTimeOfOrder: Date;
  quantity: number;
  price: number;
  tradeAmount: number;
}

// Request types for .NET API
export interface BuyOrderRequest {
  stockSymbol: string;
  stockName: string;
  dateAndTimeOfOrder: string; // ISO 8601 format for API
  quantity: number;
  price: number;
}

export interface SellOrderRequest {
  stockSymbol: string;
  stockName: string;
  dateAndTimeOfOrder: string; // ISO 8601 format for API
  quantity: number;
  price: number;
}

// Stock trade information from .NET API
export interface StockTrade {
  stockSymbol?: string;
  stockName?: string;
  pricePerShare: number;
  logo?: string;
}

// Stock search result
export interface StockSearchResult {
  count: number;
  result: {
    description: string;
    displaySymbol: string;
    symbol: string;
    type: string;
  }[];
}

export interface StockProfile {
  country: string;
  currency: string;
  exchange: string;
  finnhubIndustry: string;
  ipo: string;
  logo: string;
  marketCapitalization: number;
  name: string;
  phone: string;
  shareOutstanding: number;
  ticker: string;
  weburl: string;
}

export interface StockQuote {
  c: number; // Current price
  d: number; // Change
  dp: number; // Percent change
  h: number; // High price of the day
  l: number; // Low price of the day
  o: number; // Open price of the day
  pc: number; // Previous close price
  t: number; // Timestamp
}

export interface WebSocketTrade {
  p: number; // Last price
  s: string; // Symbol
  t: number; // Timestamp
  v: number; // Volume
}

export interface WebSocketMessage {
  data: WebSocketTrade[];
  type: string;
}