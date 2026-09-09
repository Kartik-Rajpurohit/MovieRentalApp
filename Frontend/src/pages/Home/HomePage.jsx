import { useContext } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "primereact/button";
import { Card } from "primereact/card";
import { Message } from "primereact/message";
import { AuthContext } from "../../context/AuthContext";

// Displays a welcome/waiting landing page for authenticated users who have no assigned role yet
// Informs the user that their account is pending administrator approval and provides a logout action
export default function HomePage() {
  const navigate = useNavigate();
  // Access the current user profile and logout action
  const { user, logout } = useContext(AuthContext);

  return (
    <div style={{
      minHeight: "100vh",
      background: "#f5f6fa",
      display: "flex",
      flexDirection: "column",
    }}>

      {/* Minimal topbar — only logout */}
      <div style={{
        height: "60px",
        background: "#fff",
        borderBottom: "1px solid #e5e7eb",
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        padding: "0 28px",
        flexShrink: 0,
      }}>
        <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
          <i className="pi pi-video" style={{ fontSize: "18px", color: "#6366f1" }} />
          <span style={{ fontWeight: 700, fontSize: "16px", color: "#111827" }}>
            Movie Rental
          </span>
        </div>
        <Button
          label="Logout"
          icon="pi pi-sign-out"
          severity="secondary"
          outlined
          onClick={async () => {
            await logout();
            navigate("/login");
          }}
        />
      </div>

      {/* Full page centered content */}
      <div style={{
        flex: 1,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        padding: "24px",
      }}>
        <Card style={{ width: "100%", maxWidth: "560px" }}>
          <div style={{ textAlign: "center", padding: "40px 24px" }}>

            {/* Avatar */}
            <div style={{
              width: "80px", height: "80px", borderRadius: "50%",
              background: "#ede9fe", display: "flex", alignItems: "center",
              justifyContent: "center", margin: "0 auto 24px",
            }}>
              <i className="pi pi-user" style={{ fontSize: "2.5rem", color: "#6366f1" }} />
            </div>

            {/* Name + Email */}
            <h2 style={{ margin: "0 0 8px 0", fontSize: "24px", fontWeight: 600, color: "#111827" }}>
              Welcome, {user?.fullName}!
            </h2>
            <p style={{ margin: "0 0 28px 0", color: "#6b7280", fontSize: "16px" }}>
              {user?.email}
            </p>

            {/* Waiting message */}
            <Message
              severity="warning"
              text="Your account is awaiting admin approval. Once an administrator assigns a role to your account, you'll be able to access all features."
              style={{ marginBottom: "20px", textAlign: "left", width: "100%" }}
            />
            <p style={{ color: "#6b7280", margin: 0, fontSize: "14px" }}>
              You currently have no assigned role. Please wait for the administrator to configure your account.
            </p>

          </div>
        </Card>
      </div>
    </div>
  );
}
