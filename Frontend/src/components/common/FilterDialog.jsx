import { Dialog } from "primereact/dialog";
import { Button } from "primereact/button";

export default function FilterDialog({
  visible,
  onHide,
  title = "Filters",
  onApply,
  onClear,
  children,
}) {
  const footer = (
    <div style={{ display: "flex", gap: "8px", justifyContent: "flex-end" }}>
      <Button
        label="Clear"
        icon="pi pi-times"
        severity="secondary"
        outlined
        onClick={onClear}
      />
      <Button label="Apply" icon="pi pi-check" onClick={onApply} />
    </div>
  );

  return (
    <Dialog
      header={title}
      visible={visible}
      onHide={onHide}
      footer={footer}
      style={{ width: "380px" }}
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
