# .NET API Integration Guide

This React frontend has been configured to work with your .NET API backend.

## Configuration

### API Base URL

Update the API base URL in `/src/app/config/api.ts`:

```typescript
export const API_CONFIG = {
  baseUrl: "https://localhost:7001", // Update this to your API URL
  corsEnabled: true,
};
```

For production, replace with your deployed API URL (e.g., `https://api.yourapp.com`).

## API Endpoints

The frontend calls the following .NET API endpoints:

### 1. Get Trade Info
- **Endpoint**: `GET /api/Trade/GetTradeInfo/{stockSymbol?}`
- **Description**: Fetches stock information including company profile and current price
- **Response**: `StockTrade` object
```json
{
  "stockSymbol": "MSFT",
  "stockName": "Microsoft Corporation",
  "pricePerShare": 415.50,
  "logo": "https://..."
}
```

### 2. Search Stocks
- **Endpoint**: `GET /api/Trade/SearchStocks?query={query}`
- **Description**: Searches for stocks by symbol or company name
- **Response**: `StockSearchResult` object

### 3. Create Buy Order
- **Endpoint**: `POST /api/Trade/BuyOrder`
- **Description**: Creates a new buy order
- **Request Body**:
```json
{
  "stockSymbol": "MSFT",
  "stockName": "Microsoft Corporation",
  "dateAndTimeOfOrder": "2026-03-19T10:30:00Z",
  "quantity": 100,
  "price": 415.50
}
```
- **Response**: `BuyOrderResponse` object with `buyOrderID` and `tradeAmount`

### 4. Create Sell Order
- **Endpoint**: `POST /api/Trade/SellOrder`
- **Description**: Creates a new sell order
- **Request Body**:
```json
{
  "stockSymbol": "MSFT",
  "stockName": "Microsoft Corporation",
  "dateAndTimeOfOrder": "2026-03-19T10:30:00Z",
  "quantity": 100,
  "price": 415.50
}
```
- **Response**: `SellOrderResponse` object with `sellOrderID` and `tradeAmount`

### 5. Get All Buy Orders
- **Endpoint**: `GET /api/Trade/GetAllBuyOrders`
- **Description**: Retrieves all buy orders
- **Response**: Array of `BuyOrderResponse` objects

### 6. Get All Sell Orders
- **Endpoint**: `GET /api/Trade/GetAllSellOrders`
- **Description**: Retrieves all sell orders
- **Response**: Array of `SellOrderResponse` objects

## CORS Configuration

Your .NET API must allow CORS requests from the frontend origin. Add the following to your `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// After building the app
app.UseCors("AllowFrontend");
```

For production, update the origin to your deployed frontend URL.

## WebSocket Integration

The frontend still uses Finnhub's WebSocket for real-time price updates. This provides live stock price changes during market hours.

If you want to handle WebSocket connections through your .NET API, you would need to:
1. Add SignalR or WebSocket support to your .NET API
2. Create a hub that forwards Finnhub WebSocket data to connected clients
3. Update the frontend to connect to your .NET WebSocket endpoint instead

## Testing

1. Ensure your .NET API is running
2. Update the API base URL in `/src/app/config/api.ts`
3. Start the React frontend
4. Test the following flows:
   - Load stock data for different symbols
   - Create buy/sell orders
   - View orders page
   - Export orders to PDF

## Error Handling

The frontend includes error handling for all API calls:
- Network errors are logged to the console
- Failed operations show toast notifications to users
- Empty states are displayed when no data is available

## TypeScript Types

All API request/response types are defined in `/src/app/types/trading.ts` to ensure type safety.
