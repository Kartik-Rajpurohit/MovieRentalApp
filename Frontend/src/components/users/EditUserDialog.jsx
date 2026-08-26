import { useState, useEffect } from "react";
import { Dialog } from "primereact/dialog";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { Dropdown } from "primereact/dropdown";
import { Button } from "primereact/button";
import { updateUser } from "../../services/userService";

const roleOptions = [
  { label: "Admin", value: "admin" },
  { label: "User", value: "user" },
  { label: "Producer", value: "producer" },
  { label: "Designer", value: "designer" },
  { label: "Director", value: "director" },
];

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

export default function EditUserDialog({ visible, onHide, user, onSuccess }) {
  const [form, setForm] = useState({});
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  // Jab dialog khule — user data se form fill karo
  useEffect(() => {
    if (visible && user) {
      setForm({ ...user, password: "" });
      setErrors({});
    }
  }, [visible, user]);

  const handleChange = (field) => (e) => {
    const value = e.target !== undefined ? e.target.value : e.value;
    setForm((prev) => ({ ...prev, [field]: value }));
    setErrors((prev) => ({ ...prev, [field]: undefined }));
  };

  const validate = () => {
    const e = {};
    if (!form.name?.trim()) e.name = "Name is required";
    if (!form.email?.trim()) e.email = "Email is required";
    else if (!/\S+@\S+\.\S+/.test(form.email)) e.email = "Invalid email";
    if (form.password && form.password.length < 6)
      e.password = "Min 6 characters";
    if (!form.role) e.role = "Role is required";
    return e;
  };

  const handleSubmit = async () => {
    const e = validate();
    if (Object.keys(e).length > 0) {
      setErrors(e);
      return;
    }

    setLoading(true);
    try {
      await updateUser(form);
      onSuccess();
      onHide();
    } catch (err) {
      setErrors({ submit: "Failed to update user." });
    } finally {
      setLoading(false);
    }
  };

  const handleHide = () => {
    setErrors({});
    onHide();
  };

  const footer = (
    <div style={{ display: "flex", gap: "8px", justifyContent: "flex-end" }}>
      <Button
        label="Cancel"
        icon="pi pi-times"
        outlined
        onClick={handleHide}
        disabled={loading}
      />
      <Button
        label="Save Changes"
        icon="pi pi-check"
        onClick={handleSubmit}
        loading={loading}
      />
    </div>
  );

  return (
    <Dialog
      header="Edit User"
      visible={visible}
      onHide={handleHide}
      footer={footer}
      style={{ width: "420px" }}
      modal
    >
      <div
        style={{
          display: "flex",
          flexDirection: "column",
          gap: "16px",
          paddingTop: "8px",
        }}
      >
        <div>
          <label style={labelStyle}>Name</label>
          <InputText
            value={form.name || ""}
            onChange={handleChange("name")}
            placeholder="Enter name"
            style={{ width: "100%" }}
            className={errors.name ? "p-invalid" : ""}
          />
          {errors.name && <small className="p-error">{errors.name}</small>}
        </div>

        <div>
          <label style={labelStyle}>Email</label>
          <InputText
            value={form.email || ""}
            onChange={handleChange("email")}
            placeholder="Enter email"
            style={{ width: "100%" }}
            className={errors.email ? "p-invalid" : ""}
          />
          {errors.email && <small className="p-error">{errors.email}</small>}
        </div>

        <div>
          <label style={labelStyle}>
            New Password{" "}
            <span style={{ color: "#9ca3af", fontWeight: 400 }}>
              (optional)
            </span>
          </label>
          <Password
            value={form.password || ""}
            onChange={handleChange("password")}
            placeholder="Leave blank to keep current"
            style={{ width: "100%" }}
            inputStyle={{ width: "100%" }}
            className={errors.password ? "p-invalid" : ""}
            feedback={false}
            toggleMask
          />
          {errors.password && (
            <small className="p-error">{errors.password}</small>
          )}
        </div>

        <div>
          <label style={labelStyle}>Role</label>
          <Dropdown
            value={form.role}
            options={roleOptions}
            onChange={handleChange("role")}
            placeholder="Select role"
            style={{ width: "100%" }}
            className={errors.role ? "p-invalid" : ""}
          />
          {errors.role && <small className="p-error">{errors.role}</small>}
        </div>

        {errors.submit && (
          <small className="p-error" style={{ textAlign: "center" }}>
            {errors.submit}
          </small>
        )}
      </div>
    </Dialog>
  );
}
