import { useState } from "react";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Search } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "./ui/dialog";

interface StockSymbolSelectorProps {
  currentSymbol: string;
  onSymbolChange: (symbol: string) => void;
}

const POPULAR_STOCKS = [
  { symbol: "MSFT", name: "Microsoft" },
  { symbol: "AAPL", name: "Apple" },
  { symbol: "GOOGL", name: "Alphabet (Google)" },
  { symbol: "AMZN", name: "Amazon" },
  { symbol: "TSLA", name: "Tesla" },
  { symbol: "META", name: "Meta (Facebook)" },
  { symbol: "NVDA", name: "NVIDIA" },
  { symbol: "NFLX", name: "Netflix" },
];

export function StockSymbolSelector({ currentSymbol, onSymbolChange }: StockSymbolSelectorProps) {
  const [customSymbol, setCustomSymbol] = useState("");
  const [open, setOpen] = useState(false);

  function handleSelectStock(symbol: string) {
    onSymbolChange(symbol);
    setOpen(false);
  }

  function handleCustomSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (customSymbol.trim()) {
      onSymbolChange(customSymbol.trim().toUpperCase());
      setCustomSymbol("");
      setOpen(false);
    }
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="sm">
          <Search className="w-4 h-4 mr-2" />
          Change Stock
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Select Stock Symbol</DialogTitle>
          <DialogDescription>
            Choose from popular stocks or enter a custom symbol
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4">
          <div>
            <Label className="text-sm text-gray-600 mb-2 block">Popular Stocks</Label>
            <div className="grid grid-cols-2 gap-2">
              {POPULAR_STOCKS.map((stock) => (
                <Button
                  key={stock.symbol}
                  variant={currentSymbol === stock.symbol ? "default" : "outline"}
                  onClick={() => handleSelectStock(stock.symbol)}
                  className="justify-start"
                >
                  <div className="text-left">
                    <div className="font-semibold">{stock.symbol}</div>
                    <div className="text-xs opacity-70">{stock.name}</div>
                  </div>
                </Button>
              ))}
            </div>
          </div>

          <div className="pt-4 border-t">
            <Label htmlFor="custom-symbol" className="text-sm text-gray-600 mb-2 block">
              Or Enter Custom Symbol
            </Label>
            <form onSubmit={handleCustomSubmit} className="flex gap-2">
              <Input
                id="custom-symbol"
                placeholder="e.g., TSLA"
                value={customSymbol}
                onChange={(e) => setCustomSymbol(e.target.value)}
                className="uppercase"
              />
              <Button type="submit">Go</Button>
            </form>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
