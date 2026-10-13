import { useEffect, useState, useContext } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { Button } from "primereact/button";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import { getRentalById, returnRental } from "../../services/rentalService";
import { getPayments } from "../../services/paymentService";
import { AuthContext } from "../../context/AuthContext";
import { useToastContext } from "../../context/ToastContext";

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

// Displays complete details of a movie rental transaction and provides a return movie action
export default function RentalDetailPage() {
  const { user } = useContext(AuthContext);
  const { showSuccess, showError } = useToastContext();
  // Read rental ID from route parameters
  const { id } = useParams();
  const navigate = useNavigate();
  // State holding rental details
  const [rental, setRental] = useState(null);
  const [loading, setLoading] = useState(true);
  const [returning, setReturning] = useState(false);
  const [payments, setPayments] = useState([]);
  const [paymentsLoading, setPaymentsLoading] = useState(false);

  // Fetch rental details and associated payment ledger whenever rental ID changes
  useEffect(() => {
    loadRental();
    loadPayments();
  }, [id]);

  // Load rental record, customer, staff, and return status from backend
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

  // Load related payments ledger for this rental transaction
  const loadPayments = async () => {
    setPaymentsLoading(true);
    try {
      const res = await getPayments({ rentalId: id });
      setPayments(res.data ?? []);
    } catch (err) {
      console.error(err);
    } finally {
      setPaymentsLoading(false);
    }
  };

  // Mark the rented movie as returned after confirmation
  const handleReturn = () => {
    confirmDialog({
      message: `Mark rental #${id} as returned?`,
      header: "Confirm Return",
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        setReturning(true);
        try {
          await returnRental(id);
          showSuccess("Rental marked as returned successfully");
          await Promise.all([loadRental(), loadPayments()]);
        } catch (err) {
          showError(err.response?.data?.message || err.message || "Failed to mark rental as returned");
        } finally {
          setReturning(false);
        }
      },
    });
  };

  // Show loading spinner while rental details are being fetched
  if (loading) {
    return (
      <AppLayout>
        <LoadingSpinner />
      </AppLayout>
    );
  }

  // Fallback if rental record does not exist
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
        backLabel={user?.role === "Customer" ? "My Rentals" : "Rentals"}
        title={`Rental #${rental.rentalId}`}
        actions={[
          ...(!rental.isReturned && user?.role !== "Customer"
            ? [{ label: "Mark as Returned", icon: "pi pi-check", severity: "success", outlined: true, loading: returning, onClick: handleReturn }]
            : []),
        ]}
      />

      <Card>
        {/* Header — Movie name + status */}
        <div style={{ display: "flex", alignItems: "center", gap: "16px", marginBottom: "32px" }}>
          <div style={{ width: "64px", height: "64px", borderRadius: "50%", background: "#ede9fe", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <i className="pi pi-sync" style={{ fontSize: "1.8rem", color: "#6366f1" }} />
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: "22px", fontWeight: 600 }}>{rental.movieTitle}</h2>
            <div style={{ display: "flex", alignItems: "center", gap: "8px", marginTop: "4px" }}>
              <Tag value={rental.isReturned ? "Returned" : "Active"} severity={rental.isReturned ? "success" : "warning"} />
              <span style={{ color: "#6b7280", fontSize: "14px" }}>Rental #{rental.rentalId}</span>
            </div>
          </div>
        </div>

        {/* Info Grid */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "24px" }}>
          <div><p style={FIELD_LABEL}>Rental ID</p><p style={FIELD_VALUE}>{rental.rentalId}</p></div>
          <div><p style={FIELD_LABEL}>Movie</p><p style={FIELD_VALUE}>{rental.movieTitle}</p></div>
          <div><p style={FIELD_LABEL}>Inventory ID</p><p style={FIELD_VALUE}>{rental.inventoryId}</p></div>
          <div><p style={FIELD_LABEL}>Customer</p><p style={{ ...FIELD_VALUE, textTransform: "capitalize" }}>{rental.customerName?.toLowerCase()}</p></div>
          <div><p style={FIELD_LABEL}>Staff</p><p style={{ ...FIELD_VALUE, textTransform: "capitalize" }}>{rental.staffName?.toLowerCase()}</p></div>
          <div><p style={FIELD_LABEL}>Rental Date</p><p style={FIELD_VALUE}>{new Date(rental.rentalDate).toLocaleDateString()}</p></div>
          <div><p style={FIELD_LABEL}>Return Date</p><p style={FIELD_VALUE}>{rental.returnDate ? new Date(rental.returnDate).toLocaleDateString() : "Not returned yet"}</p></div>
          <div><p style={FIELD_LABEL}>Total Paid</p><p style={FIELD_VALUE}>${rental.totalPaid?.toFixed(2)}</p></div>
          <div><p style={FIELD_LABEL}>Payments</p><p style={FIELD_VALUE}>{rental.paymentCount}</p></div>
        </div>
      </Card>

      {/* Payment Ledger */}
      <Card title="Payment Ledger" style={{ marginTop: "24px" }}>
        <DataTable
          value={payments}
          loading={paymentsLoading}
          emptyMessage="No payments recorded for this rental."
        >
          <Column field="paymentId" header="Payment ID" style={{ width: "120px" }} />
          <Column
            field="amount"
            header="Amount"
            body={(p) => `$${Number(p.amount).toFixed(2)}`}
            style={{ width: "120px" }}
          />
          <Column
            field="paymentDate"
            header="Payment Date"
            body={(p) => (p.paymentDate ? new Date(p.paymentDate).toLocaleDateString() : "—")}
          />
          <Column field="staffName" header="Processed By" />
        </DataTable>
      </Card>
    </AppLayout>
  );
}
