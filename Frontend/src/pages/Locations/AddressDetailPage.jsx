import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import { getAddressById } from "../../services/addressService";

const FIELD_LABEL = {
  margin: "0 0 4px 0",
  fontSize: "12px",
  color: "#6b7280",
  fontWeight: 500,
  textTransform: "uppercase",
  letterSpacing: "0.05em",
};

const FIELD_VALUE = {
  margin: 0,
  fontSize: "15px",
  color: "#111827",
  fontWeight: 600,
};

export default function AddressDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [address, setAddress] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchAddress();
  }, [id]);

  const fetchAddress = async () => {
    try {
      const data = await getAddressById(id);
      setAddress(data);
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

  if (!address) {
    return (
      <AppLayout>
        <p>Address not found.</p>
        <Button label="Back" onClick={() => navigate("/addresses")} />
      </AppLayout>
    );
  }

  return (
    <AppLayout>
      <DetailPageHeader
        backPath="/addresses"
        backLabel="Addresses"
        title={`#${address.addressId} — ${address.street}`}
      />

      <Card>
        {/* Header */}
        <div style={{ display: "flex", alignItems: "center", gap: "16px", marginBottom: "32px", flexWrap: "wrap" }}>
          <div style={{ width: "64px", height: "64px", borderRadius: "50%", background: "#ede9fe", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <i className="pi pi-home" style={{ fontSize: "1.8rem", color: "#6366f1" }} />
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: "22px", fontWeight: 600 }}>
              {address.street}
            </h2>
            <span style={{ color: "#6b7280", fontSize: "14px" }}>
              {address.cityName}, {address.countryName}
            </span>
          </div>
        </div>

        {/* Info Grid */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "24px" }}>
          <div>
            <p style={FIELD_LABEL}>Address ID</p>
            <p style={FIELD_VALUE}>{address.addressId}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Street</p>
            <p style={FIELD_VALUE}>{address.street}</p>
          </div>
          {address.street2 && (
            <div>
              <p style={FIELD_LABEL}>Street 2</p>
              <p style={FIELD_VALUE}>{address.street2}</p>
            </div>
          )}
          <div>
            <p style={FIELD_LABEL}>District</p>
            <p style={FIELD_VALUE}>{address.district || "—"}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>City</p>
            <p
              style={{ ...FIELD_VALUE, color: "#6366f1", cursor: "pointer" }}
              onClick={() => navigate(`/cities/${address.cityId}`)}
            >
              {address.cityName}
            </p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Country</p>
            <p style={FIELD_VALUE}>{address.countryName}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Postal Code</p>
            <p style={FIELD_VALUE}>{address.postalCode || "—"}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Phone</p>
            <p style={FIELD_VALUE}>{address.phone || "—"}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Users</p>
            <p style={FIELD_VALUE}>{address.userCount}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Stores</p>
            <p style={FIELD_VALUE}>{address.storeCount}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Last Update</p>
            <p style={FIELD_VALUE}>
              {address.lastUpdate ? new Date(address.lastUpdate).toLocaleDateString() : "—"}
            </p>
          </div>
        </div>
      </Card>
    </AppLayout>
  );
}
