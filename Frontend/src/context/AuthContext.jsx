// Manages global authentication state, token storage, and user session lifecycle
import { createContext, useState, useEffect, useCallback } from "react";
import { refreshToken as refreshTokenApi } from "../services/authService";
import api from "../services/api";

// Context providing authentication state and operations to the application
export const AuthContext = createContext();

// Component that wraps the application to provide authentication data and methods to all children
export function AuthProvider({ children }) {
  // Stores current logged-in user profile, role, and ID
  const [user, setUser] = useState(null);

  // Stores the JWT access token used to authorize API requests
  const [token, setToken] = useState(null);

  // Indicates whether initial auth state check from localStorage is in progress
  const [loading, setLoading] = useState(true);

  // Check and restore user session from localStorage on initial app load
  useEffect(() => {
    const savedToken = localStorage.getItem("token");
    const savedUser = localStorage.getItem("user");
    if (savedToken && savedUser) {
      setToken(savedToken);
      setUser(JSON.parse(savedUser));
    }
    setLoading(false);
  }, []);

  // Saves authentication credentials to state and localStorage upon successful login
  const login = (authResponse) => {
    setToken(authResponse.token);
    setUser(authResponse);
    localStorage.setItem("token", authResponse.token);
    localStorage.setItem("user", JSON.stringify(authResponse));
    // Refresh token is handled via HttpOnly cookie set by the backend
  };

  // Logs out the user by clearing session state, removing stored tokens, and notifying the backend
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

  // Requests a new JWT access token using the backend refresh token cookie
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
