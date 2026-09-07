import { useContext } from "react";
import { Navigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";

export default function HomeRoute({ children }) {
  const { token, user, loading } = useContext(AuthContext);

  // Wait for auth state to initialize
  if (loading) return null;

  // Not logged in — redirect to login
  if (!token) return <Navigate to="/login" replace />;

  // If user has an assigned role, redirect them to dashboard instead of the waiting page
  const hasRole = user?.role && user.role !== "Unassigned";
  if (hasRole) return <Navigate to="/dashboard" replace />;

  return children;
}
