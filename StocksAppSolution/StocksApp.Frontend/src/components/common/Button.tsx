import type { ButtonHTMLAttributes, ReactNode } from "react";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary" | "danger";
  icon?: string;
  isLoading?: boolean;
  children: ReactNode;
}

const VARIANT_CLASSES: Record<NonNullable<ButtonProps["variant"]>, string> = {
  primary:
    "bg-primary text-on-primary hover:bg-primary-container",
  secondary:
    "bg-transparent border border-outline text-on-surface hover:bg-surface-container-highest hover:border-on-surface",
  danger:
    "bg-transparent border border-error/50 text-error hover:bg-error-container/20 hover:border-error",
};

export function Button({
  variant = "primary",
  icon,
  isLoading = false,
  children,
  className = "",
  disabled,
  ...rest
}: ButtonProps) {
  return (
    <button
      className={`w-full font-body-md font-bold py-md rounded-lg flex items-center justify-center gap-sm active:scale-[0.98] transition-all disabled:opacity-50 disabled:cursor-not-allowed disabled:active:scale-100 ${VARIANT_CLASSES[variant]} ${className}`}
      disabled={disabled || isLoading}
      {...rest}
    >
      {isLoading ? (
        <span className="material-symbols-outlined text-[20px] animate-spin">
          progress_activity
        </span>
      ) : (
        icon && <span className="material-symbols-outlined text-[20px]">{icon}</span>
      )}
      {children}
    </button>
  );
}
