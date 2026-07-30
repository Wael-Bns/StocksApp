import { useMemo } from "react";
import type { PriceTick } from "../../hooks/useSymbolTicks";

interface PriceChartProps {
  ticks: PriceTick[];
}

const CHART_WIDTH = 800;
const CHART_HEIGHT = 220;
const PADDING_Y = 16;

/** Pure presentational bar chart — data comes from the caller so a single
 * tick subscription can feed both this chart and any header/summary UI. */
export function PriceChart({ ticks }: PriceChartProps) {
  const bars = useMemo(() => {
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
        isUp,
      };
    });
  }, [ticks]);

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
        <rect
          key={i}
          x={bar.x}
          y={bar.y}
          width={bar.width}
          height={bar.height}
          rx={1}
          fill={bar.isUp ? "#3cddc7" : "#ffb4ab"}
          opacity={0.55 + (i / bars.length) * 0.45}
        />
      ))}
    </svg>
  );
}
