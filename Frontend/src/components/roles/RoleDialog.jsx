import { useState, useEffect } from "react";
import { Dialog } from "primereact/dialog";
import { InputText } from "primereact/inputtext";
import { Button } from "primereact/button";
import { createRole } from "../../services/roleService";

export default function RoleDialog({
  visible,
  onHide,
  onSuccess,
  mode = "add",
}) {
  const [roleName, setRoleName] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (visible) {
      setRoleName("");
      setError("");
    }
  }, [visible]);

  const handleSubmit = async () => {
    if (!roleName.trim()) {
      setError("Role name is required");
      return;
    }
    setLoading(true);
    try {
      await createRole({ roleName });
      onSuccess();
      onHide();
    } catch (err) {
      setError(err?.response?.data ?? "Something went wrong");
    } finally {
      setLoading(false);
    }
  };

  const footer = (
    <div style={{ display: "flex", gap: "8px", justifyContent: "flex-end" }}>
      <Button
        label="Cancel"
        icon="pi pi-times"
        severity="secondary"
        outlined
        onClick={onHide}
        disabled={loading}
      />
      <Button
        label="Create Role"
        icon="pi pi-check"
        onClick={handleSubmit}
        loading={loading}
      />
    </div>
  );

  return (
    <Dialog
      header="Add New Role"
      visible={visible}
      onHide={onHide}
      footer={footer}
      style={{ width: "380px" }}
      modal
    >
      <div style={{ paddingTop: "8px" }}>
        <label
          style={{ display: "block", marginBottom: "6px", fontWeight: 500 }}
        >
          Role Name
        </label>
        <InputText
          value={roleName}
          onChange={(e) => {
            setRoleName(e.target.value);
            setError("");
          }}
          placeholder="e.g. Manager"
          style={{ width: "100%" }}
          className={error ? "p-invalid" : ""}
        />
        {error && <small className="p-error">{error}</small>}
      </div>
    </Dialog>
  );
}
