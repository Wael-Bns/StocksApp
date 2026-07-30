import { Card } from "../common/Card";
import { StatusPill } from "../common/StatusPill";
import { PriceChart } from "./PriceChart";
import { useSymbolTicks } from "../../hooks/useSymbolTicks";

interface MarketPanelProps {
  symbol: string | null;
  stockName: string | null;
}

export function MarketPanel({ symbol, stockName }: MarketPanelProps) {
  const ticks = useSymbolTicks(symbol);
  const latest = ticks.at(-1)?.price ?? null;
  const first = ticks[0]?.price ?? null;
  const changePct = latest !== null && first !== null && first !== 0
    ? ((latest - first) / first) * 100
    : null;

  if (!symbol) {
    return (
      <Card className="h-full items-center justify-center min-h-[360px]">
        <span className="material-symbols-outlined text-outline-variant text-[36px]">
          search
        </span>
        <p className="font-body-md text-body-md text-on-surface-variant text-center">
          Search a symbol to resolve its trade info and start streaming its
          live price.
        </p>
      </Card>
    );
  }

  return (
    <Card>
      <div className="flex items-center justify-between flex-wrap gap-sm">
        <div>
          <div className="flex items-center gap-sm">
            <h2 className="font-headline-lg text-headline-lg text-on-surface font-mono-code">
              {symbol}
            </h2>
            {stockName && (
              <span className="font-body-sm text-body-sm text-on-surface-variant">
                {stockName}
              </span>
            )}
          </div>
          <div className="flex items-baseline gap-sm mt-unit">
            <span className="font-mono-code text-headline-xl text-on-surface tabular-nums">
              {latest !== null ? latest.toFixed(2) : "—"}
            </span>
            {changePct !== null && (
              <span
                className={`font-mono-code text-body-md ${
                  changePct >= 0 ? "text-tertiary" : "text-error"
                }`}
              >
                {changePct >= 0 ? "+" : ""}
                {changePct.toFixed(2)}% session
              </span>
            )}
          </div>
        </div>
        <StatusPill
          label={latest !== null ? "Streaming" : "Awaiting tick"}
          active={latest !== null}
        />
      </div>

      <PriceChart ticks={ticks} />
    </Card>
  );
}
