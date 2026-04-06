import type { AuthResponse } from "@/types/Receive/AuthResponse";
import type { CreateUserDTO } from "@/types/Send/CreateUserDTO";
import { api } from "./api";

const BASE_URL = "/auth";
const OAUTH_URL = "/oauth";
let refreshPromise: Promise<AuthResponse> | null = null;

// Service for handling authentication-related API calls
export const AuthService = {
  login: async (email: string, password: string): Promise<AuthResponse> => {
    const res = await api.post(`${BASE_URL}/login`, { email, password });
    return res.data;
  },
  signup: async (data: CreateUserDTO): Promise<AuthResponse> => {
    const res = await api.post(`${BASE_URL}`, data);
    return res.data;
  },

  loginWithGoogle: async (
    credential: string,
  ): Promise<AuthResponse> => {
    const res = await api.post(`${OAUTH_URL}/google`, {
      idToken: credential,
    });
    return res.data;
  },

  // Used to refresh the auth ( JWT ) token when it expires
  // Make it single flight to prevent race conditions
  refreshOnce: async (refreshToken: string): Promise<AuthResponse> => {
    if (!refreshPromise) {
      refreshPromise = api
        .post(`${BASE_URL}/refresh`, { refreshToken })
        .then((res) => res.data)
        .finally(() => {
          refreshPromise = null;
        });
    }

    return refreshPromise;
  },
  logout: async (refreshToken: string): Promise<void> => {
    await api.post(`${BASE_URL}/logout`, { refreshToken });
  },
};
