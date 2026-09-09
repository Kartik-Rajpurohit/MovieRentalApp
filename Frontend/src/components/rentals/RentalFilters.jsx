import { useContext } from "react";
import { InputText } from "primereact/inputtext";
import { Dropdown } from "primereact/dropdown";
import { LABEL_STYLE } from "../../utils/constants";
import { AuthContext } from "../../context/AuthContext";

// Status filter options: All, Active (not returned yet), or Returned
const RETURN_STATUS_OPTIONS = [
  { label: "All", value: null },
  { label: "Active", value: false },
  { label: "Returned", value: true },
];

// Form inputs for filtering rentals by status, and by customer/staff ID for admin/staff users.
export default function RentalFilters({ filters, setFilter }) {
  const { user } = useContext(AuthContext);

  return (
    <>
      {/* Return status dropdown: All / Active / Returned */}
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
      {/* Customer ID and Staff ID filters are only shown to Staff/Admin, not Customers */}
      {user?.role !== "Customer" && (

        <>
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
      )}
    </>
  );
}
