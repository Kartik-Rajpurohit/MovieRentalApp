import { useParams, useNavigate } from "react-router-dom";
import { useState, useEffect } from "react";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import { getCustomerById } from "../../services/customerService";
import { FIELD_LABEL, FIELD_VALUE } from "../../utils/constants";

export default function CustomerDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [customer, setCustomer] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => { fetchCustomer(); }, [id]);

  const fetchCustomer = async () => {
    setLoading(true);
    try {
      const data = await getCustomerById(id);
      setCustomer(data);
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
        backPath="/customers"
        backLabel="Customers"
        title={customer?.fullName}
      />

      <Card>
        {/* Header */}
        <div style={{ display: "flex", alignItems: "center", gap: "16px", marginBottom: "32px" }}>
          <div style={{ width: "64px", height: "64px", borderRadius: "50%", background: "#d1fae5", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <i className="pi pi-users" style={{ fontSize: "1.8rem", color: "#10b981" }} />
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: "22px", fontWeight: 600 }}>{customer?.fullName}</h2>
            <span style={{ color: "#6b7280", fontSize: "14px" }}>{customer?.email}</span>
            <div style={{ marginTop: "6px" }}>
              <Tag value={customer?.isActive ? "Active" : "Inactive"} severity={customer?.isActive ? "success" : "danger"} />
            </div>
          </div>
        </div>

        {/* Info Grid */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))", gap: "24px", marginBottom: "32px" }}>
          <div><p style={FIELD_LABEL}>Customer ID</p><p style={FIELD_VALUE}>{customer?.customerId}</p></div>
          <div><p style={FIELD_LABEL}>Full Name</p><p style={FIELD_VALUE}>{customer?.fullName}</p></div>
          <div><p style={{ ...FIELD_LABEL }}>Email</p><p style={{ ...FIELD_VALUE, wordBreak: "break-all" }}>{customer?.email ?? "—"}</p></div>
          <div><p style={FIELD_LABEL}>Store</p><p style={FIELD_VALUE}>Store {customer?.storeId}</p></div>
          <div><p style={FIELD_LABEL}>Joined</p><p style={FIELD_VALUE}>{customer?.createDate ? new Date(customer.createDate).toLocaleDateString() : "—"}</p></div>
          <div>
            <p style={FIELD_LABEL}>Status</p>
            <Tag value={customer?.isActive ? "Active" : "Inactive"} severity={customer?.isActive ? "success" : "danger"} />
          </div>
        </div>

        {/* Address Section */}
        <div style={{ borderTop: "1px solid #e5e7eb", paddingTop: "24px" }}>
          <h3 style={{ margin: "0 0 20px 0", fontSize: "16px", fontWeight: 600, color: "#374151" }}>
            <i className="pi pi-map-marker" style={{ marginRight: "8px", color: "#10b981" }} />
            Address
          </h3>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))", gap: "24px" }}>
            <div><p style={FIELD_LABEL}>Street</p><p style={FIELD_VALUE}>{customer?.street ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>District</p><p style={FIELD_VALUE}>{customer?.district ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>Postal Code</p><p style={FIELD_VALUE}>{customer?.postalCode ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>Phone</p><p style={FIELD_VALUE}>{customer?.phone ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>City</p><p style={FIELD_VALUE}>{customer?.cityName ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>Country</p><p style={FIELD_VALUE}>{customer?.countryName ?? "—"}</p></div>
          </div>
        </div>
      </Card>
    </AppLayout>
  );
}
