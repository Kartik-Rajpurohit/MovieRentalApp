import { Card } from "primereact/card";

// Reusable card component for displaying metric statistics (icon, label, and value) on dashboards
export default function StatCard({ icon, label, value, color = "#6366f1" }) {
  return (
    <Card>
      <div style={{ display: "flex", alignItems: "center", gap: "16px" }}>
        <div
          style={{
            width: "48px",
            height: "48px",
            borderRadius: "12px",
            background: `${color}20`,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            flexShrink: 0,
          }}
        >
          <i className={`pi ${icon}`} style={{ fontSize: "1.4rem", color }} />
        </div>
        <div>
          <p style={{ margin: 0, fontSize: "13px", color: "#6b7280" }}>
            {label}
          </p>
          <p
            style={{
              margin: 0,
              fontSize: "22px",
              fontWeight: 700,
              color: "#111827",
            }}
          >
            {value}
          </p>
        </div>
      </div>
    </Card>
  );
}
