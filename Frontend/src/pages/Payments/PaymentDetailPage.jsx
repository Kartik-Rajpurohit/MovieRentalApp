import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import { getPaymentById } from "../../services/paymentService";

const fieldStyle = { display: "flex", flexDirection: "column", gap: "4px" };

const labelStyle = {
  fontSize: "12px",
  color: "#6b7280",
  textTransform: "uppercase",
  letterSpacing: "0.05em",
};

const valueStyle = { fontSize: "15px", color: "#111827", fontWeight: 500 };

export default function PaymentDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [payment, setPayment] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getPaymentById(id)
      .then(setPayment)
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

  if (!payment) {
    return (
      <AppLayout>
        <p>Payment not found.</p>
        <Button label="Back to Payments" icon="pi pi-arrow-left" text onClick={() => navigate("/payments")} />
      </AppLayout>
    );
  }

  return (
    <AppLayout>
      <DetailPageHeader
        backPath="/payments"
        backLabel="Payments"
        title={`Payment #${payment.paymentId}`}
      />

      {/* Main Info Card */}
      <Card>
        {/* Amount badge inside card */}
        <div style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          marginBottom: "24px",
          paddingBottom: "20px",
          borderBottom: "1px solid #f3f4f6",
        }}>
          <span style={{ fontSize: "14px", fontWeight: 600, color: "#374151" }}>
            Payment Summary
          </span>
          <span style={{
            fontSize: "20px",
            fontWeight: 700,
            color: "#16a34a",
            background: "#f0fdf4",
            padding: "6px 18px",
            borderRadius: "8px",
            border: "1px solid #bbf7d0",
            whiteSpace: "nowrap",
          }}>
            ${payment.amount?.toFixed(2)}
          </span>
        </div>

        {/* Fields grid */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: "24px" }}>
          <div style={fieldStyle}>
            <span style={labelStyle}>Film</span>
            <span style={valueStyle}>{payment.filmTitle || "—"}</span>
          </div>

          <div style={fieldStyle}>
            <span style={labelStyle}>Rental ID</span>
            <span style={valueStyle}>#{payment.rentalId}</span>
          </div>

          <div style={fieldStyle}>
            <span style={labelStyle}>Payment Date</span>
            <span style={valueStyle}>
              {payment.paymentDate
                ? new Date(payment.paymentDate).toLocaleDateString("en-US", {
                    year: "numeric",
                    month: "long",
                    day: "numeric",
                  })
                : "—"}
            </span>
          </div>

          <div style={fieldStyle}>
            <span style={labelStyle}>Customer</span>
            <span style={valueStyle}>{payment.customerName}</span>
          </div>

          <div style={fieldStyle}>
            <span style={labelStyle}>Customer ID</span>
            <span style={valueStyle}>#{payment.customerId}</span>
          </div>

          <div style={fieldStyle}>
            <span style={labelStyle}>Processed By (Staff)</span>
            <span style={valueStyle}>{payment.staffName}</span>
          </div>
        </div>
      </Card>
    </AppLayout>
  );
}
