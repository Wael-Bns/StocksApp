import { useMemo } from "react";
import type { PriceTick } from "../../hooks/useSymbolTicks";
import type { MarketCandle } from "../../types/trade.types";

interface PriceChartProps {
  ticks: PriceTick[];
  candles: MarketCandle[];
}

const CHART_WIDTH = 800;
const CHART_HEIGHT = 220;
const PADDING_Y = 16;

/** Pure presentational bar chart — data comes from the caller so a single
 * tick subscription can feed both this chart and any header/summary UI. */
export function PriceChart({ ticks, candles }: PriceChartProps) {
  const bars = useMemo(() => {
    if (candles.length > 0) {
      const min = Math.min(...candles.map((candle) => candle.low));
      const max = Math.max(...candles.map((candle) => candle.high));
      const range = max - min || 1;
      const barWidth = CHART_WIDTH / Math.max(candles.length, 30);
      return candles.slice(-120).map((candle, i) => {
        const x = i * barWidth;
        const highY = CHART_HEIGHT - PADDING_Y - ((candle.high - min) / range) * (CHART_HEIGHT - PADDING_Y * 2);
        const lowY = CHART_HEIGHT - PADDING_Y - ((candle.low - min) / range) * (CHART_HEIGHT - PADDING_Y * 2);
        const openY = CHART_HEIGHT - PADDING_Y - ((candle.open - min) / range) * (CHART_HEIGHT - PADDING_Y * 2);
        const closeY = CHART_HEIGHT - PADDING_Y - ((candle.close - min) / range) * (CHART_HEIGHT - PADDING_Y * 2);
        return { x, width: Math.max(barWidth - 2, 1), y: Math.min(openY, closeY), height: Math.max(Math.abs(closeY - openY), 2), wickY: highY, wickHeight: Math.max(lowY - highY, 1), isUp: candle.close >= candle.open, candle: true };
      });
    }
    if (ticks.length === 0) return [];

    const prices = ticks.map((t) => t.price);
    const min = Math.min(...prices);
    const max = Math.max(...prices);
    const range = max - min || 1;
    const barWidth = CHART_WIDTH / Math.max(ticks.length, 30);

    return ticks.map((tick, i) => {
      const normalized = (tick.price - min) / range;
      const barHeight = normalized * (CHART_HEIGHT - PADDING_Y * 2) + 4;
      const previous = ticks[i - 1]?.price ?? tick.price;
      const isUp = tick.price >= previous;
      return {
        x: i * barWidth,
        width: Math.max(barWidth - 2, 1),
        y: CHART_HEIGHT - PADDING_Y - barHeight,
        height: barHeight,
        isUp, candle: false, wickY: 0, wickHeight: 0,
      };
    });
  }, [candles, ticks]);

  if (bars.length === 0) {
    return (
      <div className="flex items-center justify-center h-[220px]">
        <p className="font-mono-code text-body-sm text-outline">
          Waiting for the first tick from stocksHub...
        </p>
      </div>
    );
  }

  return (
    <svg
      viewBox={`0 0 ${CHART_WIDTH} ${CHART_HEIGHT}`}
      className="w-full h-[220px]"
      preserveAspectRatio="none"
    >
      {bars.map((bar, i) => (
        <g key={i}>
          {bar.candle && (
            <line x1={bar.x + bar.width / 2} x2={bar.x + bar.width / 2} y1={bar.wickY} y2={bar.wickY + bar.wickHeight} stroke={bar.isUp ? "#3cddc7" : "#ffb4ab"} strokeWidth="1" />
          )}
          <rect x={bar.x} y={bar.y} width={bar.width} height={bar.height} rx={1} fill={bar.isUp ? "#3cddc7" : "#ffb4ab"} opacity={0.55 + (i / bars.length) * 0.45} />
        </g>
      ))}
    </svg>
  );
}
