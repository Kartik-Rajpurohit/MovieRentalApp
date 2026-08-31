import { Dropdown } from "primereact/dropdown";
import { useEffect, useState } from "react";
import { getAddresses } from "../../services/addressService";

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

export default function StoreFormFields({ form, setForm, errors }) {
  const [addresses, setAddresses] = useState([]);

  useEffect(() => {
    // Load addresses for dropdown
    getAddresses({ page: 1, pageSize: 100 })
      .then((res) =>
        setAddresses(
          (res.data ?? []).map((a) => ({
            label: `${a.street} — ${a.cityName}`,
            value: a.addressId,
          })),
        ),
      )
      .catch(console.error);
  }, []);

  return (
    <>
      {/* Address */}
      <div>
        <label style={labelStyle}>Address</label>
        <Dropdown
          value={form.addressId}
          options={addresses}
          onChange={(e) => setForm((prev) => ({ ...prev, addressId: e.value }))}
          placeholder="Select address"
          style={{ width: "100%" }}
          filter
          className={errors?.addressId ? "p-invalid" : ""}
        />
        {errors?.addressId && (
          <small className="p-error">{errors.addressId}</small>
        )}
      </div>
    </>
  );
}
