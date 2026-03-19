import { createBrowserRouter } from "react-router";
import { Root } from "./components/Root";
import { TradingIndex } from "./pages/TradingIndex";
import { Orders } from "./pages/Orders";
import { OrdersPDF } from "./pages/OrdersPDF";

export const router = createBrowserRouter([
  {
    path: "/",
    Component: Root,
    children: [
      { index: true, Component: TradingIndex },
      { path: "orders", Component: Orders },
      { path: "orders-pdf", Component: OrdersPDF },
    ],
  },
]);
