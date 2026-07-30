import { useEffect, useState } from "react";
import { TopBar } from "../components/layout/TopBar";
import { Card } from "../components/common/Card";
import { OrdersTable } from "../components/trade/OrdersTable";
import { tradeService } from "../services/tradeService";
import { useToast } from "../hooks/useToast";
import type { OrderResponse } from "../types/trade.types";

type OrderTab = "buy" | "sell";

export function OrdersPage() {
  const { showToast } = useToast();
  const [activeTab, setActiveTab] = useState<OrderTab>("buy");
  const [buyOrders, setBuyOrders] = useState<OrderResponse[]>([]);
  const [sellOrders, setSellOrders] = useState<OrderResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function loadOrders() {
      setIsLoading(true);
      try {
        const [buys, sells] = await Promise.all([
          tradeService.getAllBuyOrders(),
          tradeService.getAllSellOrders(),
        ]);
        if (!cancelled) {
          setBuyOrders(buys);
          setSellOrders(sells);
        }
      } catch {
        if (!cancelled) showToast("FAILED TO LOAD ORDERS", "error");
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    }

    void loadOrders();
    return () => {
      cancelled = true;
    };
  }, [showToast]);

  return (
    <div className="flex flex-col min-h-screen">
      <TopBar
        title="Order Ledger"
        subtitle="All buy and sell orders recorded by the trading node."
      />

      <div className="p-lg">
      <Card>
        <div className="flex gap-xs border-b border-outline-variant/30 pb-md -mt-xs">
          <button
            onClick={() => setActiveTab("buy")}
            className={`font-label-caps text-label-caps uppercase px-md py-sm rounded-lg transition-all ${
              activeTab === "buy"
                ? "bg-primary text-on-primary"
                : "text-on-surface-variant hover:bg-surface-container-highest"
            }`}
          >
            Buy Orders ({buyOrders.length})
          </button>
          <button
            onClick={() => setActiveTab("sell")}
            className={`font-label-caps text-label-caps uppercase px-md py-sm rounded-lg transition-all ${
              activeTab === "sell"
                ? "bg-primary text-on-primary"
                : "text-on-surface-variant hover:bg-surface-container-highest"
            }`}
          >
            Sell Orders ({sellOrders.length})
          </button>
        </div>

        {isLoading ? (
          <p className="font-mono-code text-body-sm text-outline text-center py-lg">
            Loading orders...
          </p>
        ) : (
          <OrdersTable
            orders={activeTab === "buy" ? buyOrders : sellOrders}
            emptyLabel={
              activeTab === "buy" ? "No buy orders yet" : "No sell orders yet"
            }
          />
        )}
      </Card>
      </div>
    </div>
  );
}
