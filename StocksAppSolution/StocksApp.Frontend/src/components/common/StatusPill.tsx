interface StatusPillProps {
  label: string;
  active?: boolean;
}

export function StatusPill({ label, active = true }: StatusPillProps) {
  return (
    <div className="flex items-center gap-xs bg-surface-container-lowest px-sm py-unit rounded-full border border-outline-variant/30">
      <span
        className={`w-2 h-2 rounded-full ${
          active
            ? "bg-tertiary status-pulse shadow-[0_0_8px_rgba(60,221,199,0.6)]"
            : "bg-outline"
        }`}
      />
      <span
        className={`font-label-caps text-[10px] uppercase ${
          active ? "text-tertiary" : "text-outline"
        }`}
      >
        {label}
      </span>
    </div>
  );
}
