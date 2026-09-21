import { useEffect, useState, type FormEvent } from "react";
import { Card } from "../common/Card";
import { Input } from "../common/Input";
import { Button } from "../common/Button";
import { tradeService } from "../../services/tradeService";
import { useAuth } from "../../hooks/useAuth";
import { useToast } from "../../hooks/useToast";

type Side = "buy" | "sell";

interface TradePanelProps {
  symbol: string | null;
  stockName: string | null;
  onOrderPlaced?: () => void;
}

export function TradePanel({ symbol, stockName, onOrderPlaced }: TradePanelProps) {
  const { userId } = useAuth();
  const { showToast } = useToast();

  const [side, setSide] = useState<Side>("buy");
  const [name, setName] = useState(stockName ?? "");
  const [quantity, setQuantity] = useState("10");
  const [price, setPrice] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (stockName) setName(stockName);
  }, [stockName]);

  const parsedQuantity = Number(quantity) || 0;
  const parsedPrice = Number(price) || 0;
  const estimatedTotal = parsedQuantity * parsedPrice;

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();

    if (!symbol) {
      showToast("RESOLVE A SYMBOL FIRST", "warning");
      return;
    }
    if (!name || !quantity || !price) {
      showToast("ALL ORDER FIELDS REQUIRED", "warning");
      return;
    }
    if (side === "buy" && !userId) {
      showToast("COULD NOT RESOLVE USER ID FROM SESSION", "error");
      return;
    }

    setIsSubmitting(true);
    try {
      if (side === "buy") {
        await tradeService.placeBuyOrder({
          stockSymbol: symbol,
          stockName: name,
          quantity: parsedQuantity,
          price: parsedPrice,
          userId: userId as string,
          dateAndTimeOfOrder: new Date().toISOString(),
        });
      } else {
        await tradeService.placeSellOrder({
          stockSymbol: symbol,
          stockName: name,
          quantity: parsedQuantity,
          price: parsedPrice,
          dateAndTimeOfOrder: new Date().toISOString(),
        });
      }
      showToast(
        `${side === "buy" ? "BUY" : "SELL"} ORDER PLACED FOR ${symbol}`,
        "success",
      );
      setQuantity("10");
      setPrice("");
      onOrderPlaced?.();
    } catch {
      showToast(`${side === "buy" ? "BUY" : "SELL"} ORDER REJECTED`, "error");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <Card className="h-fit">
      <div className="flex items-center justify-between">
        <span className="font-label-caps text-label-caps text-outline uppercase">
          Execute Trade
        </span>
      </div>

      <div className="flex gap-unit bg-surface-container-lowest rounded-lg p-unit">
        <button
          type="button"
          onClick={() => setSide("buy")}
          className={`flex-1 py-sm rounded font-body-md font-bold transition-all ${
            side === "buy"
              ? "bg-tertiary text-on-tertiary"
              : "text-on-surface-variant hover:text-on-surface"
          }`}
        >
          Buy
        </button>
        <button
          type="button"
          onClick={() => setSide("sell")}
          className={`flex-1 py-sm rounded font-body-md font-bold transition-all ${
            side === "sell"
              ? "bg-error text-on-error"
              : "text-on-surface-variant hover:text-on-surface"
          }`}
        >
          Sell
        </button>
      </div>

      <form className="flex flex-col gap-md" onSubmit={handleSubmit}>
        <div className="flex flex-col gap-xs">
          <span className="font-label-caps text-label-caps text-outline uppercase">
            Order Type
          </span>
          <div className="relative">
            <select
              disabled
              value="market"
              title="The API only accepts a fixed price and quantity — no order-type field exists to switch this"
              className="w-full appearance-none bg-surface-container-lowest border border-outline-variant rounded-lg py-md pl-md pr-xl text-on-surface font-body-md cursor-not-allowed opacity-70"
            >
              <option value="market">Market Order</option>
            </select>
            <span className="material-symbols-outlined absolute right-md top-1/2 -translate-y-1/2 text-outline text-[20px] pointer-events-none">
              expand_more
            </span>
          </div>
        </div>

        <div className="flex flex-col gap-xs">
          <span className="font-label-caps text-label-caps text-outline uppercase">
            Symbol
          </span>
          <div className="bg-surface-container-lowest border border-outline-variant rounded-lg py-md px-md font-mono-code text-body-md text-on-surface">
            {symbol ?? "— resolve a symbol —"}
          </div>
        </div>

        <Input
          label="Stock Name"
          placeholder="Apple Inc."
          value={name}
          onChange={(e) => setName(e.target.value)}
        />

        <div className="grid grid-cols-2 gap-md">
          <Input
            label="Price"
            type="number"
            min={1}
            max={10000}
            step="0.01"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
          />
          <Input
            label="Qty"
            type="number"
            min={1}
            max={10000}
            value={quantity}
            onChange={(e) => setQuantity(e.target.value)}
          />
        </div>

        <div
          className="grid grid-cols-4 gap-xs"
          title="Sized against an account balance, which the API doesn't expose"
        >
          {(["25%", "50%", "75%", "MAX"] as const).map((label) => (
            <button
              key={label}
              type="button"
              disabled
              className="py-unit rounded font-label-caps text-[11px] uppercase bg-surface-container-lowest text-outline cursor-not-allowed"
            >
              {label}
            </button>
          ))}
        </div>

        <div className="flex flex-col gap-xs border-t border-outline-variant/20 pt-md">
          <div className="flex justify-between items-center">
            <span className="font-label-caps text-label-caps text-outline uppercase">
              Est. Total
            </span>
            <span className="font-mono-code text-body-md text-on-surface">
              {estimatedTotal > 0 ? estimatedTotal.toFixed(2) : "—"}
            </span>
          </div>
          <div
            className="flex justify-between items-center"
            title="No account/fee endpoint exists yet"
          >
            <span className="font-label-caps text-label-caps text-outline uppercase">
              Trading Fee
            </span>
            <span className="font-mono-code text-body-md text-outline">—</span>
          </div>
          <div
            className="flex justify-between items-center"
            title="No account/balance endpoint exists yet"
          >
            <span className="font-label-caps text-label-caps text-outline uppercase">
              Available Balance
            </span>
            <span className="font-mono-code text-body-md text-outline">—</span>
          </div>
        </div>

        <Button
          type="submit"
          variant={side === "buy" ? "primary" : "danger"}
          icon={side === "buy" ? "shopping_cart" : "sell"}
          isLoading={isSubmitting}
        >
          {side === "buy" ? "Confirm Buy Order" : "Confirm Sell Order"}
        </Button>
      </form>
    </Card>
  );
}
