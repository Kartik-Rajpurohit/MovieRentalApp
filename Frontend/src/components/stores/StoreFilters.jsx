import { InputText } from "primereact/inputtext";
import { LABEL_STYLE } from "../../utils/constants";

// Form inputs for filtering stores by city and country.
export default function StoreFilters({ filters, setFilter }) {
  return (
    <>
      {/* City filter input */}
      <div>
        <label style={LABEL_STYLE}>City</label>

        <InputText
          value={filters.city}
          onChange={(e) => setFilter("city")(e.target.value)}
          placeholder="Filter by city"
          style={{ width: "100%" }}
        />
      </div>

      <div>
        <label style={LABEL_STYLE}>Country</label>
        <InputText
          value={filters.country}
          onChange={(e) => setFilter("country")(e.target.value)}
          placeholder="Filter by country"
          style={{ width: "100%" }}
        />
      </div>
    </>
  );
}
