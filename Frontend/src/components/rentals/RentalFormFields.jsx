import { useEffect, useState } from "react";
import { Dropdown } from "primereact/dropdown";
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

export default function RentalFormFields({ form, setForm, errors }) {
  const [inventory, setInventory] = useState([]);
  const [invLoading, setInvLoading] = useState(false);
  const [selectedStoreId, setSelectedStoreId] = useState(null);
  const [customers, setCustomers] = useState([]);
  const [staffList, setStaffList] = useState([]);

  // Load all available inventory on mount — available copies are naturally small in number
  useEffect(() => {
    setInvLoading(true);
    getInventory({ page: 1, pageSize: 500, isAvailable: true })
      .then((res) =>
        setInventory(
          (res.data ?? []).map((i) => ({
            label: `#${i.inventoryId} — ${i.filmTitle} (Store ${i.storeId})`,
            value: i.inventoryId,
            storeId: i.storeId,
          }))
        )
      )
      .catch(console.error)
      .finally(() => setInvLoading(false));
  }, []);

  // Load customers + staff when store changes — staff/customers per store are also small
  useEffect(() => {
    if (!selectedStoreId) return;
    getCustomers(1, 500, "", true, selectedStoreId)
      .then((res) =>
        setCustomers(
          (res.data ?? []).map((c) => ({ label: c.fullName, value: c.customerId }))
        )
      )
      .catch(console.error);

    getStaff(1, 500, "", true, selectedStoreId)
      .then((res) =>
        setStaffList(
          (res.data ?? []).map((s) => ({ label: s.fullName, value: s.staffId }))
        )
      )
      .catch(console.error);
  }, [selectedStoreId]);

  const handleInventoryChange = (inventoryId) => {
    const selected = inventory.find((i) => i.value === inventoryId);
    const storeId = selected?.storeId ?? null;
    setSelectedStoreId(storeId);
    // Reset customer and staff when inventory changes
    setCustomers([]);
    setStaffList([]);
    setForm((p) => ({ ...p, inventoryId, customerId: null, staffId: null }));
  };

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
      {/* Inventory — all available copies, filter in dropdown */}
      <div>
        <label style={labelStyle}>Inventory Item</label>
        <Dropdown
          value={form.inventoryId}
          options={inventory}
          onChange={(e) => handleInventoryChange(e.value)}
          placeholder={invLoading ? "Loading..." : "Select available copy"}
          style={{ width: "100%" }}
          filter
          appendTo="self"
          className={errors?.inventoryId ? "p-invalid" : ""}
        />
        {errors?.inventoryId && (
          <small className="p-error">{errors.inventoryId}</small>
        )}
      </div>

      {/* Customer — filtered by store of selected inventory */}
      <div>
        <label style={labelStyle}>Customer</label>
        <Dropdown
          value={form.customerId}
          options={customers}
          onChange={(e) => setForm((p) => ({ ...p, customerId: e.value }))}
          placeholder={!selectedStoreId ? "Select inventory first" : "Select customer"}
          style={{ width: "100%" }}
          filter
          appendTo="self"
          disabled={!selectedStoreId}
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
          placeholder={!selectedStoreId ? "Select inventory first" : "Select staff"}
          style={{ width: "100%" }}
          filter
          appendTo="self"
          disabled={!selectedStoreId}
          className={errors?.staffId ? "p-invalid" : ""}
        />
        {errors?.staffId && <small className="p-error">{errors.staffId}</small>}
      </div>
    </div>
  );
}
