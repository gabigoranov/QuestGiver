import axios from "axios";
import { API_URL } from "./common/apiUrl";

// Set up base xios instance with base URL and interceptors for automatic DTO transformation
export const api = axios.create({
  baseURL: API_URL,
});

// Automatically adds Authorization header with token if it exists in localStorage, and transforms DTOs
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  if (config.data) {
    config.data = deepTransform(config.data);
  }

  return config;
});

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
      Object.entries(obj).map(([key, value]) => [
        key,
        deepTransform(value),
      ])
    );
  }

  return obj;
}

