import { InputText } from "primereact/inputtext";
import { Dropdown } from "primereact/dropdown";
import { LABEL_STYLE } from "../../utils/constants";

const RETURN_STATUS_OPTIONS = [
  { label: "All", value: null },
  { label: "Active", value: false },
  { label: "Returned", value: true },
];

export default function RentalFilters({ filters, setFilter }) {
  return (
    <>
      <div>
        <label style={LABEL_STYLE}>Return Status</label>
        <Dropdown
          value={filters.isReturned}
          options={RETURN_STATUS_OPTIONS}
          onChange={(e) => setFilter("isReturned")(e.value)}
          placeholder="All"
          style={{ width: "100%" }}
        />
      </div>
      <div>
        <label style={LABEL_STYLE}>Customer ID</label>
        <InputText
          value={filters.customerId ?? ""}
          onChange={(e) => setFilter("customerId")(e.target.value || null)}
          placeholder="Filter by customer ID"
          style={{ width: "100%" }}
          keyfilter="int"
        />
      </div>
      <div>
        <label style={LABEL_STYLE}>Staff ID</label>
        <InputText
          value={filters.staffId ?? ""}
          onChange={(e) => setFilter("staffId")(e.target.value || null)}
          placeholder="Filter by staff ID"
          style={{ width: "100%" }}
          keyfilter="int"
        />
      </div>
    </>
  );
}
