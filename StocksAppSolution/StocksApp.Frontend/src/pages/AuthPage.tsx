import { useState } from "react";
import { Navigate } from "react-router-dom";
import { Card } from "../components/common/Card";
import { StatusPill } from "../components/common/StatusPill";
import { LoginForm } from "../components/auth/LoginForm";
import { RegisterForm } from "../components/auth/RegisterForm";
import { useAuth } from "../hooks/useAuth";

type AuthMode = "login" | "register";

export function AuthPage() {
  const { isAuthenticated } = useAuth();
  const [mode, setMode] = useState<AuthMode>("login");

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <div className="min-h-screen flex items-center justify-center px-md py-xl">
        <main className="w-full max-w-md">
          <div className="flex flex-col items-center mb-xl">
            <div className="flex items-center gap-sm mb-xs">
              <span
                className="material-symbols-outlined text-primary text-headline-xl"
                style={{ fontVariationSettings: "'FILL' 1" }}
              >
                terminal
              </span>
              <h1 className="font-headline-xl text-headline-xl text-primary tracking-tighter">
                TRADIFY
              </h1>
            </div>
            <p className="font-label-caps text-label-caps text-outline uppercase tracking-widest">
              Global Trading Protocol
            </p>
          </div>

          <Card glass className="border border-outline-variant">
            <div className="flex justify-between items-start">
              <div>
                <h2 className="font-headline-md text-headline-md text-on-surface">
                  {mode === "login" ? "Authentication" : "Node Registration"}
                </h2>
                <p className="font-body-sm text-body-sm text-on-surface-variant">
                  {mode === "login"
                    ? "Access the high-frequency trading node."
                    : "Provision a new operator identity."}
                </p>
              </div>
              <StatusPill label="API Ready" />
            </div>

            {mode === "login" ? <LoginForm /> : <RegisterForm />}

            <div className="flex items-center gap-sm px-sm">
              <div className="h-[1px] flex-1 bg-outline-variant/30" />
              <span className="font-label-caps text-[10px] text-outline-variant uppercase">
                or
              </span>
              <div className="h-[1px] flex-1 bg-outline-variant/30" />
            </div>

            <button
              type="button"
              onClick={() => setMode(mode === "login" ? "register" : "login")}
              className="font-body-sm text-body-sm text-primary hover:underline text-center"
            >
              {mode === "login"
                ? "Need an account? Create one"
                : "Already provisioned? Sign in"}
            </button>
          </Card>
        </main>
    </div>
  );
}
