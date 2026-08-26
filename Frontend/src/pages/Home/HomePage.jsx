import { useContext } from "react";
import { useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import { Message } from "primereact/message";
import AppLayout from "../../components/layout/AppLayout";
import { AuthContext } from "../../context/AuthContext";

export default function HomePage() {
  const { user, logout } = useContext(AuthContext);
  const navigate = useNavigate();

  if (!user) {
    return navigate("/login");
  }

  const hasRole = user.role && user.role !== "Unassigned";

  return (
    <AppLayout>
          <Card>
            <div style={{ textAlign: "center", paddingTop: "40px" }}>
              <div
                style={{
                  width: "80px",
                  height: "80px",
                  borderRadius: "50%",
                  background: "#ede9fe",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  margin: "0 auto 24px",
                }}
              >
                <i
                  className="pi pi-user"
                  style={{ fontSize: "2.5rem", color: "#6366f1" }}
                />
              </div>

              <h2
                style={{
                  margin: "0 0 8px 0",
                  fontSize: "24px",
                  fontWeight: 600,
                }}
              >
                Welcome, {user.fullName}!
              </h2>
              <p
                style={{
                  margin: "0 0 24px 0",
                  color: "#6b7280",
                  fontSize: "16px",
                }}
              >
                {user.email}
              </p>

              {!hasRole ? (
                <>
                  <Message
                    severity="warning"
                    text="Your account is awaiting admin approval. Once an administrator assigns a role to your account, you'll be able to access all features."
                    style={{ marginBottom: "32px", textAlign: "left" }}
                  />

                  <p style={{ color: "#6b7280", marginBottom: "24px" }}>
                    You currently have no assigned role. Please wait for the
                    administrator to configure your account.
                  </p>
                </>
              ) : (
                <Message
                  severity="success"
                  text={`Your role: ${user.role}`}
                  style={{ marginBottom: "32px", textAlign: "left" }}
                />
              )}

              <div
                style={{
                  display: "flex",
                  gap: "12px",
                  justifyContent: "center",
                }}
              >
                <Button
                  label="View Profile"
                  icon="pi pi-user"
                  outlined
                  onClick={() => navigate(`/users/${user.userId}`)}
                  disabled={!hasRole}
                />
                <Button
                  label="Logout"
                  icon="pi pi-sign-out"
                  severity="danger"
                  outlined
                  onClick={() => {
                    logout();
                    navigate("/login");
                  }}
                />
              </div>
            </div>
          </Card>
    </AppLayout>
  );
}
