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

api.interceptors.response.use((response) => {
  if (response.data) {
    response.data = deepTransform(response.data);
  }
  return response;
});

// TODO: This probably doesnt work lmao
let isRefreshing = false;
let refreshPromise: Promise<string> | null = null;

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // If we have tried refreshing and get a 401, reject
    if (originalRequest.url?.includes("/auth/refresh")) {
      return Promise.reject(error);
    }

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      const refreshToken = localStorage.getItem("refreshToken");

      if (!refreshToken) {
        return Promise.reject(error);
      }

      try {
        if (!isRefreshing) {
          isRefreshing = true;

          refreshPromise = AuthService.refresh(refreshToken)
            .then((res) => {
              localStorage.setItem("accessToken", res.token.accessToken);
              localStorage.setItem("refreshToken", res.token.refreshToken);
              return res.token.accessToken;
            })
            .finally(() => {
              isRefreshing = false;
            });
        }

        const newAccessToken = await refreshPromise;

        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;

        return api(originalRequest);
      } catch (err) {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");
        return Promise.reject(err);
      }
    }

    return Promise.reject(error);
  },
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

  if (typeof obj === "string" && isIsoDateString(obj)) {
    return new Date(obj);
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

function isIsoDateString(value: string) {
  return /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/.test(value);
}
