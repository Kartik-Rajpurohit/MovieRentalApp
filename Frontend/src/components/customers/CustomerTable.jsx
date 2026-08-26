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
import CustomerFilters from "./CustomerFilters";
import useDialog from "../../hooks/useDialog";
import usePagination from "../../hooks/usePagination";
import useFilters from "../../hooks/useFilters";
import { getCustomers } from "../../services/customerService";

const INIT_FILTERS = { name: "", isActive: null };

export default function CustomerTable() {
  const navigate = useNavigate();
  const filterDialog = useDialog();
  const { lazyState, onPage, reset } = usePagination(10);
  const { filters, setFilters, reset: resetFilters } = useFilters(INIT_FILTERS);
  const { filters: localFilters, setFilter: setLocalFilter, setFilters: setLocalFilters } = useFilters(INIT_FILTERS);

  const [customers, setCustomers] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState("");

  useEffect(() => {
    loadCustomers();
  }, [lazyState, search, filters]);

  const loadCustomers = async () => {
    setLoading(true);
    try {
      const res = await getCustomers(
        lazyState.page + 1,
        lazyState.rows,
        search,
        filters.isActive,
      );
      setCustomers(res.data ?? []);
      setTotalRecords(res.totalRecords);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const onSearchChange = (val) => {
    setSearch(val);
    reset();
  };

  const handleApply = () => {
    setFilters(localFilters);
    reset();
    filterDialog.close();
  };
  const handleClear = () => {
    resetFilters();
    setLocalFilters(INIT_FILTERS);
    reset();
    filterDialog.close();
  };

  const openFilter = () => {
    setLocalFilters(filters);
    filterDialog.open();
  };
  const activeCount = [
    filters.name,
    filters.isActive !== null ? "x" : "",
  ].filter(Boolean).length;

  return (
    <div>
      <FilterDialog
        visible={filterDialog.visible}
        onHide={filterDialog.close}
        title="Filter Customers"
        onApply={handleApply}
        onClear={handleClear}
      >
        <CustomerFilters filters={localFilters} setFilter={setLocalFilter} />
      </FilterDialog>

      <PageHeader title="Customers" onAdd={null} />

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
          placeholder="Search customers..."
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

      <DataTable
        value={customers}
        paginator
        lazy
        loading={loading}
        first={lazyState.first}
        rows={lazyState.rows}
        totalRecords={totalRecords}
        onPage={onPage}
        rowsPerPageOptions={[5, 10, 20]}
        emptyMessage="No customers found."
        onRowClick={(e) => navigate(`/customers/${e.data.customerId}`)}
        rowClassName={() => "cursor-pointer"}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column field="customerId" header="ID" style={{ width: "70px" }} />
        <Column field="fullName" header="Full Name" />
        <Column field="email" header="Email" />
        <Column
          field="storeId"
          header="Store"
          style={{ width: "100px" }}
          body={(r) => `Store ${r.storeId}`}
        />
        <Column
          field="createDate"
          header="Joined"
          style={{ width: "130px" }}
          body={(r) =>
            r.createDate ? new Date(r.createDate).toLocaleDateString() : "—"
          }
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
