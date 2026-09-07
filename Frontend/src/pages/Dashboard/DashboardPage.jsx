import { useContext, useEffect, useState } from "react";
import { AuthContext } from "../../context/AuthContext";
import AppLayout from "../../components/layout/AppLayout";
import AdminDashboard from "../../components/dashboard/AdminDashboard";
import StaffDashboard from "../../components/dashboard/StaffDashboard";
import CustomerDashboard from "../../components/dashboard/CustomerDashboard";
import { getDashboard } from "../../services/dashboardService";

export default function DashboardPage() {
  const { user } = useContext(AuthContext);
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getDashboard()
      .then(setStats)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

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
      {user?.role === "Admin" && <AdminDashboard stats={stats} />}
      {user?.role === "Staff" && <StaffDashboard stats={stats} />}
      {user?.role === "Customer" && <CustomerDashboard stats={stats} />}
    </AppLayout>
  );
}
