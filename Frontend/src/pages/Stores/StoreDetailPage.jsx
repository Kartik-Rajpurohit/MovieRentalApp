import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import { getStoreById } from "../../services/storeService";

const fieldStyle = { display: "flex", flexDirection: "column", gap: "4px" };
const labelStyle = {
  fontSize: "12px",
  color: "#6b7280",
  textTransform: "uppercase",
  letterSpacing: "0.05em",
};
const valueStyle = { fontSize: "15px", color: "#111827", fontWeight: 500 };

export default function StoreDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [store, setStore] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getStoreById(id)
      .then(setStore)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) {
    return (
      <AppLayout>
        <LoadingSpinner />
      </AppLayout>
    );
  }

  if (!store) {
    return (
      <AppLayout>
        <p>Store not found.</p>
        <Button label="Back to Stores" icon="pi pi-arrow-left" text onClick={() => navigate("/stores")} />
      </AppLayout>
    );
  }

  return (
    <AppLayout>
      <DetailPageHeader
        backPath="/stores"
        backLabel="Stores"
        title={`Store #${store.storeId}`}
      />

      {/* Stats row */}
      <div style={{ display: "flex", gap: "16px", marginBottom: "20px" }}>
        <div style={{
          flex: 1, background: "#fff", borderRadius: "10px", padding: "20px",
          border: "1px solid #e5e7eb", textAlign: "center"
        }}>
          <div style={{ fontSize: "28px", fontWeight: 700, color: "#6366f1" }}>{store.totalStaff}</div>
          <div style={{ fontSize: "13px", color: "#6b7280", marginTop: "4px" }}>Staff Members</div>
        </div>
        <div style={{
          flex: 1, background: "#fff", borderRadius: "10px", padding: "20px",
          border: "1px solid #e5e7eb", textAlign: "center"
        }}>
          <div style={{ fontSize: "28px", fontWeight: 700, color: "#0ea5e9" }}>{store.totalCustomers}</div>
          <div style={{ fontSize: "13px", color: "#6b7280", marginTop: "4px" }}>Customers</div>
        </div>
        <div style={{
          flex: 1, background: "#fff", borderRadius: "10px", padding: "20px",
          border: "1px solid #e5e7eb", textAlign: "center"
        }}>
          <div style={{ fontSize: "28px", fontWeight: 700, color: "#16a34a" }}>{store.totalInventory}</div>
          <div style={{ fontSize: "13px", color: "#6b7280", marginTop: "4px" }}>Inventory Copies</div>
        </div>
      </div>

      {/* Store Info Card */}
      <Card style={{ marginBottom: "16px" }}>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: "24px" }}>
          <div style={fieldStyle}>
            <span style={labelStyle}>Manager</span>
            <span style={valueStyle}>{store.managerName}</span>
          </div>

          <div style={fieldStyle}>
            <span style={labelStyle}>Phone</span>
            <span style={valueStyle}>{store.phone || "—"}</span>
          </div>

          <div style={fieldStyle}>
            <span style={labelStyle}>Last Updated</span>
            <span style={valueStyle}>
              {store.lastUpdate
                ? new Date(store.lastUpdate).toLocaleDateString("en-US", {
                    year: "numeric", month: "long", day: "numeric"
                  })
                : "—"}
            </span>
          </div>

          <div style={fieldStyle}>
            <span style={labelStyle}>Address</span>
            <span style={valueStyle}>
              {store.street}
              {store.street2 ? `, ${store.street2}` : ""}
            </span>
          </div>

          <div style={fieldStyle}>
            <span style={labelStyle}>District</span>
            <span style={valueStyle}>{store.district || "—"}</span>
          </div>

          <div style={fieldStyle}>
            <span style={labelStyle}>Postal Code</span>
            <span style={valueStyle}>{store.postalCode || "—"}</span>
          </div>

          <div style={fieldStyle}>
            <span style={labelStyle}>City</span>
            <span style={valueStyle}>{store.cityName}</span>
          </div>

          <div style={fieldStyle}>
            <span style={labelStyle}>Country</span>
            <span style={valueStyle}>{store.countryName}</span>
          </div>
        </div>
      </Card>
    </AppLayout>
  );
}
