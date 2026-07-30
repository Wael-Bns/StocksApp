import axios, {
  type AxiosError,
  type InternalAxiosRequestConfig,
} from "axios";
import { API_BASE_URL, API_ROUTES } from "../config/constants";
import { clearSession, loadSession, saveSession } from "../utils/storage";
import type { AuthenticationResponse, TokenModel } from "../types/auth.types";

export const httpClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { "Content-Type": "application/json" },
});

httpClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const session = loadSession();
  if (session?.token) {
    config.headers.set("Authorization", `Bearer ${session.token}`);
  }
  return config;
});

let refreshInFlight: Promise<AuthenticationResponse> | null = null;

/**
 * Calls POST /api/Auth/generate-new-access-token with the current
 * token/refreshToken pair. The endpoint's response schema isn't documented,
 * so the result is treated as a partial AuthenticationResponse and merged
 * over the existing session (preserving userName/email if they're omitted).
 */
async function refreshAccessToken(): Promise<AuthenticationResponse> {
  const session = loadSession();
  if (!session?.token || !session?.refreshToken) {
    throw new Error("No session to refresh.");
  }

  const body: TokenModel = {
    token: session.token,
    refreshToken: session.refreshToken,
  };

  const { data } = await axios.post<Partial<AuthenticationResponse>>(
    `${API_BASE_URL}${API_ROUTES.refreshToken}`,
    body,
  );

  const merged: AuthenticationResponse = { ...session, ...data };
  saveSession(merged);
  return merged;
}

httpClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as
      | (InternalAxiosRequestConfig & { _retry?: boolean })
      | undefined;

    const isAuthRoute =
      originalRequest?.url?.includes(API_ROUTES.login) ||
      originalRequest?.url?.includes(API_ROUTES.register) ||
      originalRequest?.url?.includes(API_ROUTES.refreshToken);

    if (
      error.response?.status === 401 &&
      originalRequest &&
      !originalRequest._retry &&
      !isAuthRoute
    ) {
      originalRequest._retry = true;
      try {
        refreshInFlight = refreshInFlight ?? refreshAccessToken();
        const refreshed = await refreshInFlight;
        refreshInFlight = null;
        originalRequest.headers.set(
          "Authorization",
          `Bearer ${refreshed.token}`,
        );
        return httpClient(originalRequest);
      } catch (refreshError) {
        refreshInFlight = null;
        clearSession();
        window.location.assign("/auth");
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  },
);
