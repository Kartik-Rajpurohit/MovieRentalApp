import { useParams, useNavigate } from "react-router-dom";
import { useState, useEffect } from "react";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import { getStaffById } from "../../services/staffService";
import { FIELD_LABEL, FIELD_VALUE } from "../../utils/constants";

export default function StaffDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [staff, setStaff] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => { fetchStaff(); }, [id]);

  const fetchStaff = async () => {
    setLoading(true);
    try {
      const data = await getStaffById(id);
      setStaff(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <AppLayout>
        <LoadingSpinner />
      </AppLayout>
    );
  }

  return (
    <AppLayout>
      <DetailPageHeader
        backPath="/staff"
        backLabel="Staff"
        title={staff?.fullName}
      />

      <Card>
        {/* Header */}
        <div style={{ display: "flex", alignItems: "center", gap: "16px", marginBottom: "32px" }}>
          <div style={{ width: "64px", height: "64px", borderRadius: "50%", background: "#ede9fe", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <i className="pi pi-user" style={{ fontSize: "1.8rem", color: "#6366f1" }} />
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: "22px", fontWeight: 600 }}>{staff?.fullName}</h2>
            <span style={{ color: "#6b7280", fontSize: "14px" }}>{staff?.email}</span>
            <div style={{ marginTop: "6px" }}>
              <Tag value={staff?.isActive ? "Active" : "Inactive"} severity={staff?.isActive ? "success" : "danger"} />
            </div>
          </div>
        </div>

        {/* Info Grid */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))", gap: "24px", marginBottom: "32px" }}>
          <div><p style={FIELD_LABEL}>Staff ID</p><p style={FIELD_VALUE}>{staff?.staffId}</p></div>
          <div><p style={FIELD_LABEL}>Full Name</p><p style={FIELD_VALUE}>{staff?.fullName}</p></div>
          <div><p style={FIELD_LABEL}>Email</p><p style={FIELD_VALUE}>{staff?.email ?? "—"}</p></div>
          <div><p style={FIELD_LABEL}>Store</p><p style={FIELD_VALUE}>Store {staff?.storeId}</p></div>
          <div>
            <p style={FIELD_LABEL}>Status</p>
            <Tag value={staff?.isActive ? "Active" : "Inactive"} severity={staff?.isActive ? "success" : "danger"} />
          </div>
        </div>

        {/* Address Section */}
        <div style={{ borderTop: "1px solid #e5e7eb", paddingTop: "24px" }}>
          <h3 style={{ margin: "0 0 20px 0", fontSize: "16px", fontWeight: 600, color: "#374151" }}>
            <i className="pi pi-map-marker" style={{ marginRight: "8px", color: "#6366f1" }} />
            Address
          </h3>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))", gap: "24px" }}>
            <div><p style={FIELD_LABEL}>Street</p><p style={FIELD_VALUE}>{staff?.street ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>District</p><p style={FIELD_VALUE}>{staff?.district ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>Postal Code</p><p style={FIELD_VALUE}>{staff?.postalCode ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>Phone</p><p style={FIELD_VALUE}>{staff?.phone ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>City</p><p style={FIELD_VALUE}>{staff?.cityName ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>Country</p><p style={FIELD_VALUE}>{staff?.countryName ?? "—"}</p></div>
          </div>
        </div>
      </Card>
    </AppLayout>
  );
}
