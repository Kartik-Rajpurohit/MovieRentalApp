import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import StatCard from "../common/StatCard";

export default function AdminDashboard({ stats }) {
  if (!stats) return null;

  const gridStyle = { display: "grid", gap: "16px", marginBottom: "24px" };

  return (
    <div>
      <h1 style={{ margin: "0 0 24px 0", fontSize: "24px", fontWeight: 700 }}>
        Admin Dashboard
      </h1>

      {/* Row 1 — Key Numbers */}
      <div style={{ ...gridStyle, gridTemplateColumns: "repeat(4, 1fr)" }}>
        <StatCard
          icon="pi-users"
          label="Total Users"
          value={stats.totalUsers}
          color="#6366f1"
        />
        <StatCard
          icon="pi-video"
          label="Total Movies"
          value={stats.totalFilms}
          color="#3b82f6"
        />
        <StatCard
          icon="pi-receipt"
          label="Total Rentals"
          value={stats.totalRentals}
          color="#f59e0b"
        />
        <StatCard
          icon="pi-dollar"
          label="Total Revenue"
          value={`$${stats.totalRevenue?.toFixed(2)}`}
          color="#10b981"
        />
      </div>

      {/* Row 2 — Current Status */}
      <div style={{ ...gridStyle, gridTemplateColumns: "repeat(4, 1fr)" }}>
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
          icon="pi-id-card"
          label="Total Staff"
          value={stats.totalStaff}
          color="#3b82f6"
        />
      </div>

      {/* Row 3 — Lists */}
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "1fr 1fr",
          gap: "16px",
          marginBottom: "24px",
        }}
      >
        {/* Top 5 Movies */}
        <Card title="Top 5 Most Rented Movies">
          <DataTable value={stats.topRentedFilms} emptyMessage="No data.">
            <Column field="title" header="Movie" />
            <Column
              field="rentalCount"
              header="Rentals"
              style={{ width: "90px" }}
            />
          </DataTable>
        </Card>

        {/* Revenue by Store */}
        <Card title="Revenue by Store">
          <DataTable value={stats.revenueByStore} emptyMessage="No data.">
            <Column field="storeName" header="Store" />
            <Column
              field="revenue"
              header="Revenue"
              body={(r) => (
                <span style={{ color: "#16a34a", fontWeight: 600 }}>
                  ${r.revenue?.toFixed(2)}
                </span>
              )}
            />
          </DataTable>
        </Card>
      </div>

      {/* Recent Rentals */}
      <Card title="Recent Rentals">
        <DataTable value={stats.recentRentals} emptyMessage="No data.">
          <Column field="rentalId" header="ID" style={{ width: "70px" }} />
          <Column field="filmTitle" header="Movie" />
          <Column field="customerName" header="Customer" />
          <Column
            field="rentalDate"
            header="Date"
            body={(r) => new Date(r.rentalDate).toLocaleDateString()}
          />
          <Column
            field="isReturned"
            header="Status"
            body={(r) => (
              <Tag
                value={r.isReturned ? "Returned" : "Active"}
                severity={r.isReturned ? "success" : "warning"}
              />
            )}
          />
        </DataTable>
      </Card>
    </div>
  );
}
