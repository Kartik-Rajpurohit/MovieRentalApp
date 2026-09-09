import { useContext, useEffect, useState } from "react";
import { AuthContext } from "../../context/AuthContext";
import AppLayout from "../../components/layout/AppLayout";
import AdminDashboard from "../../components/dashboard/AdminDashboard";
import StaffDashboard from "../../components/dashboard/StaffDashboard";
import CustomerDashboard from "../../components/dashboard/CustomerDashboard";
import { getDashboard } from "../../services/dashboardService";

// Displays the role-based dashboard with metrics tailored for Admin, Staff, or Customer users
export default function DashboardPage() {
  // Access the currently logged-in user and their role from AuthContext
  const { user } = useContext(AuthContext);
  // State for holding dashboard statistics returned by the backend
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);

  // Fetch dashboard summary statistics when the page is opened
  useEffect(() => {
    getDashboard()
      .then(setStats)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  // Show loading spinner while metrics are being fetched
  if (loading)
    return (
      <AppLayout>
        <div
          style={{
            display: "flex",
            justifyContent: "center",
            paddingTop: "60px",
          }}
        >
          <i
            className="pi pi-spin pi-spinner"
            style={{ fontSize: "2rem", color: "#6366f1" }}
          />
        </div>
      </AppLayout>
    );

  return (
    <AppLayout>
      {/* Conditionally render role-specific dashboard views based on user role */}
      {user?.role === "Admin" && <AdminDashboard stats={stats} />}
      {user?.role === "Staff" && <StaffDashboard stats={stats} />}
      {user?.role === "Customer" && <CustomerDashboard stats={stats} />}
    </AppLayout>
  );
}

