import { useEffect, useSyncExternalStore } from "react";
import { tickStore } from "../stores/tickStore";
import type { CandleInterval } from "../types/trade.types";

export function useMarketCandles(
  symbol: string | null,
  interval: CandleInterval = "1m",
) {
  const candles = useSyncExternalStore(
    tickStore.subscribe,
    () => (symbol ? tickStore.getCandles(symbol) : []),
  );

  useEffect(() => {
    if (!symbol) return;
    void tickStore.hydrate(symbol, interval).catch((error: unknown) => {
      console.error(`Unable to hydrate market candles for ${symbol}.`, error);
    });
  }, [symbol, interval]);

  return candles;
}
