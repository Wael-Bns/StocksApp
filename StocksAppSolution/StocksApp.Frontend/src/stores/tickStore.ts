import { signalRService } from "../services/signalRService";
import type { PriceTick } from "../hooks/useSymbolTicks";
import { tradeService } from "../services/tradeService";
import type { CandleHydrationResponse, CandleInterval, MarketCandle } from "../types/trade.types";

const MAX_TICKS = 60;

interface SymbolState {
  ticks: PriceTick[];
  candles: MarketCandle[];
  interval: CandleInterval;
  hydration?: CandleHydrationResponse;
  unsubscribe?: () => void;
}

const symbols = new Map<string, SymbolState>();
const listeners = new Set<() => void>();

function notify() {
  listeners.forEach((l) => l());
}

function ensureTracked(symbol: string) {
  if (symbols.has(symbol)) return;

  const state: SymbolState = { ticks: [], candles: [], interval: "1m" };
  symbols.set(symbol, state);

  signalRService.subscribe(symbol, (update) => {
    state.ticks = [...state.ticks, { price: update.price, time: update.time }].slice(-MAX_TICKS);
    const width = intervalWidth(state.interval);
    const bucket = Math.floor(update.time / width) * width;
    const current = state.candles.at(-1);
    if (current && current.time === bucket) {
      state.candles = [...state.candles.slice(0, -1), {
        ...current,
        high: Math.max(current.high, update.price),
        low: Math.min(current.low, update.price),
        close: update.price,
        volume: current.volume + update.volume,
      }];
    } else if (!current || bucket > current.time) {
      state.candles = [...state.candles, {
        time: bucket, open: update.price, high: update.price,
        low: update.price, close: update.price, volume: update.volume,
      }].slice(-391);
    }
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
  getCandles(symbol: string): MarketCandle[] {
    ensureTracked(symbol);
    return symbols.get(symbol)?.candles ?? [];
  },
  async hydrate(symbol: string, interval: CandleInterval): Promise<void> {
    ensureTracked(symbol);
    const state = symbols.get(symbol)!;
    const response = await tradeService.getCandleHydration(symbol, interval);
    state.interval = interval;
    state.hydration = response;
    state.candles = [
      ...response.closed,
      ...(response.current ? [response.current] : []),
    ];
    if (response.lastPrice && state.ticks.length === 0) {
      state.ticks = [{ price: response.lastPrice.price, time: response.lastPrice.time }];
    }
    notify();
  },
  async hydratePrices(symbolsToLoad: string[]): Promise<void> {
    const prices = await tradeService.getMarketPrices(symbolsToLoad);
    prices.forEach((price) => {
      ensureTracked(price.symbol);
      const state = symbols.get(price.symbol)!;
      if (state.ticks.length === 0) {
        state.ticks = [{ price: price.price, time: price.time }];
      }
    });
    notify();
  },
};

function intervalWidth(interval: CandleInterval): number {
  switch (interval) {
    case "5m":
      return 5 * 60_000;
    case "1h":
      return 60 * 60_000;
    case "1d":
      return 24 * 60 * 60_000;
    default:
      return 60_000;
  }
}