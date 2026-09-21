import { useEffect, useMemo, useRef, useState } from "react";
import { Card } from "../common/Card";
import { EmptyState } from "../common/EmptyState";
import { useStockPrice } from "../../hooks/useStockPrice";
import { TOP_50_SYMBOLS, type MarketSymbol } from "../../config/topSymbols";

interface WatchlistProps {
  activeSymbol: string | null;
  onSelect: (symbol: MarketSymbol) => void;
}

/** Each row subscribes to its own live price group on stocksHub, so the
 * whole list streams concurrently — the symbols themselves are a fixed
 * reference list (see topSymbols.ts), not something the API provides. */
export function Watchlist({ activeSymbol, onSelect }: WatchlistProps) {
  const [filter, setFilter] = useState("");

  const filtered = useMemo(() => {
    const query = filter.trim().toUpperCase();
    if (!query) return TOP_50_SYMBOLS;
    return TOP_50_SYMBOLS.filter(
      (item) =>
        item.symbol.includes(query) || item.name.toUpperCase().includes(query),
    );
  }, [filter]);

  return (
    <Card className="h-fit">
      <div className="flex items-center justify-between gap-sm">
        <span className="font-label-caps text-label-caps text-outline uppercase whitespace-nowrap">
          Watchlist ({TOP_50_SYMBOLS.length})
        </span>
        <div className="relative flex-1 max-w-[140px]">
          <span className="material-symbols-outlined absolute left-sm top-1/2 -translate-y-1/2 text-outline text-[16px]">
            filter_alt
          </span>
          <input
            value={filter}
            onChange={(e) => setFilter(e.target.value)}
            placeholder="Filter"
            className="w-full bg-surface-container-lowest border border-outline-variant rounded-lg py-unit pl-lg pr-sm text-on-surface font-mono-code text-[12px] focus:outline-none focus:border-primary transition-all placeholder:text-outline-variant"
          />
        </div>
      </div>

      {filtered.length === 0 ? (
        <EmptyState icon="visibility_off" title="No symbols match that filter" />
      ) : (
        <div className="flex flex-col divide-y divide-surface-container-highest/60 -mt-xs max-h-[560px] overflow-y-auto">
          {filtered.map((item) => (
            <WatchlistRow
              key={item.symbol}
              item={item}
              isActive={item.symbol === activeSymbol}
              onSelect={() => onSelect(item)}
            />
          ))}
        </div>
      )}
    </Card>
  );
}

function WatchlistRow({
  item,
  isActive,
  onSelect,
}: {
  item: MarketSymbol;
  isActive: boolean;
  onSelect: () => void;
}) {
  const price = useStockPrice(item.symbol);
  const firstPriceRef = useRef<number | null>(null);
  const [changePct, setChangePct] = useState<number | null>(null);

  useEffect(() => {
    if (price === null) return;
    if (firstPriceRef.current === null) {
      firstPriceRef.current = price;
      return;
    }
    setChangePct(((price - firstPriceRef.current) / firstPriceRef.current) * 100);
  }, [price]);

  return (
    <button
      type="button"
      onClick={onSelect}
      className={`flex items-center justify-between gap-sm py-sm px-xs rounded-lg text-left transition-all ${
        isActive ? "bg-surface-container-highest" : "hover:bg-surface-container-high/50"
      }`}
    >
      <div className="flex flex-col min-w-0">
        <span className="font-body-md text-body-md font-bold text-on-surface font-mono-code">
          {item.symbol}
        </span>
        <span className="font-body-sm text-body-sm text-on-surface-variant truncate max-w-[140px]">
          {item.name}
        </span>
      </div>

      <div className="flex flex-col items-end shrink-0">
        <span className="font-mono-code text-body-sm text-on-surface tabular-nums">
          {price !== null ? price.toFixed(2) : "—"}
        </span>
        {changePct !== null ? (
          <span
            className={`font-mono-code text-[11px] ${
              changePct >= 0 ? "text-tertiary" : "text-error"
            }`}
          >
            {changePct >= 0 ? "+" : ""}
            {changePct.toFixed(2)}%
          </span>
        ) : (
          <span className="font-mono-code text-[11px] text-outline">
            {price !== null ? "live" : "connecting"}
          </span>
        )}
      </div>
    </button>
  );
}
