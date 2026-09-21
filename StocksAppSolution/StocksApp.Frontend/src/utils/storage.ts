import { AUTH_STORAGE_KEY } from "../config/constants";
import type { AuthenticationResponse } from "../types/auth.types";

export function saveSession(session: AuthenticationResponse): void {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
}

export function loadSession(): AuthenticationResponse | null {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthenticationResponse;
  } catch {
    return null;
  }
}

export function clearSession(): void {
  localStorage.removeItem(AUTH_STORAGE_KEY);
}
