import {
  createContext,
  useCallback,
  useRef,
  useState,
  type ReactNode,
} from "react";

export type ToastVariant = "info" | "success" | "warning" | "error";

export interface ToastState {
  id: number;
  message: string;
  variant: ToastVariant;
}

export interface ToastContextValue {
  toast: ToastState | null;
  showToast: (message: string, variant?: ToastVariant) => void;
}

// eslint-disable-next-line react-refresh/only-export-components
export const ToastContext = createContext<ToastContextValue | undefined>(
  undefined,
);

const TOAST_DURATION_MS = 3200;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toast, setToast] = useState<ToastState | null>(null);
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const idRef = useRef(0);

  const showToast = useCallback(
    (message: string, variant: ToastVariant = "info") => {
      idRef.current += 1;
      setToast({ id: idRef.current, message, variant });

      if (timeoutRef.current) clearTimeout(timeoutRef.current);
      timeoutRef.current = setTimeout(() => setToast(null), TOAST_DURATION_MS);
    },
    [],
  );

  return (
    <ToastContext.Provider value={{ toast, showToast }}>
      {children}
    </ToastContext.Provider>
  );
}
