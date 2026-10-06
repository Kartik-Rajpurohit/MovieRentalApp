import { useEffect, useState } from "react";
import { Dropdown } from "primereact/dropdown";
import { AutoComplete } from "primereact/autocomplete";
import { getInventory } from "../../services/inventoryService";
import { getCustomers } from "../../services/customerService";
import { getStaff } from "../../services/staffService";

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

// Form fields for creating a new rental with server-side Inventory AutoComplete and Customer AutoComplete
export default function RentalFormFields({ form, setForm, errors }) {
  // Inventory AutoComplete state
  const [selectedInventory, setSelectedInventory] = useState(null);
  const [inventorySuggestions, setInventorySuggestions] = useState([]);

  // Customer AutoComplete state
  const [selectedCustomer, setSelectedCustomer] = useState(null);
  const [customerSuggestions, setCustomerSuggestions] = useState([]);

  // Store ID of the chosen inventory item used to filter customers and staff
  const [selectedStoreId, setSelectedStoreId] = useState(null);
  // Staff dropdown options for the selected store (small dataset: 1-2 staff members)
  const [staffList, setStaffList] = useState([]);

  // Clear selections when form is reset
  useEffect(() => {
    if (!form.inventoryId) {
      setSelectedInventory(null);
      setSelectedStoreId(null);
    }
    if (!form.customerId) {
      setSelectedCustomer(null);
    }
  }, [form.inventoryId, form.customerId]);

  // Load store-scoped staff when store ID is selected
  useEffect(() => {
    if (!selectedStoreId) {
      setStaffList([]);
      return;
    }
    getStaff(1, 10, "", true, selectedStoreId)
      .then((res) =>
        setStaffList(
          (res.data ?? []).map((s) => ({ label: s.fullName, value: s.staffId }))
        )
      )
      .catch(console.error);
  }, [selectedStoreId]);

  // Debounced server-side inventory search using GET /api/Inventory?page=1&pageSize=10&search={term}&isAvailable=true
  const searchInventory = async (event) => {
    const query = event.query?.trim();
    if (!query) {
      setInventorySuggestions([]);
      return;
    }
    try {
      const res = await getInventory({
        page: 1,
        pageSize: 10,
        search: query,
        isAvailable: true,
        sortBy: "movieTitle",
        sortOrder: "asc",
      });
      setInventorySuggestions(res.data ?? []);
    } catch (err) {
      console.error("Failed to search inventory:", err);
      setInventorySuggestions([]);
    }
  };

  // Suggestion item template displaying:
  // #421 — Academy Dinosaur
  // Store #1 · Available
  const inventoryItemTemplate = (item) => (
    <div style={{ display: "flex", flexDirection: "column", gap: "2px" }}>
      <span style={{ fontWeight: 600, color: "#1f2937" }}>
        #{item.inventoryId} — {item.movieTitle}
      </span>
      <span style={{ fontSize: "12px", color: "#6b7280" }}>
        Store #{item.storeId} · Available
      </span>
    </div>
  );

  const handleInventoryChange = (e) => {
    const val = e.value;
    setSelectedInventory(val);
    if (val && typeof val === "object" && val.inventoryId) {
      setSelectedStoreId(val.storeId);
      setForm((p) => ({
        ...p,
        inventoryId: val.inventoryId,
        customerId: null,
        staffId: null,
      }));
      setSelectedCustomer(null);
    } else {
      setSelectedStoreId(null);
      setSelectedCustomer(null);
      setForm((p) => ({
        ...p,
        inventoryId: null,
        customerId: null,
        staffId: null,
      }));
    }
  };

  // Debounced server-side customer search using GET /api/Customer?page=1&pageSize=10&search={term}
  const searchCustomers = async (event) => {
    const query = event.query?.trim();
    if (!query) {
      setCustomerSuggestions([]);
      return;
    }
    try {
      const res = await getCustomers(1, 10, query, true, selectedStoreId);
      setCustomerSuggestions(res.data ?? []);
    } catch (err) {
      console.error("Failed to search customers:", err);
      setCustomerSuggestions([]);
    }
  };

  // Suggestion item template displaying:
  // Linda Smith
  // linda.smith@email.com · #482
  const customerItemTemplate = (item) => (
    <div style={{ display: "flex", flexDirection: "column", gap: "2px" }}>
      <span style={{ fontWeight: 600, color: "#1f2937" }}>{item.fullName}</span>
      <span style={{ fontSize: "12px", color: "#6b7280" }}>
        {item.email ? `${item.email} · ` : ""}#{item.customerId}
      </span>
    </div>
  );

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
      {/* Inventory AutoComplete — server-side search of available copies */}
      <div>
        <label style={labelStyle}>Inventory Item</label>
        <AutoComplete
          value={selectedInventory}
          suggestions={inventorySuggestions}
          completeMethod={searchInventory}
          field="movieTitle"
          delay={300}
          placeholder="Search by movie title or inventory ID..."
          itemTemplate={inventoryItemTemplate}
          onChange={handleInventoryChange}
          style={{ width: "100%" }}
          inputStyle={{ width: "100%" }}
          className={errors?.inventoryId ? "p-invalid" : ""}
        />
        {errors?.inventoryId && (
          <small className="p-error">{errors.inventoryId}</small>
        )}
      </div>

      {/* Customer AutoComplete — searched on typing via GET /api/Customer?page=1&pageSize=10&search=... */}
      <div>
        <label style={labelStyle}>Customer</label>
        <AutoComplete
          value={selectedCustomer}
          suggestions={customerSuggestions}
          completeMethod={searchCustomers}
          field="fullName"
          delay={300}
          placeholder={!selectedStoreId ? "Select an inventory copy first" : "Type to search customer..."}
          itemTemplate={customerItemTemplate}
          disabled={!selectedStoreId}
          onChange={(e) => {
            setSelectedCustomer(e.value);
            if (e.value && typeof e.value === "object" && e.value.customerId) {
              setForm((prev) => ({ ...prev, customerId: e.value.customerId }));
            } else {
              setForm((prev) => ({ ...prev, customerId: null }));
            }
          }}
          style={{ width: "100%" }}
          inputStyle={{ width: "100%" }}
          className={errors?.customerId ? "p-invalid" : ""}
        />
        {errors?.customerId && (
          <small className="p-error">{errors.customerId}</small>
        )}
      </div>

      {/* Staff — filtered by store of selected inventory */}
      <div>
        <label style={labelStyle}>Staff</label>
        <Dropdown
          value={form.staffId}
          options={staffList}
          onChange={(e) => setForm((p) => ({ ...p, staffId: e.value }))}
          placeholder={!selectedStoreId ? "Select an inventory copy first" : "Select staff"}
          style={{ width: "100%" }}
          disabled={!selectedStoreId}
          className={errors?.staffId ? "p-invalid" : ""}
        />
        {errors?.staffId && <small className="p-error">{errors.staffId}</small>}
      </div>
    </div>
  );
}
