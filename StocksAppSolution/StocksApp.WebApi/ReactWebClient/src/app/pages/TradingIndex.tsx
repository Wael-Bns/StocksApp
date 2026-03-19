import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router";
import { apiService } from "../services/apiService";
import { finnhubService } from "../services/finnhubService";
import { StockTrade, WebSocketMessage } from "../types/trading";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/card";
import { ArrowUp, ArrowDown, TrendingUp } from "lucide-react";
import { toast } from "sonner";
import { Toaster } from "../components/ui/sonner";
import { ApiInfoBanner } from "../components/ApiInfoBanner";
import { StockSymbolSelector } from "../components/StockSymbolSelector";

const DEFAULT_STOCK_SYMBOL = "MSFT";
const DEFAULT_ORDER_QUANTITY = 100;

export function TradingIndex() {
  const navigate = useNavigate();
  const [stockSymbol, setStockSymbol] = useState(DEFAULT_STOCK_SYMBOL);
  const [stockData, setStockData] = useState<StockTrade | null>(null);
  const [currentPrice, setCurrentPrice] = useState<number | null>(null);
  const [previousPrice, setPreviousPrice] = useState<number | null>(null);
  const [quantity, setQuantity] = useState<number>(DEFAULT_ORDER_QUANTITY);
  const [loading, setLoading] = useState(true);
  const wsRef = useRef<WebSocket | null>(null);

  useEffect(() => {
    // Cleanup previous WebSocket before loading new data
    if (wsRef.current) {
      wsRef.current.close();
      wsRef.current = null;
    }
    
    loadStockData();
    
    return () => {
      // Cleanup WebSocket on unmount
      if (wsRef.current) {
        wsRef.current.close();
      }
    };
  }, [stockSymbol]);

  async function loadStockData() {
    setLoading(true);
    try {
      // Fetch stock data from .NET API
      const tradeData = await apiService.getTradeInfo(stockSymbol);

      if (tradeData) {
        setStockData(tradeData);
        setCurrentPrice(tradeData.pricePerShare);
        setPreviousPrice(tradeData.pricePerShare);
      }

      // Connect to WebSocket for live updates (still using Finnhub for real-time prices)
      connectWebSocket();
    } catch (error) {
      console.error("Error loading stock data:", error);
      toast.error("Failed to load stock data");
    } finally {
      setLoading(false);
    }
  }

  function connectWebSocket() {
    const ws = new WebSocket(finnhubService.getWebSocketUrl());

    ws.addEventListener("open", () => {
      console.log("WebSocket connected");
      // Subscribe to stock symbol
      ws.send(
        JSON.stringify({ type: "subscribe", symbol: stockSymbol })
      );
    });

    ws.addEventListener("message", (event) => {
      try {
        const message: WebSocketMessage = JSON.parse(event.data);
        if (message.type === "trade" && message.data && message.data.length > 0) {
          // Get the highest price from the data array
          const prices = message.data.map((trade) => trade.p);
          const latestPrice = Math.max(...prices);
          setCurrentPrice(latestPrice);
        }
      } catch (error) {
        console.error("Error parsing WebSocket message:", error);
      }
    });

    ws.addEventListener("error", (error) => {
      console.error("WebSocket error:", error);
    });

    ws.addEventListener("close", () => {
      console.log("WebSocket disconnected");
    });

    wsRef.current = ws;
  }

  async function handleBuyOrder(e: React.FormEvent) {
    e.preventDefault();
    
    if (!stockData || !currentPrice) {
      toast.error("Stock data not available");
      return;
    }

    if (quantity < 1 || quantity > 100000) {
      toast.error("Quantity must be between 1 and 100,000");
      return;
    }

    if (currentPrice < 1 || currentPrice > 10000) {
      toast.error("Price must be between $1 and $10,000");
      return;
    }

    try {
      await apiService.createBuyOrder({
        stockSymbol: stockSymbol,
        stockName: stockData.stockName || stockSymbol,
        dateAndTimeOfOrder: new Date().toISOString(),
        quantity,
        price: currentPrice,
      });

      toast.success(`Buy order created for ${quantity} shares at $${currentPrice.toFixed(2)}`);
      navigate("/orders");
    } catch (error) {
      toast.error("Failed to create buy order");
    }
  }

  async function handleSellOrder(e: React.FormEvent) {
    e.preventDefault();
    
    if (!stockData || !currentPrice) {
      toast.error("Stock data not available");
      return;
    }

    if (quantity < 1 || quantity > 100000) {
      toast.error("Quantity must be between 1 and 100,000");
      return;
    }

    if (currentPrice < 1 || currentPrice > 10000) {
      toast.error("Price must be between $1 and $10,000");
      return;
    }

    try {
      await apiService.createSellOrder({
        stockSymbol: stockSymbol,
        stockName: stockData.stockName || stockSymbol,
        dateAndTimeOfOrder: new Date().toISOString(),
        quantity,
        price: currentPrice,
      });

      toast.success(`Sell order created for ${quantity} shares at $${currentPrice.toFixed(2)}`);
      navigate("/orders");
    } catch (error) {
      toast.error("Failed to create sell order");
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <TrendingUp className="w-12 h-12 animate-pulse mx-auto mb-4 text-blue-600" />
          <p className="text-gray-600">Loading stock data...</p>
        </div>
      </div>
    );
  }

  const priceChange = currentPrice && previousPrice ? currentPrice - previousPrice : 0;
  const priceChangePercent = currentPrice && previousPrice ? ((currentPrice - previousPrice) / previousPrice) * 100 : 0;
  const isPositive = priceChange >= 0;

  return (
    <div className="container mx-auto px-4 py-8 max-w-6xl">
      <Toaster />
      
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-4xl font-bold mb-2">Stock Trading Platform</h1>
          <p className="text-gray-600">Real-time stock prices via .NET API</p>
        </div>
        <StockSymbolSelector 
          currentSymbol={stockSymbol}
          onSymbolChange={setStockSymbol}
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
        {/* Company Info */}
        {stockData && (
          <Card className="lg:col-span-2">
            <CardHeader>
              <div className="flex items-start justify-between">
                <div className="flex items-center gap-4">
                  {stockData.logo && (
                    <img src={stockData.logo} alt={stockData.stockName} className="w-16 h-16 rounded" />
                  )}
                  <div>
                    <CardTitle className="text-2xl">{stockData.stockName || stockSymbol}</CardTitle>
                    <p className="text-gray-500 text-sm">{stockData.stockSymbol || stockSymbol}</p>
                  </div>
                </div>
                <div className="text-right">
                  <div className="text-3xl font-bold">
                    ${currentPrice?.toFixed(2)}
                  </div>
                  <div className={`flex items-center gap-1 justify-end ${isPositive ? 'text-green-600' : 'text-red-600'}`}>
                    {isPositive ? <ArrowUp className="w-4 h-4" /> : <ArrowDown className="w-4 h-4" />}
                    <span>{isPositive ? '+' : ''}{priceChange.toFixed(2)} ({priceChangePercent.toFixed(2)}%)</span>
                  </div>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-sm text-gray-600">
                <p>Real-time price updates via WebSocket</p>
                <p className="mt-2">Stock data provided by .NET API</p>
              </div>
            </CardContent>
          </Card>
        )}

        {/* Trading Form */}
        <Card>
          <CardHeader>
            <CardTitle>Place Order</CardTitle>
          </CardHeader>
          <CardContent>
            <form className="space-y-4">
              <div>
                <Label htmlFor="quantity">Quantity</Label>
                <Input
                  id="quantity"
                  type="number"
                  min="1"
                  max="100000"
                  value={quantity}
                  onChange={(e) => setQuantity(Number(e.target.value))}
                  className="mt-1"
                />
                <p className="text-xs text-gray-500 mt-1">Min: 1, Max: 100,000</p>
              </div>

              <div>
                <Label>Current Price</Label>
                <div className="mt-1 p-3 bg-gray-50 rounded-md font-semibold">
                  ${currentPrice?.toFixed(2)}
                </div>
              </div>

              <div>
                <Label>Total Amount</Label>
                <div className="mt-1 p-3 bg-blue-50 rounded-md font-bold text-blue-900">
                  ${((currentPrice || 0) * quantity).toFixed(2)}
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3 pt-4">
                <Button
                  type="button"
                  onClick={handleBuyOrder}
                  className="w-full bg-green-600 hover:bg-green-700"
                >
                  Buy
                </Button>
                <Button
                  type="button"
                  onClick={handleSellOrder}
                  variant="destructive"
                  className="w-full"
                >
                  Sell
                </Button>
              </div>
            </form>

            <div className="mt-6 pt-6 border-t">
              <Button
                variant="outline"
                onClick={() => navigate("/orders")}
                className="w-full"
              >
                View Orders
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>

      <ApiInfoBanner />
    </div>
  );
}