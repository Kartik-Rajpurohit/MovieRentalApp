import { useState, useEffect } from "react";
import { Dialog } from "primereact/dialog";
import { InputText } from "primereact/inputtext";
import { Button } from "primereact/button";
import { createLanguage, updateLanguage } from "../../services/languageService";

const labelStyle = {
  display: "block", marginBottom: "6px",
  fontWeight: "500", fontSize: "14px", color: "#374151",
};

export default function LanguageDialog({ visible, onHide, onSuccess, mode = "add", language = null }) {
  const isEdit = mode === "edit";
  const [name, setName] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  // Sync form when dialog opens
  useEffect(() => {
    if (!visible) return;
    setName(isEdit && language ? language.name : "");
    setError("");
  }, [visible]);

  const handleSubmit = async () => {
    if (!name.trim()) { setError("Name is required"); return; }

    setLoading(true);
    try {
      if (isEdit) {
        await updateLanguage({ languageId: language.languageId, name });
      } else {
        await createLanguage({ name });
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
      <Button label="Cancel" icon="pi pi-times" outlined severity="secondary" onClick={onHide} disabled={loading} />
      <Button label={isEdit ? "Save Changes" : "Create"} icon="pi pi-check" onClick={handleSubmit} loading={loading} />
    </div>
  );

  return (
    <Dialog
      header={isEdit ? "Edit Language" : "Add Language"}
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
          onChange={(e) => { setName(e.target.value); setError(""); }}
          placeholder="e.g. English"
          style={{ width: "100%" }}
          className={error ? "p-invalid" : ""}
        />
        {error && <small className="p-error">{error}</small>}
      </div>
    </Dialog>
  );
}
