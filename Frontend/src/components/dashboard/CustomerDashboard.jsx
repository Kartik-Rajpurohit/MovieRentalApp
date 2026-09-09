import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Card } from "primereact/card";
import StatCard from "../common/StatCard";

export default function CustomerDashboard({ stats }) {
  if (!stats) return null;

  return (
    <div>
      <h1 style={{ margin: "0 0 24px 0", fontSize: "24px", fontWeight: 700 }}>
        My Dashboard
      </h1>

      {/* Row 1 — Stats */}
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(3, 1fr)",
          gap: "16px",
          marginBottom: "24px",
        }}
      >
        <StatCard
          icon="pi-clock"
          label="Active Rentals"
          value={stats.activeRentals}
          color="#f59e0b"
        />
        <StatCard
          icon="pi-receipt"
          label="Total Rentals"
          value={stats.totalRentals}
          color="#3b82f6"
        />
        <StatCard
          icon="pi-dollar"
          label="Total Spent"
          value={`$${stats.totalSpent?.toFixed(2)}`}
          color="#10b981"
        />
      </div>

      {/* Active Rentals List */}
      <Card title="My Active Rentals">
        <DataTable
          value={stats.myActiveRentals}
          emptyMessage="No active rentals."
        >
          <Column field="rentalId" header="ID" style={{ width: "70px" }} />
          <Column field="filmTitle" header="Movie" />
          <Column
            field="rentalDate"
            header="Rented On"
            body={(r) => new Date(r.rentalDate).toLocaleDateString()}
          />
          <Column
            field="suggestedAmount"
            header="Est. Amount"
            body={(r) => (
              <span style={{ color: "#f59e0b", fontWeight: 600 }}>
                ${r.suggestedAmount?.toFixed(2)}
              </span>
            )}
          />
        </DataTable>
      </Card>
    </div>
  );
}
