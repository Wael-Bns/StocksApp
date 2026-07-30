// hooks/useStockPrice.ts
import { useSyncExternalStore } from "react";
import { tickStore } from "../stores/tickStore";

export function useStockPrice(symbol: string | null): number | null {
  return useSyncExternalStore(
    tickStore.subscribe,
    () => (symbol ? tickStore.getPrice(symbol) : null),
  );
}