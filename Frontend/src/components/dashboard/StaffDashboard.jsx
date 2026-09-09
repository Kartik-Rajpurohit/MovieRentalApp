import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Card } from "primereact/card";
import StatCard from "../common/StatCard";

export default function StaffDashboard({ stats }) {
  if (!stats) return null;

  return (
    <div>
      <h1 style={{ margin: "0 0 4px 0", fontSize: "24px", fontWeight: 700 }}>
        Staff Dashboard
      </h1>
      <p style={{ margin: "0 0 24px 0", color: "#6b7280" }}>
        Store #{stats.storeId}
      </p>

      {/* Row 1 — Stats */}
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(4, 1fr)",
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
          icon="pi-box"
          label="Available Inventory"
          value={stats.availableInventory}
          color="#10b981"
        />
        <StatCard
          icon="pi-user"
          label="Total Customers"
          value={stats.totalCustomers}
          color="#6366f1"
        />
        <StatCard
          icon="pi-dollar"
          label="Today's Payments"
          value={`$${stats.todaysPayments?.toFixed(2)}`}
          color="#10b981"
        />
      </div>

      {/* Row 2 — Lists */}
      <div
        style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "16px" }}
      >
        <Card title="Recent Rentals">
          <DataTable value={stats.recentRentals} emptyMessage="No rentals.">
            <Column field="rentalId" header="ID" style={{ width: "60px" }} />
            <Column field="filmTitle" header="Movie" />
            <Column field="customerName" header="Customer" />
          </DataTable>
        </Card>

        <Card title="Recent Payments">
          <DataTable value={stats.recentPayments} emptyMessage="No payments.">
            <Column field="paymentId" header="ID" style={{ width: "60px" }} />
            <Column field="customerName" header="Customer" />
            <Column
              field="amount"
              header="Amount"
              body={(r) => (
                <span style={{ color: "#16a34a", fontWeight: 600 }}>
                  ${r.amount?.toFixed(2)}
                </span>
              )}
            />
          </DataTable>
        </Card>
      </div>
    </div>
  );
}
