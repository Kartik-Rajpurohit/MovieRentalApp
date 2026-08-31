import { Dropdown } from "primereact/dropdown";
import { LABEL_STYLE } from "../../../utils/constants";

export default function CityFilters({ filters, setFilter, countries }) {
  return (
    <div>
      <label style={LABEL_STYLE}>Country</label>
      <Dropdown
        value={filters.countryId}
        options={countries}
        onChange={(e) => setFilter("countryId")(e.value)}
        placeholder="All Countries"
        style={{ width: "100%" }}
        filter
      />
    </div>
  );
}
