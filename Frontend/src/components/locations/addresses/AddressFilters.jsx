import { InputText } from "primereact/inputtext";
import { LABEL_STYLE } from "../../../utils/constants";

export default function AddressFilters({ filters, setFilter }) {
  return (
    <>
      <div>
        <label style={LABEL_STYLE}>City</label>
        <InputText
          value={filters.city ?? ""}
          onChange={(e) => setFilter("city")(e.target.value || null)}
          placeholder="Filter by city name"
          style={{ width: "100%" }}
        />
      </div>
      <div>
        <label style={LABEL_STYLE}>Postal Code</label>
        <InputText
          value={filters.postalCode ?? ""}
          onChange={(e) => setFilter("postalCode")(e.target.value || null)}
          placeholder="Filter by postal code"
          style={{ width: "100%" }}
        />
      </div>
    </>
  );
}
