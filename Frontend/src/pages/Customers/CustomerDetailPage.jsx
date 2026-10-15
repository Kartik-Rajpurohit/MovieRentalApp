import { useParams } from "react-router-dom";
import { useState, useEffect } from "react";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { Button } from "primereact/button";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import { getCustomerById } from "../../services/customerService";
import { getRentals } from "../../services/rentalService";
import { FIELD_LABEL, FIELD_VALUE } from "../../utils/constants";

// Displays full profile, address details, and rental history with tab switching for a specific customer
export default function CustomerDetailPage() {
  // Read customer ID from the route parameters
  const { id } = useParams();
  // State for holding customer profile data
  const [customer, setCustomer] = useState(null);
  const [loading, setLoading] = useState(true);

  // Tab state for rental history (0: Active Rentals, 1: All History)
  const [activeTab, setActiveTab] = useState(0);
  const [rentals, setRentals] = useState([]);
  const [rentalsLoading, setRentalsLoading] = useState(false);

  // Fetch customer profile whenever customer ID changes
  useEffect(() => {
    let isCancelled = false;
    const controller = new AbortController();

    const fetchCustomer = async () => {
      setLoading(true);
      try {
        const data = await getCustomerById(id);
        if (!isCancelled) {
          setCustomer(data);
        }
      } catch (err) {
        if (!isCancelled) {
          console.error(err);
        }
      } finally {
        if (!isCancelled) {
          setLoading(false);
        }
      }
    };

    fetchCustomer();

    return () => {
      isCancelled = true;
      controller.abort();
    };
  }, [id]);

  // Fetch rentals whenever active tab or customer ID changes with AbortController to prevent race conditions
  useEffect(() => {
    let isCancelled = false;
    const controller = new AbortController();

    const fetchRentals = async () => {
      setRentalsLoading(true);
      try {
        const res = await getRentals(
          {
            customerId: id,
            isReturned: activeTab === 0 ? false : undefined,
            page: 1,
            pageSize: 20,
            sortField: "rentaldate",
            sortOrder: "desc",
          },
          { signal: controller.signal }
        );
        if (!isCancelled) {
          setRentals(res.data ?? []);
        }
      } catch (err) {
        if (!isCancelled && err.name !== "CanceledError" && err.name !== "AbortError") {
          console.error("Failed to load customer rentals:", err);
        }
      } finally {
        if (!isCancelled) {
          setRentalsLoading(false);
        }
      }
    };

    fetchRentals();

    return () => {
      isCancelled = true;
      controller.abort();
    };
  }, [id, activeTab]);

  // Show loading spinner while customer details are loading
  if (loading) {
    return (
      <AppLayout>
        <LoadingSpinner />
      </AppLayout>
    );
  }


  return (
    <AppLayout>
      <DetailPageHeader
        backPath="/customers"
        backLabel="Customers"
        title={customer?.fullName}
      />

      <Card>
        {/* Header */}
        <div style={{ display: "flex", alignItems: "center", gap: "16px", marginBottom: "32px" }}>
          <div style={{ width: "64px", height: "64px", borderRadius: "50%", background: "#d1fae5", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <i className="pi pi-users" style={{ fontSize: "1.8rem", color: "#10b981" }} />
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: "22px", fontWeight: 600 }}>{customer?.fullName}</h2>
            <span style={{ color: "#6b7280", fontSize: "14px" }}>{customer?.email}</span>
            <div style={{ marginTop: "6px" }}>
              <Tag value={customer?.isActive ? "Active" : "Inactive"} severity={customer?.isActive ? "success" : "danger"} />
            </div>
          </div>
        </div>

        {/* Info Grid */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))", gap: "24px", marginBottom: "32px" }}>
          <div><p style={FIELD_LABEL}>Customer ID</p><p style={FIELD_VALUE}>{customer?.customerId}</p></div>
          <div><p style={FIELD_LABEL}>Full Name</p><p style={FIELD_VALUE}>{customer?.fullName}</p></div>
          <div><p style={{ ...FIELD_LABEL }}>Email</p><p style={{ ...FIELD_VALUE, wordBreak: "break-all" }}>{customer?.email ?? "—"}</p></div>
          <div><p style={FIELD_LABEL}>Store</p><p style={FIELD_VALUE}>Store {customer?.storeId}</p></div>
          <div><p style={FIELD_LABEL}>Joined</p><p style={FIELD_VALUE}>{customer?.createDate ? new Date(customer.createDate).toLocaleDateString() : "—"}</p></div>
          <div>
            <p style={FIELD_LABEL}>Status</p>
            <Tag value={customer?.isActive ? "Active" : "Inactive"} severity={customer?.isActive ? "success" : "danger"} />
          </div>
        </div>

        {/* Address Section */}
        <div style={{ borderTop: "1px solid #e5e7eb", paddingTop: "24px" }}>
          <h3 style={{ margin: "0 0 20px 0", fontSize: "16px", fontWeight: 600, color: "#374151" }}>
            <i className="pi pi-map-marker" style={{ marginRight: "8px", color: "#10b981" }} />
            Address
          </h3>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))", gap: "24px" }}>
            <div><p style={FIELD_LABEL}>Street</p><p style={FIELD_VALUE}>{customer?.street ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>Postal Code</p><p style={FIELD_VALUE}>{customer?.postalCode ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>Phone</p><p style={FIELD_VALUE}>{customer?.phone ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>City</p><p style={FIELD_VALUE}>{customer?.cityName ?? "—"}</p></div>
            <div><p style={FIELD_LABEL}>Country</p><p style={FIELD_VALUE}>{customer?.countryName ?? "—"}</p></div>
          </div>
        </div>
      </Card>

      {/* Customer Rentals with Tab Switcher */}
      <Card
        title={
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "12px" }}>
            <span>Rental History</span>
            <div style={{ display: "flex", gap: "8px" }}>
              <Button
                label="Active Rentals"
                icon="pi pi-clock"
                size="small"
                severity={activeTab === 0 ? "primary" : "secondary"}
                outlined={activeTab !== 0}
                onClick={() => setActiveTab(0)}
                aria-label="Show active rentals"
              />
              <Button
                label="All History"
                icon="pi pi-history"
                size="small"
                severity={activeTab === 1 ? "primary" : "secondary"}
                outlined={activeTab !== 1}
                onClick={() => setActiveTab(1)}
                aria-label="Show all rental history"
              />
            </div>
          </div>
        }
        style={{ marginTop: "24px" }}
      >
        <DataTable
          value={rentals}
          loading={rentalsLoading}
          emptyMessage={activeTab === 0 ? "No active rentals for this customer." : "No rental history found."}
        >
          <Column field="rentalId" header="ID" style={{ width: "70px" }} />
          <Column field="movieTitle" header="Movie" style={{ minWidth: "180px" }} />
          <Column
            field="rentalDate"
            header="Rented On"
            body={(r) => (r.rentalDate ? new Date(r.rentalDate).toLocaleDateString() : "—")}
          />
          <Column
            field="returnDate"
            header="Returned On"
            body={(r) => (r.returnDate ? new Date(r.returnDate).toLocaleDateString() : "Active")}
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
          <Column
            field="totalPaid"
            header="Paid"
            body={(r) => (r.totalPaid !== undefined && r.totalPaid !== null ? `$${Number(r.totalPaid).toFixed(2)}` : "—")}
          />
        </DataTable>
      </Card>
    </AppLayout>
  );
}

