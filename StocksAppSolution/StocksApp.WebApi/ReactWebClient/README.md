# Stock Trading Platform - React Frontend

A modern stock trading application built with React, TypeScript, and Tailwind CSS, integrated with a .NET API backend.

## 🚀 Features

- **Real-time Stock Data**: Live stock prices via WebSocket integration
- **Trading Operations**: Create buy and sell orders with validation
- **Order Management**: View all trading history with detailed information
- **PDF Export**: Generate printable reports of trading activity
- **Stock Search**: Search and switch between different stock symbols
- **Responsive Design**: Works seamlessly on desktop and mobile devices
- **.NET API Integration**: All operations backed by secure server-side API

## 📋 Prerequisites

- Node.js 18+ installed
- .NET 8+ API backend running (StocksApp.WebApi)
- Modern web browser with WebSocket support

## 🛠️ Installation

1. **Clone the repository** (if not already done)

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Configure API endpoint**:
   
   Copy the environment file:
   ```bash
   cp .env.example .env
   ```
   
   Edit `.env` and set your .NET API URL:
   ```
   VITE_API_BASE_URL=https://localhost:7001
   ```

4. **Start the development server**:
   ```bash
   npm run dev
   ```

5. **Open your browser** to `http://localhost:5173`

## 🏗️ Architecture

### Frontend Stack
- **React 18** - UI framework
- **TypeScript** - Type safety
- **Tailwind CSS v4** - Styling
- **React Router** - Client-side routing
- **Radix UI** - Accessible component primitives
- **Vite** - Build tool and dev server

### Backend Integration
The frontend communicates with a .NET Web API that handles:
- Stock data fetching from Finnhub API
- Order creation and storage
- Database persistence
- Server-side validation

## 📁 Project Structure

```
├── src/
│   ├── app/
│   │   ├── components/       # Reusable React components
│   │   │   ├── ui/          # shadcn/ui components
│   │   │   ├── ApiInfoBanner.tsx
│   │   │   ├── Root.tsx
│   │   │   └── StockSymbolSelector.tsx
│   │   ├── config/          # Configuration files
│   │   │   └── api.ts       # API endpoint configuration
│   │   ├── hooks/           # Custom React hooks
│   │   ├── pages/           # Page components
│   │   │   ├── TradingIndex.tsx   # Main trading dashboard
│   │   │   ├── Orders.tsx         # Orders history page
│   │   │   └── OrdersPDF.tsx      # PDF export view
│   │   ├── services/        # API service layer
│   │   │   ├── apiService.ts      # .NET API integration
│   │   │   └── finnhubService.ts  # WebSocket integration
│   │   ├── types/           # TypeScript type definitions
│   │   │   └── trading.ts
│   │   ├── App.tsx          # Main app component
│   │   └── routes.tsx       # Route configuration
│   └── styles/              # Global styles
├── .env.example             # Environment variables template
├── API_INTEGRATION.md       # API integration guide
├── MIGRATION_SUMMARY.md     # Migration details
├── QUICKSTART.md           # Quick start guide
└── package.json
```

## 🔌 API Integration

### Endpoints

| Operation | Endpoint | Method |
|-----------|----------|--------|
| Get stock info | `/api/Trade/GetTradeInfo/{symbol}` | GET |
| Search stocks | `/api/Trade/SearchStocks?query={query}` | GET |
| Create buy order | `/api/Trade/BuyOrder` | POST |
| Create sell order | `/api/Trade/SellOrder` | POST |
| Get buy orders | `/api/Trade/GetAllBuyOrders` | GET |
| Get sell orders | `/api/Trade/GetAllSellOrders` | GET |

### Configuration

API configuration is centralized in `/src/app/config/api.ts`:

```typescript
export const API_CONFIG = {
  baseUrl: import.meta.env.VITE_API_BASE_URL || "https://localhost:7001",
  corsEnabled: true,
};
```

## 🔒 CORS Setup (Required)

Your .NET API must allow CORS from the frontend origin. Add to `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// After building the app
app.UseCors("AllowFrontend");
```

## 🎨 Features in Detail

### Trading Dashboard
- View real-time stock information
- See live price updates during market hours
- Place buy/sell orders with validation
- Quantity validation: 1 - 100,000 shares
- Price validation: $1 - $10,000 per share

### Orders Page
- View all buy and sell orders
- Summary statistics (total orders, amounts)
- Sortable and filterable order lists
- Order details including ID, symbol, quantity, price, total

### PDF Export
- Generate printable order reports
- Professional formatting
- Summary totals
- Browser print dialog integration

## 🔧 Development

### Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production

### Environment Variables

Create a `.env` file (copy from `.env.example`):

```
VITE_API_BASE_URL=https://localhost:7001
```

### Hot Reload

The development server supports hot module replacement (HMR). Changes to `.tsx`, `.ts`, and `.css` files will automatically reload in the browser.

## 🚀 Production Build

1. Update `.env` with production API URL
2. Build the application:
   ```bash
   npm run build
   ```
3. The build output will be in the `dist/` directory
4. Deploy to your hosting service (Netlify, Vercel, etc.)

## 🧪 Testing

Before deploying, test these critical paths:

1. ✅ Load stock data for default symbol (MSFT)
2. ✅ Switch to different stock symbols
3. ✅ Create buy orders
4. ✅ Create sell orders
5. ✅ View orders page
6. ✅ Export to PDF
7. ✅ WebSocket price updates (during market hours)

## 📚 Documentation

- **[QUICKSTART.md](/QUICKSTART.md)** - Quick start guide
- **[API_INTEGRATION.md](/API_INTEGRATION.md)** - Detailed API integration
- **[MIGRATION_SUMMARY.md](/MIGRATION_SUMMARY.md)** - Migration from client-side to .NET API

## 🐛 Troubleshooting

### Common Issues

**Issue**: "Failed to fetch trade info"
- Check that .NET API is running
- Verify API URL in `.env` or `api.ts`
- Check browser console for errors

**Issue**: CORS errors
- Ensure CORS is configured in .NET API
- Verify frontend origin is allowed
- Check for credential requirements

**Issue**: Orders not saving
- Verify .NET API database is configured
- Check .NET API logs for errors
- Ensure services are registered in DI

**Issue**: WebSocket not connecting
- Normal outside market hours (9:30 AM - 4:00 PM ET)
- Stock data still loads, just no live updates
- Check Finnhub API status

## 🤝 Contributing

This is a frontend integration for a .NET API backend. To contribute:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test with the .NET API
5. Submit a pull request

## 📄 License

This project is for educational/demonstration purposes.

## 🆘 Support

For issues:
- **Frontend issues**: Check this README and documentation files
- **.NET API issues**: Refer to StocksApp.WebApi documentation
- **Finnhub API**: Visit https://finnhub.io/docs/api

---

Built with ❤️ using React, TypeScript, and Tailwind CSS
