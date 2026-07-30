import { useState } from "react";
import { TopBar } from "../components/layout/TopBar";
import { Watchlist } from "../components/trade/Watchlist";
import { MarketSearch } from "../components/trade/MarketSearch";
import { MarketPanel } from "../components/trade/MarketPanel";
import { TradePanel } from "../components/trade/TradePanel";
import { ActivityTable } from "../components/trade/ActivityTable";
import { TOP_50_SYMBOLS, type MarketSymbol } from "../config/topSymbols";
import type { TradeInfoResponse } from "../types/trade.types";

function extractStockName(info: TradeInfoResponse | null): string | null {
  if (!info) return null;
  const candidate = info.stockName ?? info.name ?? info.companyName;
  return typeof candidate === "string" ? candidate : null;
}

export function MarketsPage() {
  const [activeSymbol, setActiveSymbol] = useState<MarketSymbol>(TOP_50_SYMBOLS[0]);
  const [tradeInfo, setTradeInfo] = useState<TradeInfoResponse | null>(null);
  const [activityRefreshKey, setActivityRefreshKey] = useState(0);

  const activeStockName = extractStockName(tradeInfo) ?? activeSymbol.name;

  function handleResolved(symbol: string, info: TradeInfoResponse) {
    const known = TOP_50_SYMBOLS.find((item) => item.symbol === symbol);
    setActiveSymbol(known ?? { symbol, name: symbol });
    setTradeInfo(info);
  }

  function handleSelectFromWatchlist(item: MarketSymbol) {
    setActiveSymbol(item);
    setTradeInfo(null);
  }

  return (
    <div className="flex flex-col min-h-screen">
      <TopBar
        title="Markets"
        subtitle="Connect to a symbol's live price stream and trade it."
        actions={<MarketSearch onResolved={handleResolved} />}
      />

      <div className="flex-1 grid grid-cols-1 lg:grid-cols-[280px_1fr_340px] gap-lg p-lg">
        <Watchlist activeSymbol={activeSymbol.symbol} onSelect={handleSelectFromWatchlist} />

        <div className="flex flex-col gap-lg min-w-0">
          <MarketPanel symbol={activeSymbol.symbol} stockName={activeStockName} />
          <ActivityTable symbol={activeSymbol.symbol} refreshKey={activityRefreshKey} />
        </div>

        <TradePanel
          symbol={activeSymbol.symbol}
          stockName={activeStockName}
          onOrderPlaced={() => setActivityRefreshKey((k) => k + 1)}
        />
      </div>
    </div>
  );
}
