import { API_ROUTES } from "../config/constants";
import { httpClient } from "./httpClient";
import type {
  AuthenticationResponse,
  LoginRequest,
  UserAddRequest,
} from "../types/auth.types";

export const authService = {
  async register(
    payload: UserAddRequest,
  ): Promise<AuthenticationResponse> {
    const { data } = await httpClient.post<AuthenticationResponse>(
      API_ROUTES.register,
      payload,
    );
    return data;
  },

  async login(payload: LoginRequest): Promise<AuthenticationResponse> {
    const { data } = await httpClient.post<AuthenticationResponse>(
      API_ROUTES.login,
      payload,
    );
    return data;
  },
};
