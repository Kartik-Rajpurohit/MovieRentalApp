import { createContext, useState, useEffect, useCallback } from "react";
import { refreshToken as refreshTokenApi, logoutUser } from "../services/authService";
import {
  setAccessToken,
  clearAccessToken,
  onAccessTokenChange,
} from "../services/tokenManager";

// Context providing authentication state and operations to the application
export const AuthContext = createContext();

// Component that wraps the application to provide authentication data and methods to all children
export function AuthProvider({ children }) {
  // Stores current logged-in user profile, role, and ID (without access token)
  const [user, setUser] = useState(() => {
    try {
      const savedUserStr = localStorage.getItem("user");
      if (savedUserStr) {
        const savedUser = JSON.parse(savedUserStr);
        // Sanitize: never allow an access token property in persisted user state
        if (savedUser && savedUser.token) {
          delete savedUser.token;
          localStorage.setItem("user", JSON.stringify(savedUser));
        }
        return savedUser;
      }
    } catch {
      // Ignore parse errors
    }
    return null;
  });

  // Stores the in-memory JWT access token used to authorize API requests
  const [token, setToken] = useState(null);

  // Indicates whether initial auth state check via silent refresh is in progress
  const [loading, setLoading] = useState(true);

  // Synchronize React state whenever the in-memory access token changes
  useEffect(() => {
    const unsubscribe = onAccessTokenChange((newToken) => {
      setToken(newToken);
    });
    return unsubscribe;
  }, []);

  // Restore authenticated session via HttpOnly refresh cookie on application startup / page reload
  useEffect(() => {
    let isMounted = true;

    // Remove any legacy access token persisted in localStorage
    localStorage.removeItem("token");

    const initAuth = async () => {
      try {
        const data = await refreshTokenApi();
        if (!isMounted) return;

        setAccessToken(data.token);
        setToken(data.token);

        const { token: _omitted, ...userWithoutToken } = data;
        setUser(userWithoutToken);
        localStorage.setItem("user", JSON.stringify(userWithoutToken));
      } catch {
        if (!isMounted) return;

        clearAccessToken();
        setToken(null);
        setUser(null);
        localStorage.removeItem("user");
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    initAuth();

    return () => {
      isMounted = false;
    };
  }, []);

  // Saves authentication credentials to in-memory storage and state upon successful login
  const login = (authResponse) => {
    setAccessToken(authResponse.token);
    setToken(authResponse.token);

    // Strip access token before persisting user profile information
    const { token: _omitted, ...userWithoutToken } = authResponse;
    setUser(userWithoutToken);
    localStorage.setItem("user", JSON.stringify(userWithoutToken));
    localStorage.removeItem("token");
    // Refresh token is handled via HttpOnly cookie set by the backend
  };

  // Logs out the user by clearing session state, in-memory tokens, and notifying the backend
  const logout = useCallback(async () => {
    try {
      // HttpOnly cookie is sent automatically; backend revokes it and clears the cookie
      await logoutUser();
    } catch (e) {
      console.error("Logout error:", e);
    }

    clearAccessToken();
    setToken(null);
    setUser(null);
    localStorage.removeItem("token");
    localStorage.removeItem("user");
  }, []);

  // Requests a new JWT access token using the backend refresh token cookie
  const refresh = useCallback(async () => {
    try {
      const data = await refreshTokenApi();
      setAccessToken(data.token);
      setToken(data.token);

      const { token: _omitted, ...userWithoutToken } = data;
      setUser(userWithoutToken);
      localStorage.setItem("user", JSON.stringify(userWithoutToken));
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

