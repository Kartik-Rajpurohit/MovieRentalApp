import { useEffect, useState } from "react";
import { Dropdown } from "primereact/dropdown";
import { InputNumber } from "primereact/inputnumber";
import { getReturnedUnpaidRentals } from "../../services/rentalService";
import { getStaff } from "../../services/staffService";

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

// Form fields for recording a payment against a returned unpaid rental.
export default function PaymentFormFields({ form, setForm, errors }) {
  // List of returned rentals that have not yet been paid
  const [rentals, setRentals] = useState([]);
  const [rentalsLoading, setRentalsLoading] = useState(false);

  // Load returned unpaid rentals on mount so user can pick one to pay
  useEffect(() => {
    setRentalsLoading(true);
    getReturnedUnpaidRentals()
      .then((res) =>
        setRentals(
          (res.data ?? []).map((r) => ({
            label: `#${r.rentalId} — ${r.filmTitle} (${r.customerName?.toLowerCase()})`,
            value: r.rentalId,
            customerId: r.customerId,
            customerName: r.customerName,
            staffId: r.staffId,
            staffName: r.staffName,
            suggestedAmount: r.suggestedAmount,
          })),
        ),
      )
      .catch(console.error)
      .finally(() => setRentalsLoading(false));

    getStaff(1, 500)
      .then((res) =>
        setStaffList(
          (res.data ?? []).map((s) => ({
            label: s.fullName,
            value: s.staffId,
          })),
        ),
      )
      .catch(console.error);
  }, []);

  // When a rental is selected, auto-populate customer, staff, and suggested amount
  const handleRentalChange = (rentalId) => {
    const selected = rentals.find((r) => r.value === rentalId);
    setForm((prev) => ({
      ...prev,
      rentalId,
      customerId: selected?.customerId ?? null,
      customerName: selected?.customerName ?? "",
      staffId: selected?.staffId ?? null,
      staffName: selected?.staffName ?? "",
      amount: selected?.suggestedAmount ?? null,
    }));
  };


  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
      {/* Rental — active only */}
      <div>
        <label style={labelStyle}>Rental</label>
        <Dropdown
          value={form.rentalId}
          options={rentals}
          onChange={(e) => handleRentalChange(e.value)}
          placeholder={rentalsLoading ? "Loading..." : "Select returned rental"}
          style={{ width: "100%" }}
          filter
          appendTo="self"
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
