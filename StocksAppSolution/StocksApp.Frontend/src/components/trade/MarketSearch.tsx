import { useState, type FormEvent } from "react";
import { AxiosError } from "axios";
import { tradeService } from "../../services/tradeService";
import { useToast } from "../../hooks/useToast";
import type { TradeInfoResponse } from "../../types/trade.types";

interface MarketSearchProps {
  onResolved: (symbol: string, info: TradeInfoResponse) => void;
}

/** Quick lookup for a symbol outside the watchlist's top-50 reference list,
 * via GET /trade-info/{symbol}. Selecting it sets it as the active symbol. */
export function MarketSearch({ onResolved }: MarketSearchProps) {
  const { showToast } = useToast();
  const [query, setQuery] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    const symbol = query.trim().toUpperCase();
    if (!symbol) {
      showToast("ENTER A STOCK SYMBOL", "warning");
      return;
    }

    setIsLoading(true);
    try {
      const info = await tradeService.getTradeInfo(symbol);
      onResolved(symbol, info);
      showToast(`RESOLVED ${symbol}`, "success");
      setQuery("");
    } catch (error) {
      const message =
        error instanceof AxiosError && error.response?.status === 404
          ? `SYMBOL ${symbol} NOT FOUND`
          : "LOOKUP FAILED";
      showToast(message, "error");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="relative w-64">
      <span className="material-symbols-outlined absolute left-md top-1/2 -translate-y-1/2 text-on-surface-variant text-[20px]">
        search
      </span>
      <input
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        placeholder="Search markets (e.g. AAPL)"
        className="terminal-input w-full bg-surface-container-lowest border border-outline-variant rounded-lg py-sm pl-xl pr-md text-on-surface font-body-md focus:outline-none focus:border-primary transition-all placeholder:text-outline-variant placeholder:font-mono-code"
      />
      {isLoading && (
        <span className="material-symbols-outlined absolute right-md top-1/2 -translate-y-1/2 text-primary text-[18px] animate-spin">
          progress_activity
        </span>
      )}
    </form>
  );
}
