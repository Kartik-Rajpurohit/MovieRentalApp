import { useEffect, useState, useContext } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Tag } from "primereact/tag";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import usePagination from "../../hooks/usePagination";
import { getRentals } from "../../services/rentalService";
import { Button } from "primereact/button";
import { Badge } from "primereact/badge";
import RentalFilterDialog from "./RentalFilterDialog";
import useDialog from "../../hooks/useDialog";
import FormDialog from "../common/FormDialog";
import RentalFormFields from "./RentalFormFields";
import { createRental } from "../../services/rentalService";
import { AuthContext } from "../../context/AuthContext";

/**
 * Main rental list table component.
 * Displays paginated rental records with status tags (Returned, Active, Overdue),
 * supports server-side sorting, searching, filtering, and creating new rentals.
 */
export default function RentalTable() {
  const { user } = useContext(AuthContext);
  const navigate = useNavigate();
  // Pagination state hook (first, rows, page)
  const { lazyState, onPage, reset } = usePagination(10);

  // Table records and total records count from backend
  const [rentals, setRentals] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  // Loading indicator for table queries
  const [loading, setLoading] = useState(false);
  // Search query text
  const [search, setSearch] = useState("");
  // Sort parameters (defaults to newest rental date first)
  const [sortField, setSortField] = useState("rentaldate");
  const [sortOrder, setSortOrder] = useState(-1);
  const INIT_FILTERS = { isReturned: null, customerId: null, staffId: null };
  // Active filters and filter dialog visibility
  const [filters, setFilters] = useState(INIT_FILTERS);
  const [filterVisible, setFilterVisible] = useState(false);
  // Add rental dialog visibility
  const addDialog = useDialog();
  // Form state for creating a new rental
  const [form, setForm] = useState({
    inventoryId: null,
    customerId: null,
    staffId: null,
  });
  // Saving indicator and form validation errors
  const [saving, setSaving] = useState(false);
  const [formErrors, setFormErrors] = useState({});

  // Validate and submit a new rental to the backend
  const handleAdd = async () => {
    const errs = {};
    if (!form.inventoryId) errs.inventoryId = "Inventory item is required";
    if (!form.customerId) errs.customerId = "Customer is required";
    if (!form.staffId) errs.staffId = "Staff is required";
    if (Object.keys(errs).length > 0) {
      setFormErrors(errs);
      return;
    }

    setSaving(true);
    try {
      await createRental({
        inventoryId: form.inventoryId,
        customerId: form.customerId,
        staffId: form.staffId,
      });
      addDialog.close();
      setForm({ inventoryId: null, customerId: null, staffId: null });
      setFormErrors({});
      loadRentals();
    } catch (err) {
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  // Reload rentals list when pagination, search, sort, or filters change
  useEffect(() => {
    loadRentals();
  }, [lazyState, search, sortField, sortOrder, filters]);

  // Fetch paginated rentals from API with role-based restrictions
  const loadRentals = async () => {
    setLoading(true);
    try {
      const res = await getRentals({
        page: lazyState.page + 1,
        pageSize: lazyState.rows,
        search,
        sortField,
        sortOrder: sortOrder === 1 ? "asc" : "desc",
        isReturned: filters.isReturned ?? undefined,
        customerId: user?.role === "Customer" ? user.customerId : (filters.customerId ?? undefined),
        staffId: filters.staffId ?? undefined,
      });
      setRentals(res.data ?? []);
      setTotalRecords(res.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };


  const onSort = (e) => {
    setSortField(e.sortField);
    setSortOrder(e.sortOrder);
    reset();
  };

  const statusBody = (r) => (
    <Tag
      value={r.isReturned ? "Returned" : "Active"}
      severity={r.isReturned ? "success" : "warning"}
    />
  );

  const dateBody = (val) => (val ? new Date(val).toLocaleDateString() : "—");

  return (
    <div>
      {/* Modal dialog to create a new rental */}
      <FormDialog
        visible={addDialog.visible}
        onHide={() => {
          addDialog.close();
          setForm({ inventoryId: null, customerId: null, staffId: null });
          setFormErrors({});
        }}
        title="Add Rental"
        onSubmit={handleAdd}
        loading={saving}
        submitLabel="Add Rental"
      >
        <RentalFormFields form={form} setForm={setForm} errors={formErrors} />
      </FormDialog>

      {/* Header with title and Add Rental button (for staff/admin) */}
      <PageHeader
        title={user?.role === "Customer" ? "My Rentals" : "Rentals"}
        onAdd={user?.role !== "Customer" ? addDialog.open : undefined}
        addLabel="Add Rental"
      />

      {/* Filter dialog for return status, customer ID, and staff ID */}
      <RentalFilterDialog
        visible={filterVisible}
        onHide={() => setFilterVisible(false)}
        filters={filters}
        onApply={(f) => {
          setFilters(f);
          reset();
        }}
      />
      {/* Search bar and filter dialog trigger */}
      <div style={{ display: "flex", gap: "12px", marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={(v) => {
            setSearch(v);
            reset();
          }}
          placeholder="Search by movie, customer or ID..."
        />
        <div style={{ position: "relative" }}>
          <Button
            label="Filters"
            icon="pi pi-sliders-h"
            outlined
            onClick={() => setFilterVisible(true)}
          />
          {[filters.isReturned, filters.customerId, filters.staffId].filter(
            (v) => v !== null && v !== undefined,
          ).length > 0 && (
            <Badge
              value={
                [
                  filters.isReturned,
                  filters.customerId,
                  filters.staffId,
                ].filter((v) => v !== null && v !== undefined).length
              }
              severity="danger"
              style={{ position: "absolute", top: "-8px", right: "-8px" }}
            />
          )}
        </div>
      </div>

      {/* Table displaying rentals with server-side pagination and sorting */}
      <DataTable
        value={rentals}
        paginator
        lazy
        loading={loading}

        first={lazyState.first}
        rows={lazyState.rows}
        totalRecords={totalRecords}
        onPage={onPage}
        onSort={onSort}
        sortField={sortField}
        sortOrder={sortOrder}
        rowsPerPageOptions={[5, 10, 20, 50]}
        emptyMessage="No rentals found."
        onRowClick={(e) => navigate(`/rentals/${e.data.rentalId}`)}
        rowClassName={() => "cursor-pointer"}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column
          field="rentalId"
          header="ID"
          sortable
          style={{ width: "80px" }}
        />
        <Column field="filmTitle" header="Movie" sortable />
        {user?.role !== "Customer" && (
          <Column
            field="customerName"
            header="Customer"
            body={(r) => (
              <span style={{ textTransform: "capitalize" }}>
                {r.customerName?.toLowerCase()}
              </span>
            )}
          />
        )}
        <Column
          field="staffName"
          header="Staff"
          body={(r) => (
            <span style={{ textTransform: "capitalize" }}>
              {r.staffName?.toLowerCase()}
            </span>
          )}
        />
        <Column
          field="rentalDate"
          header="Rented On"
          sortable
          body={(r) => dateBody(r.rentalDate)}
          style={{ width: "120px" }}
        />
        <Column
          field="returnDate"
          header="Returned On"
          body={(r) => dateBody(r.returnDate)}
          style={{ width: "130px" }}
        />
        <Column header="Status" style={{ width: "110px" }} body={statusBody} />
      </DataTable>
    </div>
  );
}
