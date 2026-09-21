import type { HTMLAttributes, ReactNode } from "react";

interface CardProps extends HTMLAttributes<HTMLDivElement> {
  children: ReactNode;
  glass?: boolean;
}

export function Card({ children, glass = false, className = "", ...rest }: CardProps) {
  const base = glass
    ? "glass-panel inner-glow"
    : "bg-surface-container-low border border-outline-variant";

  return (
    <div className={`${base} rounded-xl p-lg flex flex-col gap-lg ${className}`} {...rest}>
      {children}
    </div>
  );
}
