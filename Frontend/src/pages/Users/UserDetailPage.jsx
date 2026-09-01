import { useParams, useNavigate } from "react-router-dom";
import { useState, useEffect } from "react";
import { Button } from "primereact/button";
import { Tag } from "primereact/tag";
import { Card } from "primereact/card";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import UserDialog from "../../components/users/UserDialog";
import { toggleUserStatus, getUserById } from "../../services/userService";

const fieldLabelStyle = {
  margin: "0 0 4px 0",
  fontSize: "13px",
  fontWeight: "500",
  color: "#6b7280",
  textTransform: "uppercase",
  letterSpacing: "0.05em",
};

const fieldValueStyle = {
  margin: 0,
  fontSize: "16px",
  color: "#111827",
  fontWeight: "400",
};

export default function UserDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [user, setUser] = useState(null);
  const [pageLoading, setPageLoading] = useState(true);
  const [toggleLoading, setToggleLoading] = useState(false);
  const [editVisible, setEditVisible] = useState(false);

  useEffect(() => {
    fetchUser();
  }, [id]);

  const fetchUser = async () => {
    setPageLoading(true);
    try {
      const data = await getUserById(id);
      setUser(data);
    } catch (err) {
      console.error(err);
    } finally {
      setPageLoading(false);
    }
  };

  const handleToggle = async () => {
    setToggleLoading(true);
    try {
      const updated = await toggleUserStatus(user.userId);
      setUser(updated);
    } catch (err) {
      console.error(err);
    } finally {
      setToggleLoading(false);
    }
  };

  if (pageLoading) {
    return (
      <AppLayout>
        <LoadingSpinner />
      </AppLayout>
    );
  }

  return (
    <AppLayout>
      <UserDialog
        visible={editVisible}
        onHide={() => setEditVisible(false)}
        user={user}
        onSuccess={fetchUser}
        mode="edit"
      />

      <DetailPageHeader
        backPath="/users"
        backLabel="Users"
        title={user?.fullName}
        actions={[
          {
            label: "Edit",
            icon: "pi pi-pencil",
            outlined: true,
            onClick: () => setEditVisible(true),
          },
          {
            label: user?.isActive ? "Disable" : "Enable",
            icon: user?.isActive ? "pi pi-ban" : "pi pi-check-circle",
            severity: user?.isActive ? "danger" : "success",
            outlined: true,
            onClick: handleToggle,
            loading: toggleLoading,
          },
        ]}
      />

      <Card>
        {/* Header — Avatar + Name */}
        <div
          style={{
            display: "flex",
            alignItems: "center",
            gap: "16px",
            marginBottom: "32px",
            flexWrap: "wrap",
          }}
        >
          <div
            style={{
              width: "64px",
              height: "64px",
              borderRadius: "50%",
              background: "#ede9fe",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              flexShrink: 0,
            }}
          >
            <i
              className="pi pi-user"
              style={{ fontSize: "1.8rem", color: "#6366f1" }}
            />
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: "22px", fontWeight: 600 }}>
              {user?.fullName}
            </h2>
            <span style={{ color: "#6b7280", fontSize: "14px" }}>
              {user?.email}
            </span>
            <div style={{ marginTop: "6px" }}>
              <Tag
                value={user?.isActive ? "Active" : "Inactive"}
                severity={user?.isActive ? "success" : "danger"}
              />
            </div>
          </div>
        </div>

        {/* Info Grid */}
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
            gap: "24px",
          }}
        >
          <div>
            <p style={fieldLabelStyle}>ID</p>
            <p style={fieldValueStyle}>{user?.userId}</p>
          </div>
          <div>
            <p style={fieldLabelStyle}>Full Name</p>
            <p style={fieldValueStyle}>{user?.fullName}</p>
          </div>
          <div>
            <p style={fieldLabelStyle}>Email</p>
            <p style={fieldValueStyle}>{user?.email}</p>
          </div>
          <div>
            <p style={fieldLabelStyle}>Role</p>
            <p style={{ ...fieldValueStyle, textTransform: "capitalize" }}>
              {user?.roleName}
            </p>
          </div>
          <div>
            <p style={fieldLabelStyle}>Status</p>
            <Tag
              value={user?.isActive ? "Active" : "Inactive"}
              severity={user?.isActive ? "success" : "danger"}
            />
          </div>
        </div>

        {/* Address Section */}
        <div
          style={{
            borderTop: "1px solid #e5e7eb",
            paddingTop: "24px",
            marginTop: "28px",
          }}
        >
          <h3
            style={{
              margin: "0 0 20px 0",
              fontSize: "16px",
              fontWeight: 600,
              color: "#374151",
            }}
          >
            <i
              className="pi pi-map-marker"
              style={{ marginRight: "8px", color: "#6366f1" }}
            />
            Address
          </h3>
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
              gap: "24px",
            }}
          >
            <div>
              <p style={fieldLabelStyle}>Street</p>
              <p style={fieldValueStyle}>{user?.street ?? "—"}</p>
            </div>
            <div>
              <p style={fieldLabelStyle}>Postal Code</p>
              <p style={fieldValueStyle}>{user?.postalCode ?? "—"}</p>
            </div>
            <div>
              <p style={fieldLabelStyle}>Phone</p>
              <p style={fieldValueStyle}>{user?.phone ?? "—"}</p>
            </div>
            <div>
              <p style={fieldLabelStyle}>City</p>
              <p style={fieldValueStyle}>{user?.cityName ?? "—"}</p>
            </div>
            <div>
              <p style={fieldLabelStyle}>Country</p>
              <p style={fieldValueStyle}>{user?.countryName ?? "—"}</p>
            </div>
          </div>
        </div>
      </Card>
    </AppLayout>
  );
}
