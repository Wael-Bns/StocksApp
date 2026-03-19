import { useState, useEffect } from "react";
import { useNavigate } from "react-router";
import { apiService } from "../services/apiService";
import { BuyOrder, SellOrder } from "../types/trading";
import { Button } from "../components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "../components/ui/table";
import { Badge } from "../components/ui/badge";
import { ArrowLeft, FileText, TrendingUp, TrendingDown } from "lucide-react";
import * as React from "react";

export function Orders() {
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

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <TrendingUp className="w-12 h-12 animate-pulse mx-auto mb-4 text-blue-600" />
          <p className="text-gray-600">Loading orders...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8 max-w-7xl">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-4xl font-bold mb-2">Order History</h1>
          <p className="text-gray-600">View all your buy and sell orders</p>
        </div>
        <div className="flex gap-3">
          <Button variant="outline" onClick={() => navigate("/")}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back to Trading
          </Button>
          <Button onClick={() => navigate("/orders-pdf")}>
            <FileText className="w-4 h-4 mr-2" />
            Export to PDF
          </Button>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-500">Total Buy Orders</p>
                <p className="text-3xl font-bold text-green-600">{buyOrders.length}</p>
              </div>
              <TrendingUp className="w-12 h-12 text-green-600 opacity-20" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-500">Total Sell Orders</p>
                <p className="text-3xl font-bold text-red-600">{sellOrders.length}</p>
              </div>
              <TrendingDown className="w-12 h-12 text-red-600 opacity-20" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div>
              <p className="text-sm text-gray-500">Total Orders</p>
              <p className="text-3xl font-bold text-blue-600">{buyOrders.length + sellOrders.length}</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Buy Orders Table */}
      <Card className="mb-8">
        <CardHeader>
          <div className="flex items-center gap-2">
            <TrendingUp className="w-5 h-5 text-green-600" />
            <CardTitle>Buy Orders</CardTitle>
            <Badge variant="secondary" className="ml-auto">{buyOrders.length} orders</Badge>
          </div>
        </CardHeader>
        <CardContent>
          {buyOrders.length === 0 ? (
            <div className="text-center py-12 text-gray-500">
              <TrendingUp className="w-12 h-12 mx-auto mb-3 opacity-20" />
              <p>No buy orders yet</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Order ID</TableHead>
                    <TableHead>Stock Symbol</TableHead>
                    <TableHead>Stock Name</TableHead>
                    <TableHead>Date & Time</TableHead>
                    <TableHead className="text-right">Quantity</TableHead>
                    <TableHead className="text-right">Price</TableHead>
                    <TableHead className="text-right">Trade Amount</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {buyOrders.map((order) => (
                    <TableRow key={order.buyOrderID}>
                      <TableCell className="font-mono text-xs">
                        {order.buyOrderID.slice(0, 8)}...
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{order.stockSymbol}</Badge>
                      </TableCell>
                      <TableCell>{order.stockName}</TableCell>
                      <TableCell>{formatDate(order.dateAndTimeOfOrder)}</TableCell>
                      <TableCell className="text-right">{order.quantity.toLocaleString()}</TableCell>
                      <TableCell className="text-right">${order.price.toFixed(2)}</TableCell>
                      <TableCell className="text-right font-semibold text-green-600">
                        ${order.tradeAmount.toFixed(2)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Sell Orders Table */}
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <TrendingDown className="w-5 h-5 text-red-600" />
            <CardTitle>Sell Orders</CardTitle>
            <Badge variant="secondary" className="ml-auto">{sellOrders.length} orders</Badge>
          </div>
        </CardHeader>
        <CardContent>
          {sellOrders.length === 0 ? (
            <div className="text-center py-12 text-gray-500">
              <TrendingDown className="w-12 h-12 mx-auto mb-3 opacity-20" />
              <p>No sell orders yet</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Order ID</TableHead>
                    <TableHead>Stock Symbol</TableHead>
                    <TableHead>Stock Name</TableHead>
                    <TableHead>Date & Time</TableHead>
                    <TableHead className="text-right">Quantity</TableHead>
                    <TableHead className="text-right">Price</TableHead>
                    <TableHead className="text-right">Trade Amount</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {sellOrders.map((order) => (
                    <TableRow key={order.sellOrderID}>
                      <TableCell className="font-mono text-xs">
                        {order.sellOrderID.slice(0, 8)}...
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{order.stockSymbol}</Badge>
                      </TableCell>
                      <TableCell>{order.stockName}</TableCell>
                      <TableCell>{formatDate(order.dateAndTimeOfOrder)}</TableCell>
                      <TableCell className="text-right">{order.quantity.toLocaleString()}</TableCell>
                      <TableCell className="text-right">${order.price.toFixed(2)}</TableCell>
                      <TableCell className="text-right font-semibold text-red-600">
                        ${order.tradeAmount.toFixed(2)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}