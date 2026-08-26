import { Dialog } from "primereact/dialog";
import { Button } from "primereact/button";

export default function FormDialog({
  visible,
  onHide,
  title,
  onSubmit,
  loading = false,
  submitLabel = "Save",
  children,
}) {
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
        label={submitLabel}
        icon="pi pi-check"
        onClick={onSubmit}
        loading={loading}
      />
    </div>
  );

  return (
    <Dialog
      header={title}
      visible={visible}
      onHide={onHide}
      footer={footer}
      style={{ width: "440px" }}
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
        {children}
      </div>
    </Dialog>
  );
}
