import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:7176/api",
});

// Har request mein token automatically lagao
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// 401 aaye toh refresh karo
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;

    // 401 aaya + already retry nahi kiya + login/refresh endpoint nahi hai
    if (
      error.response?.status === 401 &&
      !original._retry &&
      !original.url?.includes("/auth/")
    ) {
      original._retry = true;

      const storedRefreshToken = localStorage.getItem("refreshToken");
      if (!storedRefreshToken) {
        localStorage.clear();
        window.location.href = "/login";
        return Promise.reject(error);
      }

      try {
        const res = await axios.post(
          "https://localhost:7176/api/Auth/refresh",
          { refreshToken: storedRefreshToken },
        );

        const newToken = res.data.token;
        localStorage.setItem("token", newToken);
        localStorage.setItem("refreshToken", res.data.refreshToken);
        localStorage.setItem("user", JSON.stringify(res.data));

        // Original request retry karo naye token ke saath
        original.headers.Authorization = `Bearer ${newToken}`;
        return api(original);
      } catch {
        localStorage.clear();
        window.location.href = "/login";
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  },
);

export default api;
