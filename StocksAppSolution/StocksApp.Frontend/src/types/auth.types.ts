/**
 * Types mirroring the Auth schemas in the StocksApp.WebApi OpenAPI document.
 */

/** POST /api/Auth/register body */
export interface UserAddRequest {
  userName: string;
  email: string;
  /** minLength: 6 */
  password: string;
}

/** POST /api/Auth/login body */
export interface LoginRequest {
  email: string;
  password: string;
}

/** POST /api/Auth/generate-new-access-token body */
export interface TokenModel {
  token: string;
  refreshToken: string;
}

/** Response body for /register and /login */
export interface AuthenticationResponse {
  userName: string | null;
  email: string | null;
  token: string | null;
  refreshToken: string | null;
  refreshTokenExpiry: string;
}

/** Decoded shape of the JWT access token payload we rely on client-side. */
export interface DecodedAccessToken {
  /** ASP.NET Identity user id claim, exposed under one of a few possible keys. */
  sub?: string;
  nameid?: string;
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"?: string;
  email?: string;
  exp?: number;
  [claim: string]: unknown;
}
