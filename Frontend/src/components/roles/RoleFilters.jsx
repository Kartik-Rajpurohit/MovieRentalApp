import { InputText } from "primereact/inputtext";
import { LABEL_STYLE } from "../../utils/constants";

export default function RoleFilters({ filters, setFilter }) {
  return (
    <div>
      <label style={LABEL_STYLE}>Role Name</label>
      <InputText
        value={filters.name}
        onChange={(e) => setFilter("name")(e.target.value)}
        placeholder="Filter by role name"
        style={{ width: "100%" }}
      />
    </div>
  );
}
