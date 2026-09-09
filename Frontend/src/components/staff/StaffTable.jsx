import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import { Badge } from "primereact/badge";
import FilterDialog from "../common/FilterDialog";
import SearchBar from "../common/SearchBar";
import PageHeader from "../common/PageHeader";
import StatusTag from "../common/StatusTag";
import StaffFilters from "./StaffFilters";
import useDialog from "../../hooks/useDialog";
import usePagination from "../../hooks/usePagination";
import useFilters from "../../hooks/useFilters";
import { getStaff } from "../../services/staffService";

// Initial filter state for staff list
const INIT_FILTERS = { name: "", isActive: null };

// Displays the staff directory table with live search, active status filtering, and pagination.
export default function StaffTable() {
  const navigate = useNavigate();
  // Filter dialog visibility hook
  const filterDialog = useDialog();
  // Pagination state hook (first, rows, page)
  const { lazyState, onPage, reset } = usePagination(10);
  // Applied filters and local temporary filters for the modal dialog
  const { filters, setFilters, reset: resetFilters } = useFilters(INIT_FILTERS);
  const { filters: localFilters, setFilter: setLocalFilter, setFilters: setLocalFilters } = useFilters(INIT_FILTERS);

  // Staff records and total count from API
  const [staff, setStaff] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  // Loading indicator for table queries
  const [loading, setLoading] = useState(false);
  // Search query state
  const [search, setSearch] = useState("");

  // Reload staff data whenever pagination, search, or filters change
  useEffect(() => {
    loadStaff();
  }, [lazyState, search, filters]);

  // Fetch paginated staff members from the staff API service
  const loadStaff = async () => {
    setLoading(true);
    try {
      const res = await getStaff(
        lazyState.page + 1,
        lazyState.rows,
        search,
        filters.isActive,
      );
      setStaff(res.data ?? []);
      setTotalRecords(res.totalRecords);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  // Update search text and reset pagination to page 1
  const onSearchChange = (val) => {
    setSearch(val);
    reset();
  };

  // Apply selected filter options and close dialog
  const handleApply = () => {
    setFilters(localFilters);
    reset();
    filterDialog.close();
  };
  // Reset all filters to initial state and close dialog
  const handleClear = () => {
    resetFilters();
    setLocalFilters(INIT_FILTERS);
    reset();
    filterDialog.close();
  };

  // Open the filter modal dialog with the current filter values
  const openFilter = () => {
    setLocalFilters(filters);
    filterDialog.open();
  };

  // Count active filter criteria to display badge count
  const activeCount = [
    filters.name,
    filters.isActive !== null ? "x" : "",
  ].filter(Boolean).length;


  return (
    <div>
      {/* Modal dialog for staff name and active status filters */}
      <FilterDialog
        visible={filterDialog.visible}
        onHide={filterDialog.close}
        title="Filter Staff"
        onApply={handleApply}
        onClear={handleClear}
      >
        <StaffFilters filters={localFilters} setFilter={setLocalFilter} />
      </FilterDialog>

      {/* Header bar for staff section */}
      <PageHeader title="Staff" onAdd={null} />

      {/* Search and filter controls toolbar */}
      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: "12px",
          marginBottom: "16px",
        }}
      >
        <SearchBar
          value={search}
          onChange={onSearchChange}
          placeholder="Search staff..."
        />
        <div style={{ position: "relative" }}>
          <Button
            label="Filters"
            icon="pi pi-sliders-h"
            outlined
            onClick={openFilter}
          />
          {activeCount > 0 && (
            <Badge
              value={activeCount}
              severity="danger"
              style={{ position: "absolute", top: "-8px", right: "-8px" }}
            />
          )}
        </div>
      </div>

      {/* Table displaying staff members with server-side pagination */}
      <DataTable
        value={staff}
        paginator
        lazy
        loading={loading}

        first={lazyState.first}
        rows={lazyState.rows}
        totalRecords={totalRecords}
        onPage={onPage}
        rowsPerPageOptions={[5, 10, 20]}
        emptyMessage="No staff found."
        onRowClick={(e) => navigate(`/staff/${e.data.staffId}`)}
        rowClassName={() => "cursor-pointer"}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column field="staffId" header="ID" style={{ width: "70px" }} />
        <Column field="fullName" header="Full Name" />
        <Column field="email" header="Email" />
        <Column
          field="storeId"
          header="Store"
          style={{ width: "100px" }}
          body={(r) => `Store ${r.storeId}`}
        />
        <Column
          field="isActive"
          header="Status"
          style={{ width: "110px" }}
          body={(r) => <StatusTag isActive={r.isActive} />}
        />
      </DataTable>
    </div>
  );
}
