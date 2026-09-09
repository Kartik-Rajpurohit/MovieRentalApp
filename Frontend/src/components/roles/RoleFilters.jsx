import { InputText } from "primereact/inputtext";
import { LABEL_STYLE } from "../../utils/constants";

// Form inputs for filtering roles by name.
export default function RoleFilters({ filters, setFilter }) {
  return (
    <div>
      <label style={LABEL_STYLE}>Role Name</label>
      {/* Input to filter the list of roles by name */}
      <InputText
        value={filters.name}
        onChange={(e) => setFilter("name")(e.target.value)}
        placeholder="Filter by role name"
        style={{ width: "100%" }}
      />
    </div>
  );
}

