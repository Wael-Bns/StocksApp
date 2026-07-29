interface EmptyStateProps {
  icon: string;
  title: string;
  description?: string;
}

export function EmptyState({ icon, title, description }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center gap-sm py-xl text-center">
      <span className="material-symbols-outlined text-outline-variant text-[32px]">
        {icon}
      </span>
      <p className="font-body-md text-body-md text-on-surface-variant">{title}</p>
      {description && (
        <p className="font-body-sm text-body-sm text-outline max-w-xs">
          {description}
        </p>
      )}
    </div>
  );
}
