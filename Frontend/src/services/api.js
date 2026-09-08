import axios from "axios";
import { getErrorMessage } from "../utils/errorUtils";

// Uses VITE_API_BASE_URL from .env — no hardcoded URLs (fixes Issue #11)
const API_BASE = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7176/api";

const api = axios.create({
  baseURL: API_BASE,
  withCredentials: true, // Sends HttpOnly refresh token cookie automatically on every request
});

// Attach access token to every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

let isRefreshing = false;
let failedQueue = [];
let toastNotifier = null;

export const registerToastNotifier = (notifier) => {
  toastNotifier = notifier;
  return () => {
    toastNotifier = null;
  };
};

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

// On 401 — silently refresh the access token using the HttpOnly cookie
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
