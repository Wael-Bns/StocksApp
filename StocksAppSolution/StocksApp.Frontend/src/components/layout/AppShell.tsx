import type { ReactNode } from "react";

export function AppShell({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-screen bg-background text-on-surface relative overflow-x-hidden">
      <div className="fixed inset-0 opacity-10 pointer-events-none overflow-hidden">
        <div className="absolute w-[800px] h-[800px] -top-96 -left-96 bg-primary rounded-full blur-[200px]" />
        <div className="absolute w-[600px] h-[600px] bottom-[-100px] right-[-100px] bg-tertiary-container rounded-full blur-[150px]" />
        <div
          className="absolute inset-0"
          style={{
            backgroundImage:
              "radial-gradient(#2d3449 0.5px, transparent 0.5px)",
            backgroundSize: "24px 24px",
          }}
        />
      </div>
      <div className="relative z-10">{children}</div>
    </div>
  );
}
