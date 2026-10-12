import { useState } from "react";
import { Card } from "../common/Card";
import { StatusPill } from "../common/StatusPill";
import { PriceChart } from "./PriceChart";
import { useSymbolTicks } from "../../hooks/useSymbolTicks";
import { useMarketCandles } from "../../hooks/useMarketCandles";
import type { CandleInterval } from "../../types/trade.types";
import { PREVIEW_CANDLES, PREVIEW_MARKETS } from "../../config/previewData";

const CHART_INTERVALS: Array<{
  label: string;
  value: CandleInterval | null;
}> = [
  { label: "1m", value: "1m" },
  { label: "5m", value: "5m" },
  { label: "15m", value: null },
  { label: "30m", value: null },
  { label: "1h", value: "1h" },
  { label: "4h", value: null },
  { label: "1D", value: "1d" },
];

interface MarketPanelProps {
  symbol: string | null;
  stockName: string | null;
}

export function MarketPanel({ symbol, stockName }: MarketPanelProps) {
  const [interval, setInterval] = useState<CandleInterval>("1m");
  const ticks = useSymbolTicks(symbol);
  const candles = useMarketCandles(symbol, interval);
  const previewMarket = PREVIEW_MARKETS.find((market) => market.symbol === symbol);
  const displayCandles = candles.length ? candles : PREVIEW_CANDLES;
  const latest = ticks.at(-1)?.price ?? previewMarket?.price ?? null;
  const first = ticks[0]?.price ?? null;
  const changePct = latest !== null && first !== null && first !== 0
    ? ((latest - first) / first) * 100
    : null;
  const currentCandle = displayCandles.at(-1);

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
    <Card className="flex-1 min-h-0 rounded-none border-y-0 border-l-0 p-0 gap-0 bg-surface-container-lowest">
      <div className="flex items-start justify-between flex-wrap gap-md px-md py-sm bg-surface-container border-b border-outline-variant">
        <div>
          <div className="flex items-center gap-sm">
            <div className="w-8 h-8 bg-surface-container-high rounded-lg flex items-center justify-center border border-outline-variant font-bold text-primary font-mono-code">
              {symbol.charAt(0)}
            </div>
            <h2 className="font-headline-md text-headline-md text-on-surface font-mono-code">
              {symbol}
            </h2>
            <span className="bg-primary-container/20 text-primary px-unit py-[2px] rounded font-mono-code text-[10px]">
              NASDAQ
            </span>
            {stockName && (
              <span className="font-body-sm text-body-sm text-on-surface-variant">
                {stockName}
              </span>
            )}
          </div>
          <div className="flex items-baseline gap-sm mt-unit">
            <span className="font-mono-code text-3xl text-on-surface tabular-nums">
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
          <div className="flex flex-wrap items-center gap-md mt-md font-mono-code text-[11px]">
            {[
              ["O", currentCandle?.open],
              ["H", currentCandle?.high],
              ["L", currentCandle?.low],
              ["C", currentCandle?.close],
            ].map(([label, value]) => (
              <span key={label as string} className="text-on-surface-variant">
                <span className="text-outline">{label}:</span>{" "}
                <span className="text-on-surface tabular-nums">
                  {typeof value === "number" ? value.toFixed(2) : "—"}
                </span>
              </span>
            ))}
          </div>
        </div>
        <div className="hidden xl:flex flex-col items-end">
          <span className="font-mono-code text-headline-md text-on-surface font-bold">
            {latest !== null ? latest.toFixed(2) : "192.42"}
          </span>
          <span className="font-mono-code text-[11px] text-tertiary">▲ +1.24 (+0.65%)</span>
        </div>
        <StatusPill
          label={latest !== null ? "Streaming" : "Awaiting tick"}
          active={latest !== null}
        />
      </div>

      <div className="flex items-center justify-between gap-md border-b border-outline-variant py-sm px-md">
        <div className="flex items-center gap-unit overflow-x-auto">
          {CHART_INTERVALS.map((item) => (
            <button
              key={item.label}
              type="button"
              disabled={item.value === null}
              title={item.value === null ? `${item.label} candles are not supported by the API yet` : undefined}
              onClick={() => item.value && setInterval(item.value)}
              className={`shrink-0 px-sm py-unit rounded font-mono-code text-[11px] transition-colors ${
                interval === item.value
                  ? "bg-primary text-on-primary"
                  : item.value === null
                    ? "text-outline/50 cursor-not-allowed"
                    : "text-on-surface-variant hover:bg-surface-container-highest hover:text-on-surface"
              }`}
            >
              {item.label}
            </button>
          ))}
        </div>
        <div className="hidden sm:flex items-center gap-md text-outline font-mono-code text-[11px]">
          <span>Indicators</span>
          <span className="material-symbols-outlined text-[16px]">tune</span>
          <span className="material-symbols-outlined text-[16px]">more_horiz</span>
        </div>
      </div>

      <div className="flex-1 min-h-0 overflow-hidden flex flex-col">
        <PriceChart candles={displayCandles} ticks={ticks} />
        <div className="h-28 shrink-0 border-t border-outline-variant bg-surface-container-lowest flex">
          <div className="flex-1 flex flex-col">
            <div className="flex items-center justify-between px-md py-unit border-b border-outline-variant/30 bg-surface-container-low/50">
              <div className="flex items-center gap-sm font-mono-code text-[10px]">
                <span className="flex items-center gap-unit text-on-surface font-bold">
                  <span className="w-1.5 h-1.5 rounded-full bg-tertiary" /> CCI(14)
                </span>
                <span className="text-tertiary">191.26</span>
                <span className="text-outline">Overbought: +100 | Oversold: -100</span>
              </div>
              <span className="material-symbols-outlined text-outline text-[15px]">settings</span>
            </div>
            <svg viewBox="0 0 1000 80" preserveAspectRatio="none" className="flex-1 w-full">
              <line x1="0" x2="1000" y1="20" y2="20" stroke="#464554" strokeDasharray="3 3" />
              <line x1="0" x2="1000" y1="45" y2="45" stroke="#2d3449" />
              <line x1="0" x2="1000" y1="65" y2="65" stroke="#464554" strokeDasharray="3 3" />
              <path d="M0 52 C80 72 120 60 180 42 S290 26 350 60 S450 62 540 38 S650 16 720 24 S840 50 1000 16" fill="none" stroke="#3cddc7" strokeWidth="2" />
              <path d="M0 48 C100 62 150 54 240 34 S350 48 440 60 S560 32 680 24 S840 40 1000 20" fill="none" stroke="#8083ff" strokeWidth="1.2" strokeDasharray="3 3" />
            </svg>
          </div>
          <div className="w-16 border-l border-outline-variant flex flex-col justify-between py-sm px-sm text-right font-mono-code text-[10px] text-outline">
            <span className="text-tertiary">+100.0</span><span>0.0</span><span className="text-error">-100.0</span>
          </div>
        </div>
      </div>
    </Card>
  );
}
