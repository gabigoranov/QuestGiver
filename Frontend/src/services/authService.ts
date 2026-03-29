import type { AuthResponse } from "@/types/Receive/AuthResponse";
import type { CreateUserDTO } from "@/types/Send/CreateUserDTO";
import { api } from "./api";

const BASE_URL = "/auth";

// Service for handling authentication-related API calls
export const AuthService = {
  login: async (email: string, password: string): Promise<AuthResponse> => {
    const res = await api.post(`${BASE_URL}/login`, { email, password });
    return res.data;
  },
  signup: async (data: CreateUserDTO): Promise<AuthResponse> => {
    const res = await api.post(`${BASE_URL}/signup`, data);
    return res.data;
  },
  // Used to refresh the auth ( JWT ) token when it expires
  refresh: async (refreshToken: string): Promise<AuthResponse> => {
    const res = await api.post(`${BASE_URL}/refresh`, {refreshToken});
    return res.data;
  },
  logout: async (refreshToken: string): Promise<void> => {
    await api.post(`${BASE_URL}/logout`, {refreshToken});
  },
};
