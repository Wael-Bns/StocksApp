import { jwtDecode } from "jwt-decode";
import type { DecodedAccessToken } from "../types/auth.types";

/**
 * The AuthenticationResponse the API returns does not include the user's id,
 * only their token. ASP.NET Core Identity JWTs carry the user id in one of a
 * few standard claim types depending on server configuration, so we check
 * each in turn.
 */
export function getUserIdFromToken(token: string): string | null {
  try {
    const decoded = jwtDecode<DecodedAccessToken>(token);
    return (
      decoded.nameid ??
      decoded.sub ??
      decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] as string ??
      null
    );
  } catch {
    return null;
  }
}

export function isTokenExpired(token: string): boolean {
  try {
    const decoded = jwtDecode<DecodedAccessToken>(token);
    if (!decoded.exp) return false;
    return decoded.exp * 1000 < Date.now();
  } catch {
    return true;
  }
}
