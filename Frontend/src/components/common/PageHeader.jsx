import { Button } from "primereact/button";
import { useNavigate } from "react-router-dom";

export default function PageHeader({ title, addLabel, onAdd }) {
  const navigate = useNavigate();

  return (
    <div style={{ marginBottom: "24px" }}>
      {/* Breadcrumb */}
      <div style={{ marginBottom: "8px", fontSize: "13px", color: "#6b7280" }}>
        <span
          style={{ cursor: "pointer", color: "#6366f1" }}
          onClick={() => navigate("/")}
        >
          Dashboard
        </span>
        <span style={{ margin: "0 6px" }}>/</span>
        <span style={{ color: "#111827", fontWeight: 500 }}>{title}</span>
      </div>

      {/* Title + Add Button */}
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
        }}
      >
        <h1
          style={{
            margin: 0,
            fontSize: "26px",
            fontWeight: 700,
            color: "#111827",
          }}
        >
          {title}
        </h1>
        {onAdd && (
          <Button
            label={addLabel ?? `Add ${title}`}
            icon="pi pi-plus"
            onClick={onAdd}
          />
        )}
      </div>
    </div>
  );
}
