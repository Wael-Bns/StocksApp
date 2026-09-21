import { forwardRef, type InputHTMLAttributes } from "react";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  icon?: string;
  error?: string;
  monoPlaceholder?: boolean;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, icon, error, monoPlaceholder = false, id, className = "", ...rest }, ref) => {
    const inputId = id ?? label.toLowerCase().replace(/\s+/g, "-");

    return (
      <div className="flex flex-col gap-xs">
        <label
          className="font-label-caps text-label-caps text-outline uppercase"
          htmlFor={inputId}
        >
          {label}
        </label>
        <div className="relative">
          {icon && (
            <span className="material-symbols-outlined absolute left-md top-1/2 -translate-y-1/2 text-on-surface-variant text-[20px]">
              {icon}
            </span>
          )}
          <input
            ref={ref}
            id={inputId}
            className={`terminal-input w-full bg-surface-container-lowest border rounded-lg py-md ${
              icon ? "pl-xl" : "pl-md"
            } pr-md text-on-surface font-body-md focus:outline-none transition-all placeholder:text-outline-variant ${
              monoPlaceholder ? "placeholder:font-mono-code" : ""
            } ${error ? "border-error" : "border-outline-variant focus:border-primary"} ${className}`}
            {...rest}
          />
        </div>
        {error && (
          <span className="font-body-sm text-body-sm text-error">{error}</span>
        )}
      </div>
    );
  },
);

Input.displayName = "Input";
