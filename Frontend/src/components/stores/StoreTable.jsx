import { useEffect, useState } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { useNavigate } from "react-router-dom";
import { Button } from "primereact/button";
import { Badge } from "primereact/badge";
import { Tag } from "primereact/tag";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import FilterDialog from "../common/FilterDialog";
import StoreFilters from "./StoreFilters";
import useDialog from "../../hooks/useDialog";
import usePagination from "../../hooks/usePagination";
import useFilters from "../../hooks/useFilters";
import { getStores } from "../../services/storeService";
import FormDialog from "../common/FormDialog";
import StoreFormFields from "./StoreFormFields";
import { createStore } from "../../services/storeService";

const INIT_FILTERS = { city: "", country: "" };

export default function StoreTable() {
  const navigate = useNavigate();
  const filterDialog = useDialog();
  const { lazyState, onPage, reset } = usePagination(10);
  const { filters, setFilters, reset: resetFilters } = useFilters(INIT_FILTERS);
  const {
    filters: localFilters,
    setFilter: setLocalFilter,
    setFilters: setLocalFilters,
  } = useFilters(INIT_FILTERS);

  const [stores, setStores] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [sortField, setSortField] = useState("storeid");
  const [sortOrder, setSortOrder] = useState(1);
  const [search, setSearch] = useState("");
  const addDialog = useDialog();
  const [form, setForm] = useState({ addressId: null, managerStaffId: null });
  const [saving, setSaving] = useState(false);
  const [formErrors, setFormErrors] = useState({});

  const handleAdd = async () => {
    const errs = {};
    if (!form.managerStaffId) errs.managerStaffId = "Manager is required";
    if (!form.addressId) errs.addressId = "Address is required";
    if (Object.keys(errs).length > 0) {
      setFormErrors(errs);
      return;
    }

    setSaving(true);
    try {
      await createStore({
        addressId: form.addressId,
        managerStaffId: form.managerStaffId,
      });
      addDialog.close();
      setForm({ addressId: null, managerStaffId: null });
      setFormErrors({});
      loadStores();
    } catch (err) {
      console.error(err);
    } finally {
      setSaving(false);
    }
  };
  useEffect(() => {
    loadStores();
  }, [lazyState, sortField, sortOrder, search, filters]);

  const loadStores = async () => {
    setLoading(true);
    try {
      const sortOrderStr = sortOrder === 1 ? "asc" : "desc";
      const res = await getStores(
        lazyState.page + 1,
        lazyState.rows,
        sortField,
        sortOrderStr,
        search,
        filters.city || null,
        filters.country || null,
      );
      setStores(res.data ?? []);
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

  const activeCount = [filters.city, filters.country].filter(Boolean).length;

  const statsBody = (row) => (
    <div style={{ display: "flex", gap: "6px" }}>
      <Tag value={`${row.totalStaff} Staff`} severity="info" />
      <Tag value={`${row.totalCustomers} Customers`} severity="secondary" />
      <Tag value={`${row.totalInventory} Inventory`} severity="success" />
    </div>
  );

  return (
    <div>
      <FilterDialog
        visible={filterDialog.visible}
        onHide={filterDialog.close}
        title="Filter Stores"
        onApply={handleApply}
        onClear={handleClear}
      >
        <StoreFilters filters={localFilters} setFilter={setLocalFilter} />
      </FilterDialog>
      <FormDialog
        visible={addDialog.visible}
        onHide={() => {
          addDialog.close();
          setForm({ addressId: null, managerStaffId: null });
          setFormErrors({});
        }}
        title="Add Store"
        onSubmit={handleAdd}
        loading={saving}
        submitLabel="Add Store"
      >
        <StoreFormFields form={form} setForm={setForm} errors={formErrors} />
      </FormDialog>

      <PageHeader title="Stores" onAdd={addDialog.open} addLabel="Add Store" />

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
          onChange={(v) => {
            setSearch(v);
            reset();
          }}
          placeholder="Search by city, country, manager..."
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
        value={stores}
        paginator
        lazy
        loading={loading}
        first={lazyState.first}
        rows={lazyState.rows}
        totalRecords={totalRecords}
        onPage={onPage}
        rowsPerPageOptions={[5, 10, 20]}
        sortField={sortField}
        sortOrder={sortOrder}
        onSort={onSort}
        removableSort
        emptyMessage="No stores found."
        onRowClick={(e) => navigate(`/stores/${e.data.storeId}`)}
        rowClassName={() => "cursor-pointer"}
        tableStyle={{ minWidth: "50rem", tableLayout: "auto" }}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column
          field="storeId"
          header="Store ID"
          sortable
          style={{ width: "100px" }}
          body={(r) => `Store #${r.storeId}`}
        />
        <Column
          field="managerName"
          header="Manager"
          style={{ minWidth: "150px" }}
        />
        <Column
          field="cityName"
          header="City"
          sortable
          style={{ width: "130px" }}
        />
        <Column
          field="countryName"
          header="Country"
          sortable
          style={{ width: "130px" }}
        />
        <Column field="street" header="Address" style={{ minWidth: "180px" }} />
        <Column field="phone" header="Phone" style={{ width: "130px" }} />
        <Column header="Stats" style={{ minWidth: "260px" }} body={statsBody} />
      </DataTable>
    </div>
  );
}
