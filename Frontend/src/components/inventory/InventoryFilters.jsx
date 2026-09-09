import { Dropdown } from "primereact/dropdown";
import { LABEL_STYLE } from "../../utils/constants";

// Filter dropdown options for inventory availability
const STATUS_OPTIONS = [
  { label: "All", value: null },
  { label: "Available", value: true },
  { label: "Rented", value: false },
];

// Filter dropdown options for rental store locations
const STORE_OPTIONS = [
  { label: "All Stores", value: null },
  { label: "Store 1", value: 1 },
  { label: "Store 2", value: 2 },
];

// Form fields for filtering inventory by store and availability status
export default function InventoryFilters({ filters, setFilter }) {
  return (
    <>
      <div>
        <label style={LABEL_STYLE}>Store</label>
        <Dropdown
          value={filters.storeId}
          options={STORE_OPTIONS}
          onChange={(e) => setFilter("storeId")(e.value)}
          placeholder="All Stores"
          style={{ width: "100%" }}
        />
      </div>
      <div>
        <label style={LABEL_STYLE}>Status</label>
        <Dropdown
          value={filters.isAvailable}
          options={STATUS_OPTIONS}
          onChange={(e) => setFilter("isAvailable")(e.value)}
          placeholder="All"
          style={{ width: "100%" }}
        />
      </div>
    </>
  );
}
