import { useEffect, useState } from "react";
import { signalRService, type HubStatus } from "../services/signalRService";

export function useHubStatus(): HubStatus {
  const [status, setStatus] = useState<HubStatus>(signalRService.getStatus());

  useEffect(() => signalRService.onStatusChange(setStatus), []);

  return status;
}
