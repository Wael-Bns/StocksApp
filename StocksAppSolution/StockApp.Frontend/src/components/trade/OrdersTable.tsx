import { EmptyState } from "../common/EmptyState";
import type { OrderResponse } from "../../types/trade.types";

interface OrdersTableProps {
  orders: OrderResponse[];
  emptyLabel: string;
}

function humanizeKey(key: string): string {
  return key
    .replace(/([a-z0-9])([A-Z])/g, "$1 $2")
    .replace(/^./, (c) => c.toUpperCase());
}

/** Renders rows without assuming a fixed schema, since /allbuyorders and
 * /allsellorders response shapes aren't documented in the OpenAPI spec. */
export function OrdersTable({ orders, emptyLabel }: OrdersTableProps) {
  if (orders.length === 0) {
    return <EmptyState icon="receipt_long" title={emptyLabel} />;
  }

  const columns = Array.from(
    orders.reduce((keys, order) => {
      Object.keys(order).forEach((key) => keys.add(key));
      return keys;
    }, new Set<string>()),
  );

  return (
    <div className="overflow-x-auto">
      <table className="w-full border-collapse">
        <thead>
          <tr className="border-b border-outline-variant/30">
            {columns.map((column) => (
              <th
                key={column}
                className="text-left py-sm px-sm font-label-caps text-[10px] text-outline uppercase whitespace-nowrap"
              >
                {humanizeKey(column)}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {orders.map((order, index) => (
            <tr
              key={index}
              className="border-b border-surface-container-highest/60 hover:bg-surface-container-high/40 transition-colors"
            >
              {columns.map((column) => (
                <td
                  key={column}
                  className="py-sm px-sm font-mono-code text-body-sm text-on-surface whitespace-nowrap"
                >
                  {order[column] !== null && order[column] !== undefined
                    ? String(order[column])
                    : "—"}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
