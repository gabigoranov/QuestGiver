import axios from "axios";
import { API_URL } from "./common/apiUrl";
import { AuthService } from "./authService";

// Set up base xios instance with base URL and interceptors for automatic DTO transformation
export const api = axios.create({
  baseURL: API_URL,
});

// Automatically adds Authorization header with token if it exists in localStorage, and transforms DTOs
// Request interceptor: add access token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");
  if (token) config.headers.Authorization = `Bearer ${token}`;

  if (config.data) {
    config.data = deepTransform(config.data);
  }
  return config;
});

// TODO: This probably doesnt work lmao
// Response interceptor: handle 401 and try refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Only try refresh once per request
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      const refreshToken = localStorage.getItem("refreshToken");

      if (refreshToken) {
        try {
          const res = await AuthService.refresh(refreshToken);
          localStorage.setItem("accessToken", res.token.accessToken);
          localStorage.setItem("refreshToken", res.token.refreshToken);
          originalRequest.headers.Authorization = `Bearer ${res.token.accessToken}`;
          return api(originalRequest); // retry original request
        } catch {
          // refresh failed, logout handled elsewhere
          localStorage.removeItem("accessToken");
          localStorage.removeItem("refreshToken");
        }
      }
    }

    return Promise.reject(error);
  }
);

/**
 * A helper method to automatically tansform DTOs so axios can send them to backend
 * Helpts with converting dates to string, and vise-versa
 *
 * @param {*} obj
 * @return {*}  {*}
 */

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function deepTransform(obj: any): any {
  if (obj instanceof Date) {
    return obj.toISOString();
  }

  if (Array.isArray(obj)) {
    return obj.map(deepTransform);
  }

  if (obj && typeof obj === "object") {
    return Object.fromEntries(
      Object.entries(obj).map(([key, value]) => [key, deepTransform(value)]),
    );
  }

  return obj;
}
