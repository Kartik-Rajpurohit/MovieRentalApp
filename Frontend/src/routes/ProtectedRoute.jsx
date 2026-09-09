// Route guard component that enforces login authentication and role-based page permissions
import { useContext } from "react";
import { Navigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";

export default function ProtectedRoute({ children, allowedRoles }) {
  const { token, user, loading } = useContext(AuthContext);

  // Wait for initial authentication state to load from storage
  if (loading) return null;

  // If user is not authenticated, redirect to login page
  if (!token) return <Navigate to="/login" replace />;

  // If user has no active role assigned, redirect to welcome page
  const hasRole = user?.role && user.role !== "Unassigned";
  if (!hasRole) return <Navigate to="/home" replace />;

  // Role authorization: if specific roles are required, verify user has one of them
  if (allowedRoles && allowedRoles.length > 0) {
    const userRole = user?.role;
    if (!allowedRoles.includes(userRole)) {
      // User is logged in but lacks the required permission — redirect to default dashboard
      return <Navigate to="/dashboard" replace />;
    }
  }

  // User is authenticated and authorized — display the protected page
  return children;
}
