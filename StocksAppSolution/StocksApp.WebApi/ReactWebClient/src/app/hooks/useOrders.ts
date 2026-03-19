import { useState, useEffect } from "react";
import { ordersStore } from "../services/ordersStore";
import { BuyOrder, SellOrder } from "../types/trading";

export function useOrders() {
  const [buyOrders, setBuyOrders] = useState<BuyOrder[]>([]);
  const [sellOrders, setSellOrders] = useState<SellOrder[]>([]);

  useEffect(() => {
    // Initial load
    setBuyOrders(ordersStore.getBuyOrders());
    setSellOrders(ordersStore.getSellOrders());

    // Subscribe to updates
    const unsubscribe = ordersStore.subscribe(() => {
      setBuyOrders(ordersStore.getBuyOrders());
      setSellOrders(ordersStore.getSellOrders());
    });

    return unsubscribe;
  }, []);

  return { buyOrders, sellOrders };
}
