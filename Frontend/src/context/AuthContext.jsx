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
    localStorage.setItem("refreshToken", authResponse.refreshToken); // ADD
    localStorage.setItem("user", JSON.stringify(authResponse));
  };

  const logout = async () => {
    const refreshToken = localStorage.getItem("refreshToken");
    if (refreshToken) {
      try {
        await api.post("/Auth/logout", { refreshToken });
      } catch (e) {
        console.error("Logout error:", e);
      }
    }

    setToken(null);
    setUser(null);
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("user");
  };

  // Access token expire hone par auto refresh karo
  const refresh = useCallback(async () => {
    const storedRefreshToken = localStorage.getItem("refreshToken");
    if (!storedRefreshToken) {
      logout();
      return null;
    }
    try {
      const data = await refreshTokenApi(storedRefreshToken);
      setToken(data.token);
      setUser(data);
      localStorage.setItem("token", data.token);
      localStorage.setItem("refreshToken", data.refreshToken);
      localStorage.setItem("user", JSON.stringify(data));
      return data.token;
    } catch {
      logout();
      return null;
    }
  }, []);

  return (
    <AuthContext.Provider
      value={{ user, token, login, logout, loading, refresh }}
    >
      {children}
    </AuthContext.Provider>
  );
}
