import { useState, useEffect } from "react";
import { InputText } from "primereact/inputtext";
import { Dropdown } from "primereact/dropdown";
import { getCities } from "../../../services/cityService";

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

export default function AddressFormFields({ form, setForm, errors }) {
  const [cities, setCities] = useState([]);

  useEffect(() => {
    // Load all cities for dropdown
    getCities({ page: 1, pageSize: 600 })
      .then((res) =>
        setCities(
          (res.data ?? []).map((c) => ({
            label: `${c.name} — ${c.countryName}`,
            value: c.cityId,
          })),
        ),
      )
      .catch(console.error);
  }, []);

  return (
    <>
      {/* City */}
      <div>
        <label style={labelStyle}>City</label>
        <Dropdown
          value={form.cityId}
          options={cities}
          onChange={(e) => setForm((prev) => ({ ...prev, cityId: e.value }))}
          placeholder="Select city"
          style={{ width: "100%" }}
          filter
          className={errors?.cityId ? "p-invalid" : ""}
        />
        {errors?.cityId && <small className="p-error">{errors.cityId}</small>}
      </div>

      {/* Street */}
      <div>
        <label style={labelStyle}>Street</label>
        <InputText
          value={form.street ?? ""}
          onChange={(e) =>
            setForm((prev) => ({ ...prev, street: e.target.value }))
          }
          placeholder="Enter street address"
          style={{ width: "100%" }}
          className={errors?.street ? "p-invalid" : ""}
        />
        {errors?.street && <small className="p-error">{errors.street}</small>}
      </div>

      {/* Postal Code */}
      <div>
        <label style={labelStyle}>
          Postal Code{" "}
          <span style={{ color: "#9ca3af", fontWeight: 400 }}>(optional)</span>
        </label>
        <InputText
          value={form.postalCode ?? ""}
          onChange={(e) =>
            setForm((prev) => ({ ...prev, postalCode: e.target.value }))
          }
          placeholder="Enter postal code"
          style={{ width: "100%" }}
        />
      </div>

      {/* Phone */}
      <div>
        <label style={labelStyle}>
          Phone{" "}
          <span style={{ color: "#9ca3af", fontWeight: 400 }}>(optional)</span>
        </label>
        <InputText
          value={form.phone ?? ""}
          onChange={(e) =>
            setForm((prev) => ({ ...prev, phone: e.target.value }))
          }
          placeholder="Enter phone number"
          style={{ width: "100%" }}
        />
      </div>
    </>
  );
}
