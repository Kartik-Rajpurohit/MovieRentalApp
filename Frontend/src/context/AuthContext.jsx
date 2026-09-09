// Manages global authentication state, token storage, and session lifecycle.
import { createContext, useState, useEffect, useCallback } from "react";
import { refreshToken as refreshTokenApi } from "../services/authService";
import api from "../services/api";

export const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const savedToken = localStorage.getItem("token");
    const savedUser = localStorage.getItem("user");
    if (savedToken && savedUser) {
      setToken(savedToken);
      setUser(JSON.parse(savedUser));
    }
    setLoading(false);
  }, []);

  const login = (authResponse) => {
    setToken(authResponse.token);
    setUser(authResponse);
    localStorage.setItem("token", authResponse.token);
    localStorage.setItem("user", JSON.stringify(authResponse));
    // Refresh token is now an HttpOnly cookie set by the backend — never stored in localStorage
  };

  const logout = useCallback(async () => {
    try {
      // HttpOnly cookie is sent automatically; backend revokes it and clears the cookie
      await api.post("/Auth/logout", {});
    } catch (e) {
      console.error("Logout error:", e);
    }

    setToken(null);
    setUser(null);
    localStorage.removeItem("token");
    localStorage.removeItem("user");
  }, []);

  // Auto-refresh access token when it expires — cookie is sent automatically
  const refresh = useCallback(async () => {
    try {
      const data = await refreshTokenApi();
      setToken(data.token);
      setUser(data);
      localStorage.setItem("token", data.token);
      localStorage.setItem("user", JSON.stringify(data));
      return data.token;
    } catch {
      logout();
      return null;
    }
  }, [logout]);

  return (
    <AuthContext.Provider
      value={{ user, token, login, logout, loading, refresh }}
    >
      {children}
    </AuthContext.Provider>
  );
}
