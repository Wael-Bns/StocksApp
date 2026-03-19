# Quick Start Guide - .NET API Integration

## Prerequisites

- .NET API running (your StocksApp.WebApi project)
- Node.js installed
- Frontend dependencies installed (`npm install`)

## Step 1: Configure API URL

**Option A: Using Environment Variables (Recommended)**

1. Copy `.env.example` to `.env`:
   ```bash
   cp .env.example .env
   ```

2. Edit `.env` and update the API URL:
   ```
   VITE_API_BASE_URL=https://localhost:7001
   ```

**Option B: Direct Configuration**

Open `/src/app/config/api.ts` and update the base URL:

```typescript
export const API_CONFIG = {
  baseUrl: "https://localhost:7001", // Your .NET API URL
  corsEnabled: true,
};
```

> **Note**: Environment variables take precedence over the hardcoded value.

## Step 2: Enable CORS in .NET API

In your .NET API `Program.cs`, add CORS configuration:

```csharp
// Add this before builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Add this after app is built (before UseAuthorization)
app.UseCors("AllowFrontend");
```

## Step 3: Start Your Applications

### Terminal 1 - .NET API
```bash
cd path/to/StocksApp.WebApi
dotnet run
```

The API should start on `https://localhost:7001` (or your configured port)

### Terminal 2 - React Frontend
```bash
npm run dev
```

The frontend should start on `http://localhost:5173`

## Step 4: Test the Application

1. Open browser to `http://localhost:5173`
2. You should see the trading dashboard with Microsoft (MSFT) stock loaded
3. Try these actions:
   - Click "Change Stock" to select a different stock
   - Enter a quantity and click "Buy" or "Sell"
   - Click "View Orders" to see your orders
   - Click "Export to PDF" to generate a PDF report

## Troubleshooting

### Issue: "Failed to fetch trade info"
- ✅ Check that your .NET API is running
- ✅ Verify the API URL in `/src/app/config/api.ts`
- ✅ Check browser console for CORS errors

### Issue: CORS errors
- ✅ Ensure CORS is enabled in your .NET API
- ✅ Make sure the frontend origin matches the CORS policy
- ✅ Check if you need to allow credentials

### Issue: Orders not saving
- ✅ Check that your .NET API database is configured
- ✅ Verify the StockService is registered in dependency injection
- ✅ Check .NET API logs for errors

### Issue: WebSocket not connecting
- ✅ This is expected if not during market hours (9:30 AM - 4:00 PM ET)
- ✅ Stock data will still load, just without live updates
- ✅ Check browser console for WebSocket errors

## API Endpoints Being Called

When you use the app, these endpoints will be called:

1. **On Load**: `GET /api/Trade/GetTradeInfo/MSFT`
2. **On Buy**: `POST /api/Trade/BuyOrder`
3. **On Sell**: `POST /api/Trade/SellOrder`
4. **Orders Page**: `GET /api/Trade/GetAllBuyOrders` + `GET /api/Trade/GetAllSellOrders`

## Development Tips

### Check API Responses
Open browser DevTools → Network tab to see all API calls and responses.

### Enable Detailed Errors
In `.NET API`, set `builder.Services.AddControllers()` to include detailed errors during development.

### Hot Reload
Both applications support hot reload:
- React: Changes to `.tsx` files reload automatically
- .NET: Changes to `.cs` files rebuild automatically (if using `dotnet watch`)

## Production Deployment

When deploying to production:

1. Update `/src/app/config/api.ts` with production API URL
2. Update CORS policy in .NET API to allow production frontend URL
3. Use HTTPS for both frontend and backend
4. Consider adding authentication/authorization
5. Enable API rate limiting and request validation
6. Configure proper logging and monitoring

## Support

For issues specific to:
- **Frontend**: Check `/MIGRATION_SUMMARY.md` and `/API_INTEGRATION.md`
- **.NET API**: Check your .NET project documentation
- **Finnhub API**: Visit https://finnhub.io/docs/api

---

**Happy Trading! 📈**