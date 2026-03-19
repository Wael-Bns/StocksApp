// API Configuration
// Update this URL to match your .NET API endpoint
// You can also set VITE_API_BASE_URL environment variable
export const API_CONFIG = {
  // Default: https://localhost:7001
  // For production, replace with your deployed API URL
  // Or set VITE_API_BASE_URL environment variable
  baseUrl: import.meta.env.VITE_API_BASE_URL || "https://localhost:7001",
  
  // Enable CORS if needed
  corsEnabled: true,
};

// API Endpoints
export const API_ENDPOINTS = {
  searchStocks: (query: string) => 
    `${API_CONFIG.baseUrl}/api/Trade/SearchStocks?query=${encodeURIComponent(query)}`,
  
  getTradeInfo: (stockSymbol?: string) => 
    stockSymbol 
      ? `${API_CONFIG.baseUrl}/api/Trade/GetTradeInfo/${encodeURIComponent(stockSymbol)}`
      : `${API_CONFIG.baseUrl}/api/Trade/GetTradeInfo`,
  
  buyOrder: `${API_CONFIG.baseUrl}/api/Trade/BuyOrder`,
  
  sellOrder: `${API_CONFIG.baseUrl}/api/Trade/SellOrder`,
  
  getAllBuyOrders: `${API_CONFIG.baseUrl}/api/Trade/GetAllBuyOrders`,
  
  getAllSellOrders: `${API_CONFIG.baseUrl}/api/Trade/GetAllSellOrders`,
};