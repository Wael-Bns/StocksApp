export interface MarketSymbol {
  symbol: string;
  name: string;
}

/**
 * The API doesn't expose a "top symbols" or watchlist endpoint — trade-info
 * is looked up per symbol and price streams are joined per symbol group on
 * stocksHub. This is a fixed reference list of well-known large-cap tickers
 * used to seed the watchlist; each row subscribes to its own live price via
 * SignalR (real data), the list of symbols itself just isn't API-driven.
 */
export const TOP_50_SYMBOLS: MarketSymbol[] = [
  { symbol: "AAPL", name: "Apple Inc." },
  { symbol: "MSFT", name: "Microsoft Corp." },
  { symbol: "GOOGL", name: "Alphabet Inc." },
  { symbol: "AMZN", name: "Amazon.com Inc." },
  { symbol: "NVDA", name: "NVIDIA Corp." },
  { symbol: "META", name: "Meta Platforms Inc." },
  { symbol: "TSLA", name: "Tesla Inc." },
  { symbol: "BRK.B", name: "Berkshire Hathaway" },
  { symbol: "JPM", name: "JPMorgan Chase & Co." },
  { symbol: "V", name: "Visa Inc." },
  { symbol: "UNH", name: "UnitedHealth Group" },
  { symbol: "XOM", name: "Exxon Mobil Corp." },
  { symbol: "JNJ", name: "Johnson & Johnson" },
  { symbol: "WMT", name: "Walmart Inc." },
  { symbol: "MA", name: "Mastercard Inc." },
  { symbol: "PG", name: "Procter & Gamble" },
  { symbol: "HD", name: "Home Depot Inc." },
  { symbol: "CVX", name: "Chevron Corp." },
  { symbol: "MRK", name: "Merck & Co." },
  { symbol: "ABBV", name: "AbbVie Inc." },
  { symbol: "KO", name: "Coca-Cola Co." },
  { symbol: "PEP", name: "PepsiCo Inc." },
  { symbol: "COST", name: "Costco Wholesale" },
  { symbol: "AVGO", name: "Broadcom Inc." },
  { symbol: "ORCL", name: "Oracle Corp." },
  { symbol: "ADBE", name: "Adobe Inc." },
  { symbol: "CSCO", name: "Cisco Systems" },
  { symbol: "MCD", name: "McDonald's Corp." },
  { symbol: "CRM", name: "Salesforce Inc." },
  { symbol: "BAC", name: "Bank of America" },
  { symbol: "PFE", name: "Pfizer Inc." },
  { symbol: "TMO", name: "Thermo Fisher Scientific" },
  { symbol: "ACN", name: "Accenture plc" },
  { symbol: "ABT", name: "Abbott Laboratories" },
  { symbol: "LIN", name: "Linde plc" },
  { symbol: "NFLX", name: "Netflix Inc." },
  { symbol: "DIS", name: "Walt Disney Co." },
  { symbol: "WFC", name: "Wells Fargo & Co." },
  { symbol: "DHR", name: "Danaher Corp." },
  { symbol: "TXN", name: "Texas Instruments" },
  { symbol: "VZ", name: "Verizon Communications" },
  { symbol: "NKE", name: "Nike Inc." },
  { symbol: "PM", name: "Philip Morris Intl." },
  { symbol: "INTC", name: "Intel Corp." },
  { symbol: "INTU", name: "Intuit Inc." },
  { symbol: "CMCSA", name: "Comcast Corp." },
  { symbol: "UNP", name: "Union Pacific Corp." },
  { symbol: "HON", name: "Honeywell Intl." },
  { symbol: "IBM", name: "IBM Corp." },
  { symbol: "AMD", name: "Advanced Micro Devices" },
];
