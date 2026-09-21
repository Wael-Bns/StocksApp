// hooks/useSymbolTicks.ts
import { useSyncExternalStore } from "react";
import { tickStore } from "../stores/tickStore";

export interface PriceTick {
  price: number;
  time: number;
}

const EMPTY_TICKS: PriceTick[] = [];

export function useSymbolTicks(symbol: string | null): PriceTick[] {
  return useSyncExternalStore(
    tickStore.subscribe,
    () => (symbol ? tickStore.getTicks(symbol) : EMPTY_TICKS),
  );
}