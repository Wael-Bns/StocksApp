import { useEffect, useState } from "react";
import { Card } from "../common/Card";
import { OrdersTable } from "./OrdersTable";
import { tradeService } from "../../services/tradeService";
import type { OrderResponse } from "../../types/trade.types";

interface ActivityTableProps {
  symbol: string | null;
  refreshKey: number;
}

interface MergedOrder extends OrderResponse {
  side: "BUY" | "SELL";
}

type Tab = "active" | "history" | "positions";

const TABS: { id: Tab; label: string; enabled: boolean }[] = [
  { id: "active", label: "Active Orders", enabled: true },
  { id: "history", label: "Trade History", enabled: false },
  { id: "positions", label: "Positions", enabled: false },
];

const RECENT_LIMIT = 8;

export function ActivityTable({ symbol, refreshKey }: ActivityTableProps) {
  const [tab, setTab] = useState<Tab>("active");
  const [orders, setOrders] = useState<MergedOrder[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (tab !== "active") return;
    let cancelled = false;

    async function load() {
      setIsLoading(true);
      try {
        const [buys, sells] = await Promise.all([
          tradeService.getAllBuyOrders(),
          tradeService.getAllSellOrders(),
        ]);
        if (cancelled) return;

        const merged: MergedOrder[] = [
          ...buys.map((order) => ({ ...order, side: "BUY" as const })),
          ...sells.map((order) => ({ ...order, side: "SELL" as const })),
        ]
          .filter((order) =>
            symbol ? order.stockSymbol?.toUpperCase() === symbol : true,
          )
          .sort((a, b) => {
            const timeA = a.dateAndTimeOfOrder ? Date.parse(a.dateAndTimeOfOrder) : 0;
            const timeB = b.dateAndTimeOfOrder ? Date.parse(b.dateAndTimeOfOrder) : 0;
            return timeB - timeA;
          })
          .slice(0, RECENT_LIMIT);

        setOrders(merged);
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    }

    void load();
    return () => {
      cancelled = true;
    };
  }, [symbol, refreshKey, tab]);

  return (
    <Card>
      <div className="flex gap-xs border-b border-outline-variant/30 pb-md -mt-xs -mx-xs px-xs overflow-x-auto">
        {TABS.map((t) =>
          t.enabled ? (
            <button
              key={t.id}
              type="button"
              onClick={() => setTab(t.id)}
              className={`font-label-caps text-label-caps uppercase px-md py-sm rounded-lg whitespace-nowrap transition-all ${
                tab === t.id
                  ? "bg-primary text-on-primary"
                  : "text-on-surface-variant hover:bg-surface-container-highest"
              }`}
            >
              {t.label}
            </button>
          ) : (
            <button
              key={t.id}
              type="button"
              onClick={() => setTab(t.id)}
              title="Not implemented yet — no backing endpoint"
              className={`flex items-center gap-unit font-label-caps text-label-caps uppercase px-md py-sm rounded-lg whitespace-nowrap transition-all ${
                tab === t.id
                  ? "bg-surface-container-highest text-outline"
                  : "text-outline/70 hover:bg-surface-container-highest/50"
              }`}
            >
              {t.label}
              <span className="font-label-caps text-[8px] bg-surface-container-high px-unit rounded-full">
                Soon
              </span>
            </button>
          ),
        )}
      </div>

      {tab !== "active" ? (
        <div className="flex flex-col items-center justify-center gap-sm py-xl text-center">
          <span className="material-symbols-outlined text-outline-variant text-[28px]">
            construction
          </span>
          <p className="font-body-sm text-body-sm text-on-surface-variant max-w-xs">
            {tab === "history"
              ? "Trade history isn't implemented — the API only returns the full open order lists, not a filled/closed history."
              : "Positions isn't implemented — there's no portfolio/holdings endpoint to compute it from yet."}
          </p>
        </div>
      ) : isLoading ? (
        <p className="font-mono-code text-body-sm text-outline text-center py-lg">
          Loading orders...
        </p>
      ) : (
        <>
          <span className="font-label-caps text-[11px] text-outline uppercase -mb-sm">
            {symbol ? `Filtered to ${symbol}` : "All symbols"}
          </span>
          <OrdersTable orders={orders} emptyLabel="No orders recorded yet" />
        </>
      )}
    </Card>
  );
}
