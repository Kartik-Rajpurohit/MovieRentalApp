import { InputText } from "primereact/inputtext";
import { Dropdown } from "primereact/dropdown";
import { LABEL_STYLE, STATUS_OPTIONS } from "../../utils/constants";

export default function UserFilters({ filters, setFilter, roles }) {
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
        <label style={LABEL_STYLE}>Email</label>
        <InputText
          value={filters.email}
          onChange={(e) => setFilter("email")(e.target.value)}
          placeholder="Filter by email"
          style={{ width: "100%" }}
        />
      </div>

      <div>
        <label style={LABEL_STYLE}>Role</label>
        <Dropdown
          value={filters.role}
          options={roles}
          onChange={(e) => setFilter("role")(e.value)}
          placeholder="All Roles"
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