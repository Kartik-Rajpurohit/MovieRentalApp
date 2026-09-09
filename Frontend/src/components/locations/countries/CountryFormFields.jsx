import { InputText } from "primereact/inputtext";
import { LABEL_STYLE } from "../../../utils/constants";

// Form input fields for creating or editing a country.
export default function CountryFormFields({ form, setForm }) {
  return (
    <div>
      <label style={LABEL_STYLE}>Country Name</label>
      {/* Input field to enter the country name */}
      <InputText
        value={form.name}
        onChange={(e) => setForm((prev) => ({ ...prev, name: e.target.value }))}
        placeholder="Enter country name"
        style={{ width: "100%" }}
      />
    </div>
  );
}

