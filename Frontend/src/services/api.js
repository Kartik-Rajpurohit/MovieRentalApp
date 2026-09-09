// Creates and configures the centralized Axios HTTP client used for all API requests
import axios from "axios";
import { getErrorMessage } from "../utils/errorUtils";

// Base URL of the backend API, loaded from environment variables or fallback to localhost
const API_BASE = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7176/api";

// Create the Axios client instance with cookie support for refresh tokens
const api = axios.create({
  baseURL: API_BASE,
  withCredentials: true, // Automatically sends HttpOnly cookies (like refresh token) with requests
});

// Request Interceptor: Automatically attaches the JWT access token to every outgoing request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

let isRefreshing = false;
let failedQueue = [];
let toastNotifier = null;

// Registers a UI toast notification callback function for displaying API error alerts
export const registerToastNotifier = (notifier) => {
  toastNotifier = notifier;
  return () => {
    toastNotifier = null;
  };
};

// Resolves or rejects queued requests once the token refresh finishes
const processQueue = (error, token = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Response Interceptor: Handles automatic token refresh on 401 Unauthorized errors and displays global error alerts
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;

    // Retry once on 401 — but not for auth endpoints (avoid infinite loops)
    if (
      error.response?.status === 401 &&
      !original._retry &&
      !original.url?.includes("/Auth/")
    ) {
      if (isRefreshing) {
        // While refresh is already in progress, queue this request until refresh completes
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            original.headers.Authorization = `Bearer ${token}`;
            return api(original);
          })
          .catch((err) => Promise.reject(err));
      }

      original._retry = true;
      isRefreshing = true;

      try {
        // Cookie is sent automatically — no refresh token in the request body
        const res = await axios.post(
          `${API_BASE}/Auth/refresh`,
          {},
          { withCredentials: true }
        );

        const newToken = res.data.token;
        localStorage.setItem("token", newToken);
        localStorage.setItem("user", JSON.stringify(res.data));

        processQueue(null, newToken);

        original.headers.Authorization = `Bearer ${newToken}`;
        return api(original);
      } catch (refreshErr) {
        processQueue(refreshErr, null);
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        window.location.href = "/login";
        return Promise.reject(refreshErr);
      } finally {
        isRefreshing = false;
      }
    }

    // For any non-401 error, automatically show a global Toast notification if notifier is registered
    // Exclude /Auth/logout so the user is never bothered by toasts during logout
    if (
      error.response?.status !== 401 &&
      toastNotifier &&
      !error.config?.url?.includes("/Auth/logout")
    ) {
      const data = error.response?.data;
      const status = error.response?.status;
      const summary = data?.title || (status ? `Error (${status})` : "Network Error");
      const detail = getErrorMessage(error);

      toastNotifier({
        severity: status >= 500 ? "error" : "warn",
        summary,
        detail,
        life: 5000,
      });
    }

    return Promise.reject(error);
  },
);

export default api;
