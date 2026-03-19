import { BuyOrder, SellOrder } from "../types/trading";

// In-memory storage for orders (frontend only)
class OrdersStore {
  private buyOrders: BuyOrder[] = [];
  private sellOrders: SellOrder[] = [];
  private listeners: (() => void)[] = [];

  subscribe(listener: () => void) {
    this.listeners.push(listener);
    return () => {
      this.listeners = this.listeners.filter((l) => l !== listener);
    };
  }

  private notify() {
    this.listeners.forEach((listener) => listener());
  }

  addBuyOrder(order: Omit<BuyOrder, "buyOrderID" | "tradeAmount">) {
    const newOrder: BuyOrder = {
      ...order,
      buyOrderID: crypto.randomUUID(),
      tradeAmount: order.quantity * order.price,
    };
    this.buyOrders.push(newOrder);
    this.notify();
    return newOrder;
  }

  addSellOrder(order: Omit<SellOrder, "sellOrderID" | "tradeAmount">) {
    const newOrder: SellOrder = {
      ...order,
      sellOrderID: crypto.randomUUID(),
      tradeAmount: order.quantity * order.price,
    };
    this.sellOrders.push(newOrder);
    this.notify();
    return newOrder;
  }

  getBuyOrders(): BuyOrder[] {
    return [...this.buyOrders];
  }

  getSellOrders(): SellOrder[] {
    return [...this.sellOrders];
  }
}

export const ordersStore = new OrdersStore();
