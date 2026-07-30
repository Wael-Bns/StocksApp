import { NavLink } from "react-router-dom";
import { StatusPill } from "../common/StatusPill";
import { useHubStatus } from "../../hooks/useHubStatus";
import { useAuth } from "../../hooks/useAuth";

const NAV_ITEMS = [
  { to: "/markets", label: "Markets", icon: "monitoring", enabled: true },
  { to: "/orders", label: "Orders", icon: "receipt_long", enabled: true },
  { to: "/portfolio", label: "Portfolio", icon: "pie_chart", enabled: false },
  { to: "/history", label: "History", icon: "history", enabled: false },
  { to: "/alerts", label: "Alerts", icon: "notifications", enabled: false },
] as const;

export function Sidebar() {
  const hubStatus = useHubStatus();
  const { logout } = useAuth();

  return (
    <aside className="hidden md:flex w-[260px] shrink-0 flex-col justify-between h-screen sticky top-0 border-r border-outline-variant/30 bg-surface-container-lowest/60 px-md py-lg">
      <div className="flex flex-col gap-xl">
        <div className="flex flex-col gap-sm">
          <div className="flex items-center gap-sm">
            <span
              className="material-symbols-outlined text-primary text-[24px]"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              terminal
            </span>
            <span className="font-headline-md text-headline-md text-on-surface tracking-tighter">
              TRADIFY
            </span>
          </div>
          <StatusPill
            label={hubStatus === "connected" ? "Stream Live" : hubStatus}
            active={hubStatus === "connected"}
          />
        </div>

        <nav className="flex flex-col gap-unit">
          {NAV_ITEMS.map((item) =>
            item.enabled ? (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  `flex items-center gap-sm px-md py-sm rounded-lg font-body-md text-body-md transition-all ${
                    isActive
                      ? "bg-surface-container-highest text-on-surface"
                      : "text-on-surface-variant hover:bg-surface-container-high hover:text-on-surface"
                  }`
                }
              >
                <span className="material-symbols-outlined text-[20px]">
                  {item.icon}
                </span>
                {item.label}
              </NavLink>
            ) : (
              <div
                key={item.to}
                title="Not implemented yet — no backing endpoint"
                className="flex items-center gap-sm px-md py-sm rounded-lg font-body-md text-body-md text-outline cursor-not-allowed select-none"
              >
                <span className="material-symbols-outlined text-[20px]">
                  {item.icon}
                </span>
                <span className="flex-1">{item.label}</span>
                <span className="font-label-caps text-[9px] uppercase bg-surface-container-highest text-outline px-unit py-[2px] rounded-full">
                  Soon
                </span>
              </div>
            ),
          )}
        </nav>
      </div>

      <div className="flex flex-col gap-md">
        <NavLink
          to="/markets"
          className="w-full text-center bg-primary-container/30 text-primary font-body-md font-bold py-md rounded-lg hover:bg-primary-container/50 transition-all"
        >
          Trade Now
        </NavLink>
        <button
          onClick={logout}
          className="flex items-center gap-sm px-md font-label-caps text-label-caps uppercase text-outline hover:text-error transition-all"
        >
          <span className="material-symbols-outlined text-[18px]">logout</span>
          Sign out
        </button>
      </div>
    </aside>
  );
}
