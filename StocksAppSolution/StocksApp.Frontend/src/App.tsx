import { Navigate, Route, Routes } from "react-router-dom";
import { AppShell } from "./components/layout/AppShell";
import { ProtectedRoute } from "./components/layout/ProtectedRoute";
import { Toast } from "./components/common/Toast";
import { AuthPage } from "./pages/AuthPage";
import { MarketsPage } from "./pages/MarketsPage";
import { OrdersPage } from "./pages/OrdersPage";

function App() {
  return (
    <AppShell>
      <Routes>
        <Route path="/auth" element={<AuthPage />} />
        <Route
          path="/markets"
          element={
            <ProtectedRoute>
              <MarketsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/orders"
          element={
            <ProtectedRoute>
              <OrdersPage />
            </ProtectedRoute>
          }
        />
        <Route path="/dashboard" element={<Navigate to="/markets" replace />} />
        <Route path="*" element={<Navigate to="/markets" replace />} />
      </Routes>
      <Toast />
    </AppShell>
  );
}

export default App;
