import { useEffect, useState } from "react";
import { AutoComplete } from "primereact/autocomplete";
import { InputNumber } from "primereact/inputnumber";
import { getReturnedUnpaidRentals } from "../../services/rentalService";

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

// Form fields for recording a payment against a returned unpaid rental with server-side AutoComplete
export default function PaymentFormFields({ form, setForm, errors }) {
  const [selectedRental, setSelectedRental] = useState(null);
  const [rentalSuggestions, setRentalSuggestions] = useState([]);

  // Reset selected rental when form.rentalId is cleared
  useEffect(() => {
    if (!form.rentalId) {
      setSelectedRental(null);
    }
  }, [form.rentalId]);

  // Debounced server search for returned unpaid rentals via existing GET /api/Rental
  const searchRentals = async (event) => {
    const query = event.query?.trim();
    if (!query) {
      setRentalSuggestions([]);
      return;
    }
    try {
      const res = await getReturnedUnpaidRentals(1, 10, query);
      setRentalSuggestions(res.data ?? []);
    } catch (err) {
      console.error("Failed to search returned rentals:", err);
      setRentalSuggestions([]);
    }
  };

  // Suggestion item template displaying:
  // #15432 — Academy Dinosaur
  // Customer: Linda Smith · Suggested: $4.99
  const rentalItemTemplate = (item) => (
    <div style={{ display: "flex", flexDirection: "column", gap: "2px" }}>
      <span style={{ fontWeight: 600, color: "#1f2937" }}>
        #{item.rentalId} — {item.movieTitle}
      </span>
      <span style={{ fontSize: "12px", color: "#6b7280" }}>
        Customer: {item.customerName} · Suggested: ${item.suggestedAmount?.toFixed(2)}
      </span>
    </div>
  );

  const handleSelectRental = (e) => {
    const val = e.value;
    setSelectedRental(val);
    if (val && typeof val === "object" && val.rentalId) {
      setForm((prev) => ({
        ...prev,
        rentalId: val.rentalId,
        customerId: val.customerId ?? null,
        customerName: val.customerName ?? "",
        staffId: val.staffId ?? null,
        staffName: val.staffName ?? "",
        amount: val.suggestedAmount ?? null,
      }));
    } else {
      setForm((prev) => ({
        ...prev,
        rentalId: null,
        customerId: null,
        customerName: "",
        staffId: null,
        staffName: "",
        amount: null,
      }));
    }
  };

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
      {/* Server-Side AutoComplete Rental Selection */}
      <div>
        <label style={labelStyle}>Returned Rental</label>
        <AutoComplete
          value={selectedRental}
          suggestions={rentalSuggestions}
          completeMethod={searchRentals}
          field="movieTitle"
          delay={300}
          placeholder="Search by movie title, customer, or rental ID..."
          itemTemplate={rentalItemTemplate}
          onChange={handleSelectRental}
          style={{ width: "100%" }}
          inputStyle={{ width: "100%" }}
          className={errors?.rentalId ? "p-invalid" : ""}
        />
        {errors?.rentalId && (
          <small className="p-error">{errors.rentalId}</small>
        )}
      </div>

      {/* Customer — auto-filled from rental */}
      <div>
        <label style={labelStyle}>Customer</label>
        <div
          style={{
            padding: "8px 12px",
            background: "#f9fafb",
            border: "1px solid #e5e7eb",
            borderRadius: "6px",
            fontSize: "14px",
            color: form.customerName ? "#111827" : "#9ca3af",
            minHeight: "38px",
            textTransform: "capitalize",
          }}
        >
          {form.customerName?.toLowerCase() || "Auto-filled from rental"}
        </div>
      </div>

      {/* Staff — auto-filled from rental */}
      <div>
        <label style={labelStyle}>Staff</label>
        <div
          style={{
            padding: "8px 12px",
            background: "#f9fafb",
            border: "1px solid #e5e7eb",
            borderRadius: "6px",
            fontSize: "14px",
            color: form.staffName ? "#111827" : "#9ca3af",
            minHeight: "38px",
            textTransform: "capitalize",
          }}
        >
          {form.staffName?.toLowerCase() || "Auto-filled from rental"}
        </div>
      </div>

      {/* Amount */}
      <div>
        <label style={labelStyle}>Amount ($)</label>
        <InputNumber
          value={form.amount}
          onValueChange={(e) =>
            setForm((prev) => ({ ...prev, amount: e.value }))
          }
          placeholder="Enter amount"
          mode="currency"
          currency="USD"
          style={{ width: "100%" }}
          inputStyle={{ width: "100%" }}
          min={0}
          className={errors?.amount ? "p-invalid" : ""}
        />
        {errors?.amount && <small className="p-error">{errors.amount}</small>}
      </div>
    </div>
  );
}
