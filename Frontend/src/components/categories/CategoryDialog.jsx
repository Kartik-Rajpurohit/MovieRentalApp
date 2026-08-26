import { useState, useEffect } from "react";
import { Dialog } from "primereact/dialog";
import { InputText } from "primereact/inputtext";
import { Button } from "primereact/button";
import { createCategory, updateCategory } from "../../services/categoryService";

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

export default function CategoryDialog({
  visible,
  onHide,
  onSuccess,
  mode = "add",
  category = null,
}) {
  const isEdit = mode === "edit";
  const [name, setName] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!visible) return;
    setName(isEdit && category ? category.name : "");
    setError("");
  }, [visible]);

  const handleSubmit = async () => {
    if (!name.trim()) {
      setError("Name is required");
      return;
    }

    setLoading(true);
    try {
      if (isEdit) {
        await updateCategory({ categoryId: category.categoryId, name });
      } else {
        await createCategory({ name });
      }
      onSuccess();
      onHide();
    } catch (err) {
      setError("Something went wrong.");
    } finally {
      setLoading(false);
    }
  };

  const footer = (
    <div style={{ display: "flex", gap: "8px", justifyContent: "flex-end" }}>
      <Button
        label="Cancel"
        icon="pi pi-times"
        outlined
        severity="secondary"
        onClick={onHide}
        disabled={loading}
      />
      <Button
        label={isEdit ? "Save Changes" : "Create"}
        icon="pi pi-check"
        onClick={handleSubmit}
        loading={loading}
      />
    </div>
  );

  return (
    <Dialog
      header={isEdit ? "Edit Category" : "Add Category"}
      visible={visible}
      onHide={onHide}
      footer={footer}
      style={{ width: "380px" }}
      modal
    >
      <div style={{ paddingTop: "8px" }}>
        <label style={labelStyle}>Name</label>
        <InputText
          value={name}
          onChange={(e) => {
            setName(e.target.value);
            setError("");
          }}
          placeholder="e.g. Action"
          style={{ width: "100%" }}
          className={error ? "p-invalid" : ""}
        />
        {error && <small className="p-error">{error}</small>}
      </div>
    </Dialog>
  );
}
