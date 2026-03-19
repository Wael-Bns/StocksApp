import { Info } from "lucide-react";
import { Alert, AlertDescription } from "./ui/alert";

export function ApiInfoBanner() {
  return (
    <Alert className="mb-6 border-blue-200 bg-blue-50">
      <Info className="h-4 w-4 text-blue-600" />
      <AlertDescription className="text-blue-900 text-sm">
        <strong>API Information:</strong> This app uses your .NET API backend to manage stock trading operations. 
        The .NET API handles all Finnhub API calls server-side for stock data and company profiles. 
        Real-time price updates are provided via WebSocket (available during market hours: 9:30 AM - 4:00 PM ET).
        Make sure your .NET API is running at the configured URL.
      </AlertDescription>
    </Alert>
  );
}