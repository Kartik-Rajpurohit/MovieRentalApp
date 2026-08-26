import { useContext } from "react";
import { Navigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";

// Checks if user is logged in — redirects to /login if not
// Optionally checks role — redirects to /dashboard if role not allowed
export default function ProtectedRoute({ children, allowedRoles }) {
  const { token, user, loading } = useContext(AuthContext);

  // Wait for auth state to load from localStorage
  if (loading) return null;

  // Not logged in — redirect to login
  if (!token) return <Navigate to="/login" replace />;

  // Role check — if allowedRoles provided, check if user has one of them
  if (allowedRoles && allowedRoles.length > 0) {
    const userRole = user?.role;
    if (!allowedRoles.includes(userRole)) {
      // Logged in but wrong role — redirect to dashboard
      return <Navigate to="/dashboard" replace />;
    }
  }

  return children;
}
