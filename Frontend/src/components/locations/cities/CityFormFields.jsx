import { useState, useEffect } from "react";
import { InputText } from "primereact/inputtext";
import { Dropdown } from "primereact/dropdown";
import { getCountries } from "../../../services/countryService";

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

export default function CityFormFields({ form, setForm, errors }) {
  const [countries, setCountries] = useState([]);

  useEffect(() => {
    getCountries(1, 300)
      .then((res) =>
        setCountries(
          (res.data ?? []).map((c) => ({ label: c.name, value: c.countryId })),
        ),
      )
      .catch(console.error);
  }, []);

  return (
    <>
      {/* City Name */}
      <div>
        <label style={labelStyle}>City Name</label>
        <InputText
          value={form.name ?? ""}
          onChange={(e) =>
            setForm((prev) => ({ ...prev, name: e.target.value }))
          }
          placeholder="Enter city name"
          style={{ width: "100%" }}
          className={errors?.name ? "p-invalid" : ""}
        />
        {errors?.name && <small className="p-error">{errors.name}</small>}
      </div>

      {/* Country */}
      <div>
        <label style={labelStyle}>Country</label>
        <Dropdown
          value={form.countryId}
          options={countries}
          onChange={(e) => setForm((prev) => ({ ...prev, countryId: e.value }))}
          placeholder="Select country"
          style={{ width: "100%" }}
          filter
          className={errors?.countryId ? "p-invalid" : ""}
        />
        {errors?.countryId && (
          <small className="p-error">{errors.countryId}</small>
        )}
      </div>
    </>
  );
}
