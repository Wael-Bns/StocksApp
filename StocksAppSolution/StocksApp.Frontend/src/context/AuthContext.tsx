import {
  createContext,
  useCallback,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { authService } from "../services/authService";
import { signalRService } from "../services/signalRService";
import { clearSession, loadSession, saveSession } from "../utils/storage";
import { getUserIdFromToken } from "../utils/jwt";
import type {
  AuthenticationResponse,
  LoginRequest,
  UserAddRequest,
} from "../types/auth.types";

export interface AuthContextValue {
  session: AuthenticationResponse | null;
  userId: string | null;
  isAuthenticated: boolean;
  login: (payload: LoginRequest) => Promise<void>;
  register: (payload: UserAddRequest) => Promise<void>;
  logout: () => void;
}

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthenticationResponse | null>(() =>
    loadSession(),
  );

  const userId = useMemo(
    () => (session?.token ? getUserIdFromToken(session.token) : null),
    [session?.token],
  );

  const login = useCallback(async (payload: LoginRequest) => {
    const result = await authService.login(payload);
    saveSession(result);
    setSession(result);
  }, []);

  const register = useCallback(async (payload: UserAddRequest) => {
    const result = await authService.register(payload);
    saveSession(result);
    setSession(result);
  }, []);

  const logout = useCallback(() => {
    clearSession();
    setSession(null);
    void signalRService.disconnect();
  }, []);

  const value: AuthContextValue = {
    session,
    userId,
    isAuthenticated: Boolean(session?.token),
    login,
    register,
    logout,
  };

  return (
    <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
  );
}
