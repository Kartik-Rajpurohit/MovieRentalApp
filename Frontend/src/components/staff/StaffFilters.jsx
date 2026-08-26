import { InputText } from "primereact/inputtext";
import { Dropdown } from "primereact/dropdown";
import { LABEL_STYLE, STATUS_OPTIONS } from "../../utils/constants";

export default function StaffFilters({ filters, setFilter }) {
  return (
    <>
      <div>
        <label style={LABEL_STYLE}>Name</label>
        <InputText
          value={filters.name}
          onChange={(e) => setFilter("name")(e.target.value)}
          placeholder="Filter by name"
          style={{ width: "100%" }}
        />
      </div>

      <div>
        <label style={LABEL_STYLE}>Status</label>
        <Dropdown
          value={filters.isActive}
          options={STATUS_OPTIONS}
          onChange={(e) => setFilter("isActive")(e.value)}
          placeholder="All"
          style={{ width: "100%" }}
        />
      </div>
    </>
  );
}
