import { Button } from "primereact/button";
import { useNavigate } from "react-router-dom";

// Reusable header for all detail pages
// Shows: back button, breadcrumb, title, subtitle, action buttons (edit/delete etc.)
export default function DetailPageHeader({
  backPath,           // e.g. "/categories"
  backLabel,          // e.g. "Categories"
  title,              // e.g. "Action"
  subtitle,           // e.g. "16 movies"
  actions,            // array of { label, icon, severity, outlined, onClick }
}) {
  const navigate = useNavigate();

  return (
    <div style={{ marginBottom: "24px" }}>
      {/* Back button — same across all detail pages */}
      <Button
        icon="pi pi-arrow-left"
        label={`Back to ${backLabel}`}
        text
        onClick={() => navigate(backPath)}
        style={{ paddingLeft: 0, marginBottom: "12px", color: "#6366f1" }}
      />

      {/* Breadcrumb */}
      <div style={{ marginBottom: "8px", fontSize: "13px", color: "#6b7280" }}>
        <span
          style={{ cursor: "pointer", color: "#6366f1" }}
          onClick={() => navigate(backPath)}
        >
          {backLabel}
        </span>
        <span style={{ margin: "0 6px" }}>/</span>
        <span style={{ color: "#111827", fontWeight: 500 }}>{title}</span>
      </div>

      {/* Title + Actions */}
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
        <div>
          <h1 style={{ margin: 0, fontSize: "26px", fontWeight: 700, color: "#111827" }}>
            {title}
          </h1>
          {subtitle && (
            <span style={{ color: "#6b7280", fontSize: "14px" }}>{subtitle}</span>
          )}
        </div>

        {actions && actions.length > 0 && (
          <div style={{ display: "flex", gap: "8px" }}>
            {actions.map((action, i) => (
              <Button
                key={i}
                label={action.label}
                icon={action.icon}
                severity={action.severity}
                outlined={action.outlined}
                onClick={action.onClick}
                loading={action.loading}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
