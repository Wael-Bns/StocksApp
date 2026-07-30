import { useToast } from "../../hooks/useToast";
import type { ToastVariant } from "../../context/ToastContext";

const VARIANT_ICON: Record<ToastVariant, string> = {
  info: "info",
  success: "check_circle",
  warning: "warning_amber",
  error: "error",
};

const VARIANT_COLOR: Record<ToastVariant, string> = {
  info: "text-primary border-primary/40",
  success: "text-tertiary border-tertiary/40",
  warning: "text-error border-error/40",
  error: "text-error border-error/40",
};

export function Toast() {
  const { toast } = useToast();

  if (!toast) return null;

  return (
    <div
      key={toast.id}
      className="fixed bottom-lg left-1/2 -translate-x-1/2 pointer-events-none z-50 animate-toast-in"
    >
      <div
        className={`bg-surface-container-highest border rounded px-lg py-md flex items-center gap-md ${VARIANT_COLOR[toast.variant]}`}
      >
        <span className="material-symbols-outlined">
          {VARIANT_ICON[toast.variant]}
        </span>
        <span className="font-mono-code text-body-sm text-on-surface">
          {toast.message}
        </span>
      </div>
    </div>
  );
}
