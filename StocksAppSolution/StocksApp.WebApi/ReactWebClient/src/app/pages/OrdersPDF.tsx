import { useState, useEffect } from "react";
import { useNavigate } from "react-router";
import { apiService } from "../services/apiService";
import { BuyOrder, SellOrder } from "../types/trading";
import { Button } from "../components/ui/button";
import { ArrowLeft, Download } from "lucide-react";

export function OrdersPDF() {
  const navigate = useNavigate();
  const [buyOrders, setBuyOrders] = useState<BuyOrder[]>([]);
  const [sellOrders, setSellOrders] = useState<SellOrder[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadOrders();
  }, []);

  async function loadOrders() {
    setLoading(true);
    try {
      const [buyOrdersData, sellOrdersData] = await Promise.all([
        apiService.getAllBuyOrders(),
        apiService.getAllSellOrders(),
      ]);
      
      setBuyOrders(buyOrdersData);
      setSellOrders(sellOrdersData);
    } catch (error) {
      console.error("Error loading orders:", error);
    } finally {
      setLoading(false);
    }
  }

  function formatDate(date: Date): string {
    return new Date(date).toLocaleString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  }

  function handlePrint() {
    window.print();
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <p className="text-gray-600">Loading orders...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-white">
      {/* No-print header */}
      <div className="no-print bg-gray-50 border-b p-4">
        <div className="container mx-auto flex items-center justify-between">
          <Button variant="outline" onClick={() => navigate("/orders")}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back to Orders
          </Button>
          <Button onClick={handlePrint}>
            <Download className="w-4 h-4 mr-2" />
            Print / Save as PDF
          </Button>
        </div>
      </div>

      {/* Printable content */}
      <div className="container mx-auto p-8 max-w-6xl">
        {/* Header */}
        <div className="text-center mb-8 pb-6 border-b-2 border-gray-900">
          <h1 className="text-3xl font-bold mb-2">Stock Trading Orders Report</h1>
          <p className="text-gray-600">Generated on {formatDate(new Date())}</p>
        </div>

        {/* Summary */}
        <div className="mb-8 grid grid-cols-3 gap-4 text-center">
          <div className="p-4 border-2 border-gray-200 rounded">
            <p className="text-sm text-gray-600 mb-1">Total Buy Orders</p>
            <p className="text-2xl font-bold">{buyOrders.length}</p>
          </div>
          <div className="p-4 border-2 border-gray-200 rounded">
            <p className="text-sm text-gray-600 mb-1">Total Sell Orders</p>
            <p className="text-2xl font-bold">{sellOrders.length}</p>
          </div>
          <div className="p-4 border-2 border-gray-200 rounded">
            <p className="text-sm text-gray-600 mb-1">Total Orders</p>
            <p className="text-2xl font-bold">{buyOrders.length + sellOrders.length}</p>
          </div>
        </div>

        {/* Buy Orders */}
        <div className="mb-8">
          <h2 className="text-2xl font-bold mb-4 pb-2 border-b-2 border-green-600">
            Buy Orders
          </h2>
          {buyOrders.length === 0 ? (
            <p className="text-gray-500 italic p-4">No buy orders</p>
          ) : (
            <table className="w-full border-collapse mb-4">
              <thead>
                <tr className="bg-gray-100">
                  <th className="border border-gray-300 p-2 text-left text-sm">Order ID</th>
                  <th className="border border-gray-300 p-2 text-left text-sm">Symbol</th>
                  <th className="border border-gray-300 p-2 text-left text-sm">Stock Name</th>
                  <th className="border border-gray-300 p-2 text-left text-sm">Date & Time</th>
                  <th className="border border-gray-300 p-2 text-right text-sm">Quantity</th>
                  <th className="border border-gray-300 p-2 text-right text-sm">Price</th>
                  <th className="border border-gray-300 p-2 text-right text-sm">Amount</th>
                </tr>
              </thead>
              <tbody>
                {buyOrders.map((order, index) => (
                  <tr key={order.buyOrderID} className={index % 2 === 0 ? "bg-white" : "bg-gray-50"}>
                    <td className="border border-gray-300 p-2 text-xs font-mono">
                      {order.buyOrderID}
                    </td>
                    <td className="border border-gray-300 p-2 text-sm font-semibold">
                      {order.stockSymbol}
                    </td>
                    <td className="border border-gray-300 p-2 text-sm">{order.stockName}</td>
                    <td className="border border-gray-300 p-2 text-sm">
                      {formatDate(order.dateAndTimeOfOrder)}
                    </td>
                    <td className="border border-gray-300 p-2 text-sm text-right">
                      {order.quantity.toLocaleString()}
                    </td>
                    <td className="border border-gray-300 p-2 text-sm text-right">
                      ${order.price.toFixed(2)}
                    </td>
                    <td className="border border-gray-300 p-2 text-sm text-right font-semibold">
                      ${order.tradeAmount.toFixed(2)}
                    </td>
                  </tr>
                ))}
                {buyOrders.length > 0 && (
                  <tr className="bg-green-50 font-bold">
                    <td colSpan={6} className="border border-gray-300 p-2 text-right text-sm">
                      Total Buy Amount:
                    </td>
                    <td className="border border-gray-300 p-2 text-right text-sm">
                      ${buyOrders.reduce((sum, order) => sum + order.tradeAmount, 0).toFixed(2)}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        {/* Sell Orders */}
        <div className="mb-8">
          <h2 className="text-2xl font-bold mb-4 pb-2 border-b-2 border-red-600">
            Sell Orders
          </h2>
          {sellOrders.length === 0 ? (
            <p className="text-gray-500 italic p-4">No sell orders</p>
          ) : (
            <table className="w-full border-collapse mb-4">
              <thead>
                <tr className="bg-gray-100">
                  <th className="border border-gray-300 p-2 text-left text-sm">Order ID</th>
                  <th className="border border-gray-300 p-2 text-left text-sm">Symbol</th>
                  <th className="border border-gray-300 p-2 text-left text-sm">Stock Name</th>
                  <th className="border border-gray-300 p-2 text-left text-sm">Date & Time</th>
                  <th className="border border-gray-300 p-2 text-right text-sm">Quantity</th>
                  <th className="border border-gray-300 p-2 text-right text-sm">Price</th>
                  <th className="border border-gray-300 p-2 text-right text-sm">Amount</th>
                </tr>
              </thead>
              <tbody>
                {sellOrders.map((order, index) => (
                  <tr key={order.sellOrderID} className={index % 2 === 0 ? "bg-white" : "bg-gray-50"}>
                    <td className="border border-gray-300 p-2 text-xs font-mono">
                      {order.sellOrderID}
                    </td>
                    <td className="border border-gray-300 p-2 text-sm font-semibold">
                      {order.stockSymbol}
                    </td>
                    <td className="border border-gray-300 p-2 text-sm">{order.stockName}</td>
                    <td className="border border-gray-300 p-2 text-sm">
                      {formatDate(order.dateAndTimeOfOrder)}
                    </td>
                    <td className="border border-gray-300 p-2 text-sm text-right">
                      {order.quantity.toLocaleString()}
                    </td>
                    <td className="border border-gray-300 p-2 text-sm text-right">
                      ${order.price.toFixed(2)}
                    </td>
                    <td className="border border-gray-300 p-2 text-sm text-right font-semibold">
                      ${order.tradeAmount.toFixed(2)}
                    </td>
                  </tr>
                ))}
                {sellOrders.length > 0 && (
                  <tr className="bg-red-50 font-bold">
                    <td colSpan={6} className="border border-gray-300 p-2 text-right text-sm">
                      Total Sell Amount:
                    </td>
                    <td className="border border-gray-300 p-2 text-right text-sm">
                      ${sellOrders.reduce((sum, order) => sum + order.tradeAmount, 0).toFixed(2)}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </div>

        {/* Footer */}
        <div className="mt-12 pt-6 border-t text-center text-sm text-gray-500">
          <p>Stock Trading Platform - Order Report</p>
          <p>This is a computer-generated document</p>
        </div>
      </div>

      {/* Print styles */}
      <style>{`
        @media print {
          .no-print {
            display: none !important;
          }
          body {
            print-color-adjust: exact;
            -webkit-print-color-adjust: exact;
          }
          @page {
            margin: 1cm;
          }
        }
      `}</style>
    </div>
  );
}