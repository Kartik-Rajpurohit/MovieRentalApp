import { useEffect, useState } from "react";
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

export default function RentalTable() {
  const navigate = useNavigate();
  const { lazyState, onPage, reset } = usePagination(10);

  const [rentals, setRentals] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState("rentaldate");
  const [sortOrder, setSortOrder] = useState(-1);
  const INIT_FILTERS = { isReturned: null, customerId: null, staffId: null };
  const [filters, setFilters] = useState(INIT_FILTERS);
  const [filterVisible, setFilterVisible] = useState(false);
  const addDialog = useDialog();
  const [form, setForm] = useState({
    inventoryId: null,
    customerId: null,
    staffId: null,
  });
  const [saving, setSaving] = useState(false);
  const [formErrors, setFormErrors] = useState({});

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

  useEffect(() => {
    loadRentals();
  }, [lazyState, search, sortField, sortOrder, filters]);

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
        customerId: filters.customerId ?? undefined,
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

      <PageHeader
        title="Rentals"
        onAdd={addDialog.open}
        addLabel="Add Rental"
      />

      <RentalFilterDialog
        visible={filterVisible}
        onHide={() => setFilterVisible(false)}
        filters={filters}
        onApply={(f) => {
          setFilters(f);
          reset();
        }}
      />
      <div style={{ display: "flex", gap: "12px", marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={(v) => {
            setSearch(v);
            reset();
          }}
          placeholder="Search by film, customer or ID..."
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
        <Column field="filmTitle" header="Film" sortable />
        <Column
          field="customerName"
          header="Customer"
          body={(r) => (
            <span style={{ textTransform: "capitalize" }}>
              {r.customerName?.toLowerCase()}
            </span>
          )}
        />
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
