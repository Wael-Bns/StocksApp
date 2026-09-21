import * as signalR from "@microsoft/signalr";
import {
  SIGNALR_EVENTS,
  SIGNALR_HUB_URL,
  SIGNALR_METHODS,
} from "../config/constants";
import { loadSession } from "../utils/storage";

type PriceListener = (price: number) => void;
export type HubStatus = "disconnected" | "connecting" | "connected" | "reconnecting";
type StatusListener = (status: HubStatus) => void;

/**
 * Wraps a single shared HubConnection to `stocksHub`. The hosted service on
 * the backend pushes price updates to `Clients.Group(stockSymbol)`, so this
 * service joins/leaves a group per subscribed symbol and fans incoming
 * "ReceivePriceUpdate" events out to per-symbol listeners in the UI.
 */
class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private connectPromise: Promise<void> | null = null;
  private listeners = new Map<string, Set<PriceListener>>();
  private statusListeners = new Set<StatusListener>();
  private status: HubStatus = "disconnected";

  private setStatus(status: HubStatus): void {
    this.status = status;
    this.statusListeners.forEach((listener) => listener(status));
  }

  getStatus(): HubStatus {
    return this.status;
  }

  onStatusChange(listener: StatusListener): () => void {
    this.statusListeners.add(listener);
    listener(this.status);
    return () => this.statusListeners.delete(listener);
  }

  private async ensureConnected(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    if (!this.connectPromise) {
      const session = loadSession();

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(SIGNALR_HUB_URL, {
          accessTokenFactory: () => session?.token ?? "",
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Warning)
        .build();

      this.connection.onreconnecting(() => this.setStatus("reconnecting"));
      this.connection.onreconnected(() => this.setStatus("connected"));
      this.connection.onclose(() => this.setStatus("disconnected"));

      this.connection.on(
        SIGNALR_EVENTS.receivePriceUpdate,
        (symbol: string, price: number) => {
          const symbolListeners = this.listeners.get(symbol.toUpperCase());
          symbolListeners?.forEach((listener) => listener(price));
        },
      );

      this.setStatus("connecting");
      this.connectPromise = this.connection
        .start()
        .then(() => this.setStatus("connected"))
        .catch((error) => {
          this.setStatus("disconnected");
          throw error;
        });
    }

    await this.connectPromise;
  }

  /**
   * Subscribes to live price updates for a stock symbol. Returns an
   * unsubscribe function to call on cleanup (e.g. from a useEffect).
   */
  async subscribe(symbol: string, listener: PriceListener): Promise<() => void> {
    await this.ensureConnected();

    const normalizedSymbol = symbol.toUpperCase();
    const isFirstListenerForSymbol = !this.listeners.has(normalizedSymbol);
    if (isFirstListenerForSymbol) {
      this.listeners.set(normalizedSymbol, new Set());
    }
    this.listeners.get(normalizedSymbol)!.add(listener);

    if (isFirstListenerForSymbol) {
      await this.connection?.invoke(
        SIGNALR_METHODS.subscribe,
        normalizedSymbol,
      );
    }

    return () => {
      void this.unsubscribe(normalizedSymbol, listener);
    };
  }

  private async unsubscribe(
    symbol: string,
    listener: PriceListener,
  ): Promise<void> {
    const symbolListeners = this.listeners.get(symbol);
    if (!symbolListeners) return;

    symbolListeners.delete(listener);

    if (symbolListeners.size === 0) {
      this.listeners.delete(symbol);
      try {
        await this.connection?.invoke(SIGNALR_METHODS.unsubscribe, symbol);
      } catch {
        // Connection may already be closing; nothing actionable to do here.
      }
    }
  }

  async disconnect(): Promise<void> {
    await this.connection?.stop();
    this.connection = null;
    this.connectPromise = null;
    this.listeners.clear();
    this.setStatus("disconnected");
  }
}

export const signalRService = new SignalRService();
