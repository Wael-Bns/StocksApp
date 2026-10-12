import type { MarketCandle, OrderResponse } from "../types/trade.types";

export const PREVIEW_MARKETS = [
  { symbol: "AAPL", name: "Apple Inc.", price: 192.42, change: 0.65 },
  { symbol: "TSLA", name: "Tesla, Inc.", price: 175.22, change: -2.14 },
  { symbol: "NVDA", name: "NVIDIA Corp.", price: 875.28, change: 3.42 },
  { symbol: "MSFT", name: "Microsoft", price: 412.11, change: 0.12 },
  { symbol: "AMD", name: "AMD", price: 183.15, change: -1.05 },
  { symbol: "AMZN", name: "Amazon", price: 178.22, change: 0.88 },
  { symbol: "GOOGL", name: "Alphabet", price: 154.52, change: -0.33 },
  { symbol: "META", name: "Meta Platforms", price: 502.3, change: 1.15 },
] as const;

const seedPrices = [
  191.62, 191.78, 191.55, 191.91, 191.72, 191.44, 191.2, 191.05,
  191.32, 191.46, 191.28, 191.62, 191.48, 191.72, 191.54, 191.83,
  192.05, 191.94, 192.24, 192.16, 192.35, 192.18, 192.52, 192.43,
  192.72, 192.58, 192.88, 192.76, 193.04, 192.91, 193.18, 193.05,
  193.38, 193.2, 193.57, 193.82, 193.65, 193.96, 194.12, 193.82,
  193.54, 193.02, 193.28, 192.84, 192.62, 192.9, 192.58, 192.42,
];

export const PREVIEW_CANDLES: MarketCandle[] = seedPrices.map((close, index) => {
  const open = seedPrices[index - 1] ?? close - 0.12;
  const spread = 0.08 + (index % 4) * 0.025;
  return {
    time: Date.now() - (seedPrices.length - index) * 60_000,
    open,
    high: Math.max(open, close) + spread,
    low: Math.min(open, close) - spread,
    close,
    volume: 12000 + ((index * 791) % 38000),
  };
});

export const PREVIEW_ORDERS: OrderResponse[] = [
  { dateAndTimeOfOrder: "14:22:10", stockSymbol: "AAPL", stockName: "Apple Inc.", side: "BUY", type: "MARKET", price: 192.42, quantity: 10, status: "FILLED" },
  { dateAndTimeOfOrder: "14:15:05", stockSymbol: "NVDA", stockName: "NVIDIA Corp.", side: "SELL", type: "LIMIT", price: 876.5, quantity: 5, status: "PENDING" },
  { dateAndTimeOfOrder: "13:58:32", stockSymbol: "TSLA", stockName: "Tesla, Inc.", side: "BUY", type: "MARKET", price: 174.9, quantity: 20, status: "FILLED" },
];
