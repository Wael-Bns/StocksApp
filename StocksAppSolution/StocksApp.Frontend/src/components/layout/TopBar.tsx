import type { ReactNode } from "react";
import { useAuth } from "../../hooks/useAuth";

interface TopBarProps {
  title: string;
  subtitle?: string;
  /** Page-specific controls (e.g. the markets search bar) rendered before the icon cluster. */
  actions?: ReactNode;
}

const INERT_ICON_CLASSES =
  "w-9 h-9 rounded-full flex items-center justify-center text-outline hover:text-on-surface-variant cursor-not-allowed transition-colors";

export function TopBar({ title, actions }: TopBarProps) {
  const { session } = useAuth();

  const initial = (session?.userName ?? session?.email ?? "?")
    .charAt(0)
    .toUpperCase();

  return (
    <header className="flex items-center justify-between gap-lg border-b border-outline-variant bg-surface-container-low px-lg h-12 flex-wrap">
      <div className="flex items-center gap-xl">
        {actions}
        <nav className="hidden md:flex items-center gap-lg">
          <span className="font-body-sm font-bold text-primary border-b-2 border-primary h-12 flex items-center">{title}</span>
          <span className="font-body-sm text-on-surface-variant">Portfolio</span>
          <span className="font-body-sm text-on-surface-variant">Orders</span>
        </nav>
      </div>

      <div className="flex items-center gap-md flex-wrap">
        <button
          type="button"
          disabled
          title="Deposits aren't wired to an account/balance endpoint yet"
          className="bg-primary text-on-primary font-body-sm font-bold px-lg py-sm rounded-lg cursor-not-allowed uppercase"
        >
          Deposit
        </button>

        <div className="flex items-center gap-unit">
          <button
            type="button"
            disabled
            title="Notifications — no backing endpoint yet"
            className={INERT_ICON_CLASSES}
          >
            <span className="material-symbols-outlined text-[20px]">
              notifications
            </span>
          </button>
          <button
            type="button"
            disabled
            title="Settings — not implemented yet"
            className={INERT_ICON_CLASSES}
          >
            <span className="material-symbols-outlined text-[20px]">
              settings
            </span>
          </button>
          <button
            type="button"
            disabled
            title="Help — not implemented yet"
            className={INERT_ICON_CLASSES}
          >
            <span className="material-symbols-outlined text-[20px]">
              help
            </span>
          </button>
        </div>

        <div className="flex items-center gap-sm border-l border-outline-variant/30 pl-md">
          <span className="font-mono-code text-body-sm text-on-surface-variant hidden sm:inline">
            {session?.userName ?? session?.email}
          </span>
          <div className="w-9 h-9 rounded-full bg-primary-container flex items-center justify-center font-body-md font-bold text-on-primary-container">
            {initial}
          </div>
        </div>
      </div>
    </header>
  );
}
