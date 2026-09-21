import { signalRService } from "../services/signalRService";
import type { PriceTick } from "../hooks/useSymbolTicks";

const MAX_TICKS = 60;

interface SymbolState {
  ticks: PriceTick[];
  unsubscribe?: () => void;
}

const symbols = new Map<string, SymbolState>();
const listeners = new Set<() => void>();

function notify() {
  listeners.forEach((l) => l());
}

function ensureTracked(symbol: string) {
  if (symbols.has(symbol)) return;

  const state: SymbolState = { ticks: [] };
  symbols.set(symbol, state);

  signalRService.subscribe(symbol, (price) => {
    state.ticks = [...state.ticks, { price, time: Date.now() }].slice(-MAX_TICKS);
    notify();
  }).then((unsub) => {
    state.unsubscribe = unsub;
  });
}

export const tickStore = {
  subscribe(listener: () => void) {
    listeners.add(listener);
    return () => listeners.delete(listener);
  },
  getTicks(symbol: string): PriceTick[] {
    ensureTracked(symbol);
    return symbols.get(symbol)?.ticks ?? [];
  },
  getPrice(symbol: string): number | null {
    const ticks = this.getTicks(symbol);
    return ticks.length ? ticks[ticks.length - 1].price : null;
  },
};