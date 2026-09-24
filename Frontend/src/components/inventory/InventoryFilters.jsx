import { useEffect, useState } from "react";
import { Dropdown } from "primereact/dropdown";
import { LABEL_STYLE } from "../../utils/constants";
import { getStores } from "../../services/storeService";

// Filter dropdown options for inventory availability
const STATUS_OPTIONS = [
  { label: "All", value: null },
  { label: "Available", value: true },
  { label: "Rented", value: false },
];

// Form fields for filtering inventory by store and availability status
export default function InventoryFilters({ filters, setFilter }) {
  const [storeOptions, setStoreOptions] = useState([
    { label: "All Stores", value: null },
  ]);
  const [loadingStores, setLoadingStores] = useState(false);

  useEffect(() => {
    setLoadingStores(true);
    getStores(1, 100)
      .then((res) => {
        const dynamicStores = (res.data ?? []).map((s) => ({
          label: s.cityName
            ? `Store #${s.storeId} — ${s.cityName}${s.street ? ` (${s.street})` : ""}`
            : `Store #${s.storeId}`,
          value: s.storeId,
        }));
        setStoreOptions([{ label: "All Stores", value: null }, ...dynamicStores]);
      })
      .catch((err) => {
        console.error("Failed to load stores for filter:", err);
      })
      .finally(() => setLoadingStores(false));
  }, []);

  return (
    <>
      <div>
        <label style={LABEL_STYLE}>Store</label>
        <Dropdown
          value={filters.storeId}
          options={storeOptions}
          onChange={(e) => setFilter("storeId")(e.value)}
          placeholder={loadingStores ? "Loading stores..." : "All Stores"}
          style={{ width: "100%" }}
          disabled={loadingStores}
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
