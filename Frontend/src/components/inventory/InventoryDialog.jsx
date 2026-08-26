import { useState, useEffect } from "react";
import { Dialog } from "primereact/dialog";
import { InputNumber } from "primereact/inputnumber";
import { Button } from "primereact/button";
import { createInventory, updateInventory } from "../../services/inventoryService";

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

const emptyForm = { filmId: null, storeId: null };

export default function InventoryDialog({
  visible,
  onHide,
  onSuccess,
  mode = "add",
  inventory = null,
}) {
  const isEdit = mode === "edit";
  const [form, setForm] = useState(emptyForm);
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!visible) return;
    if (isEdit && inventory) {
      setForm({
        inventoryId: inventory.inventoryId,
        filmId: inventory.filmId,
        storeId: inventory.storeId,
      });
    } else {
      setForm(emptyForm);
    }
    setErrors({});
  }, [visible]);

  const validate = () => {
    const e = {};
    if (!isEdit && !form.filmId) e.filmId = "Film ID is required";
    if (!form.storeId) e.storeId = "Store ID is required";
    return e;
  };

  const handleSubmit = async () => {
    const errs = validate();
    if (Object.keys(errs).length > 0) {
      setErrors(errs);
      return;
    }
    setLoading(true);
    try {
      if (isEdit) {
        await updateInventory({ inventoryId: form.inventoryId, storeId: form.storeId });
      } else {
        await createInventory({ filmId: form.filmId, storeId: form.storeId });
      }
      onSuccess();
      onHide();
    } catch (err) {
      setErrors({ submit: err?.response?.data ?? "Something went wrong." });
    } finally {
      setLoading(false);
    }
  };

  const handleHide = () => {
    setForm(emptyForm);
    setErrors({});
    onHide();
  };

  const footer = (
    <div style={{ display: "flex", gap: "8px", justifyContent: "flex-end" }}>
      <Button
        label="Cancel"
        icon="pi pi-times"
        severity="secondary"
        outlined
        onClick={handleHide}
        disabled={loading}
      />
      <Button
        label={isEdit ? "Save Changes" : "Add Copy"}
        icon="pi pi-check"
        onClick={handleSubmit}
        loading={loading}
      />
    </div>
  );

  return (
    <Dialog
      header={isEdit ? "Edit Inventory" : "Add Inventory Copy"}
      visible={visible}
      onHide={handleHide}
      footer={footer}
      style={{ width: "400px" }}
      modal
    >
      <div style={{ display: "flex", flexDirection: "column", gap: "16px", paddingTop: "8px" }}>

        {/* Film ID — only in add mode */}
        {!isEdit && (
          <div>
            <label style={labelStyle}>Film ID</label>
            <InputNumber
              value={form.filmId}
              onValueChange={(e) => setForm((prev) => ({ ...prev, filmId: e.value }))}
              placeholder="Enter film ID"
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
              className={errors.filmId ? "p-invalid" : ""}
              min={1}
            />
            {errors.filmId && <small className="p-error">{errors.filmId}</small>}
          </div>
        )}

        {/* Store ID */}
        <div>
          <label style={labelStyle}>Store ID</label>
          <InputNumber
            value={form.storeId}
            onValueChange={(e) => setForm((prev) => ({ ...prev, storeId: e.value }))}
            placeholder="Enter store ID"
            style={{ width: "100%" }}
            inputStyle={{ width: "100%" }}
            className={errors.storeId ? "p-invalid" : ""}
            min={1}
          />
          {errors.storeId && <small className="p-error">{errors.storeId}</small>}
        </div>

        {/* Submit Error */}
        {errors.submit && (
          <small className="p-error" style={{ textAlign: "center" }}>
            {errors.submit}
          </small>
        )}
      </div>
    </Dialog>
  );
}
