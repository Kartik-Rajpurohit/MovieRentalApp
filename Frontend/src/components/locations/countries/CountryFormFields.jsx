import { InputText } from "primereact/inputtext";
import { LABEL_STYLE } from "../../../utils/constants";

export default function CountryFormFields({ form, setForm }) {
  return (
    <div>
      <label style={LABEL_STYLE}>Country Name</label>
      <InputText
        value={form.name}
        onChange={(e) => setForm((prev) => ({ ...prev, name: e.target.value }))}
        placeholder="Enter country name"
        style={{ width: "100%" }}
      />
    </div>
  );
}
