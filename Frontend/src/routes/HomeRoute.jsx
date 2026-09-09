// Route guard for the home/landing page: directs users based on authentication and role status
import { useContext } from "react";
import { Navigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";

export default function HomeRoute({ children }) {
  const { token, user, loading } = useContext(AuthContext);

  // Wait for initial auth state verification to complete
  if (loading) return null;

  // If not logged in, redirect user to the login page
  if (!token) return <Navigate to="/login" replace />;

  // If the user already has an active role, direct them to their main dashboard
  const hasRole = user?.role && user.role !== "Unassigned";
  if (hasRole) return <Navigate to="/dashboard" replace />;

  // Render home/welcome screen for authenticated users awaiting role assignment
  return children;
}
