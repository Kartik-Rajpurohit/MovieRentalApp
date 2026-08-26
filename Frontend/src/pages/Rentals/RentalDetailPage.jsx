import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { Button } from "primereact/button";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import { getRentalById, returnRental } from "../../services/rentalService";

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

export default function RentalDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [rental, setRental] = useState(null);
  const [loading, setLoading] = useState(true);
  const [returning, setReturning] = useState(false);

  useEffect(() => { loadRental(); }, [id]);

  const loadRental = async () => {
    setLoading(true);
    try {
      const data = await getRentalById(id);
      setRental(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleReturn = () => {
    confirmDialog({
      message: `Mark rental #${id} as returned?`,
      header: "Confirm Return",
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        setReturning(true);
        try {
          await returnRental(id);
          await loadRental();
        } finally {
          setReturning(false);
        }
      },
    });
  };

  if (loading) {
    return (
      <AppLayout>
        <LoadingSpinner />
      </AppLayout>
    );
  }

  if (!rental) {
    return (
      <AppLayout>
        <p>Rental not found.</p>
        <Button label="Back to Rentals" text onClick={() => navigate("/rentals")} />
      </AppLayout>
    );
  }

  return (
    <AppLayout>
      <ConfirmDialog />

      <DetailPageHeader
        backPath="/rentals"
        backLabel="Rentals"
        title={`Rental #${rental.rentalId}`}
        actions={[
          ...(!rental.isReturned ? [{ label: "Mark as Returned", icon: "pi pi-check", severity: "success", outlined: true, loading: returning, onClick: handleReturn }] : []),
        ]}
      />

      <Card>
        {/* Header — Film name + status */}
        <div style={{ display: "flex", alignItems: "center", gap: "16px", marginBottom: "32px" }}>
          <div style={{ width: "64px", height: "64px", borderRadius: "50%", background: "#ede9fe", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <i className="pi pi-sync" style={{ fontSize: "1.8rem", color: "#6366f1" }} />
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: "22px", fontWeight: 600 }}>{rental.filmTitle}</h2>
            <div style={{ display: "flex", alignItems: "center", gap: "8px", marginTop: "4px" }}>
              <Tag value={rental.isReturned ? "Returned" : "Active"} severity={rental.isReturned ? "success" : "warning"} />
              <span style={{ color: "#6b7280", fontSize: "14px" }}>Rental #{rental.rentalId}</span>
            </div>
          </div>
        </div>

        {/* Info Grid */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "24px" }}>
          <div><p style={FIELD_LABEL}>Rental ID</p><p style={FIELD_VALUE}>{rental.rentalId}</p></div>
          <div><p style={FIELD_LABEL}>Film</p><p style={FIELD_VALUE}>{rental.filmTitle}</p></div>
          <div><p style={FIELD_LABEL}>Inventory ID</p><p style={FIELD_VALUE}>{rental.inventoryId}</p></div>
          <div><p style={FIELD_LABEL}>Customer</p><p style={{ ...FIELD_VALUE, textTransform: "capitalize" }}>{rental.customerName?.toLowerCase()}</p></div>
          <div><p style={FIELD_LABEL}>Staff</p><p style={{ ...FIELD_VALUE, textTransform: "capitalize" }}>{rental.staffName?.toLowerCase()}</p></div>
          <div><p style={FIELD_LABEL}>Rental Date</p><p style={FIELD_VALUE}>{new Date(rental.rentalDate).toLocaleDateString()}</p></div>
          <div><p style={FIELD_LABEL}>Return Date</p><p style={FIELD_VALUE}>{rental.returnDate ? new Date(rental.returnDate).toLocaleDateString() : "Not returned yet"}</p></div>
          <div><p style={FIELD_LABEL}>Total Paid</p><p style={FIELD_VALUE}>${rental.totalPaid?.toFixed(2)}</p></div>
          <div><p style={FIELD_LABEL}>Payments</p><p style={FIELD_VALUE}>{rental.paymentCount}</p></div>
        </div>
      </Card>
    </AppLayout>
  );
}
