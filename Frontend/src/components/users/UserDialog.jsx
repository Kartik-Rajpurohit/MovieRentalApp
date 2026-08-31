import { useEffect, useState } from "react";
import { Dialog } from "primereact/dialog";
import { InputText } from "primereact/inputtext";
import { Dropdown } from "primereact/dropdown";
import { Button } from "primereact/button";
import {
  updateUser,
  getRoles,
  getStores,
} from "../../services/userService";

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

// Roles that require a store assignment
const STORE_ROLES = ["Staff", "Customer"];

export default function UserDialog({ visible, onHide, onSuccess, user = null }) {
  const [form, setForm] = useState({});
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  const [roles, setRoles] = useState([]);
  const [stores, setStores] = useState([]);
  const [selectedRoleName, setSelectedRoleName] = useState(null);

  // ─── Dialog open hone par init ─────────────────────────────────────────────
  useEffect(() => {
    if (!visible) return;

    setForm({
      userId: user?.userId,
      firstName: user?.firstName ?? "",
      lastName: user?.lastName ?? "",
      roleId: user?.roleId ?? null,
      storeId: null,
      addressId: user?.addressId ?? null,
    });
    setErrors({});
    setSelectedRoleName(null);

    fetchRoles();
    fetchStores();
  }, [visible]);

  // ─── Fetchers ──────────────────────────────────────────────────────────────

  const fetchRoles = async () => {
    const data = await getRoles(1, 100);
    const mapped = data.map((r) => ({ label: r.name, value: r.id, name: r.name }));
    setRoles(mapped);

    // Pre-fill selectedRoleName from current user role — for store dropdown visibility
    if (user?.roleId) {
      const current = mapped.find((r) => r.value === user.roleId);
      setSelectedRoleName(current?.name ?? null);
    }
  };

  const fetchStores = async () => {
    const data = await getStores(1, 100);
    setStores(data.map((s) => ({ label: s.name, value: s.id })));
  };

  // ─── Handlers ──────────────────────────────────────────────────────────────

  const handleRoleChange = (roleId) => {
    const selected = roles.find((r) => r.value === roleId);
    setSelectedRoleName(selected?.name ?? null);
    setForm((prev) => ({ ...prev, roleId, storeId: null }));
    setErrors((prev) => ({ ...prev, roleId: undefined, storeId: undefined }));
  };

  const handleChange = (field) => (e) => {
    const value = e.target !== undefined ? e.target.value : e.value;
    setForm((prev) => ({ ...prev, [field]: value }));
    setErrors((prev) => ({ ...prev, [field]: undefined }));
  };

  // ─── Validation ────────────────────────────────────────────────────────────
  const validate = () => {
    const e = {};
    if (!form.firstName?.trim()) e.firstName = "First name is required";
    if (!form.lastName?.trim()) e.lastName = "Last name is required";
    if (!form.roleId) e.roleId = "Role is required";
    if (STORE_ROLES.includes(selectedRoleName) && !form.storeId)
      e.storeId = "Store is required for this role";
    return e;
  };

  // ─── Submit ────────────────────────────────────────────────────────────────
  const handleSubmit = async () => {
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    setLoading(true);
    try {
      await updateUser({
        userId: form.userId,
        firstName: form.firstName,
        lastName: form.lastName,
        roleId: form.roleId,
        addressId: form.addressId,
        storeId: form.storeId, // Role Staff/Customer hone par store assign hoga
      });

      setErrors({});
      onSuccess();
      onHide();
    } catch (error) {
      console.error(error);
      const msg = error?.response?.data ?? "Failed to update user.";
      setErrors({ submit: typeof msg === "string" ? msg : "Something went wrong." });
    } finally {
      setLoading(false);
    }
  };

  const handleHide = () => {
    setErrors({});
    onHide();
  };

  const needsStore = STORE_ROLES.includes(selectedRoleName);

  // ─── Footer ────────────────────────────────────────────────────────────────
  const footer = (
    <div style={{ display: "flex", gap: "8px", justifyContent: "flex-end" }}>
      <Button label="Cancel" icon="pi pi-times" severity="secondary" outlined onClick={handleHide} disabled={loading} />
      <Button label="Save Changes" icon="pi pi-check" onClick={handleSubmit} loading={loading} />
    </div>
  );

  // ─── Render ────────────────────────────────────────────────────────────────
  return (
    <Dialog
      header="Edit User"
      visible={visible}
      onHide={handleHide}
      footer={footer}
      style={{ width: "440px" }}
      modal
    >
      <div style={{ display: "flex", flexDirection: "column", gap: "16px", paddingTop: "8px" }}>

        {/* First Name */}
        <div>
          <label style={labelStyle}>First Name</label>
          <InputText
            value={form.firstName}
            onChange={handleChange("firstName")}
            placeholder="Enter first name"
            style={{ width: "100%" }}
            className={errors.firstName ? "p-invalid" : ""}
          />
          {errors.firstName && <small className="p-error">{errors.firstName}</small>}
        </div>

        {/* Last Name */}
        <div>
          <label style={labelStyle}>Last Name</label>
          <InputText
            value={form.lastName}
            onChange={handleChange("lastName")}
            placeholder="Enter last name"
            style={{ width: "100%" }}
            className={errors.lastName ? "p-invalid" : ""}
          />
          {errors.lastName && <small className="p-error">{errors.lastName}</small>}
        </div>

        {/* Role */}
        <div>
          <label style={labelStyle}>Role</label>
          <Dropdown
            value={form.roleId}
            options={roles}
            onChange={(e) => handleRoleChange(e.value)}
            placeholder="Select role"
            style={{ width: "100%" }}
            appendTo="self"
            className={errors.roleId ? "p-invalid" : ""}
          />
          {errors.roleId && <small className="p-error">{errors.roleId}</small>}
        </div>

        {/* Store — only when role is Staff or Customer */}
        {needsStore && (
          <div>
            <label style={labelStyle}>Store</label>
            <Dropdown
              value={form.storeId}
              options={stores}
              onChange={handleChange("storeId")}
              placeholder="Select store"
              style={{ width: "100%" }}
              appendTo="self"
              className={errors.storeId ? "p-invalid" : ""}
            />
            {errors.storeId && <small className="p-error">{errors.storeId}</small>}
          </div>
        )}

        {/* Submit Error */}
        {errors.submit && (
          <small className="p-error" style={{ textAlign: "center" }}>{errors.submit}</small>
        )}
      </div>
    </Dialog>
  );
}
