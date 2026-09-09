import { useEffect, useState, useContext } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { useNavigate } from "react-router-dom";
import { Badge } from "primereact/badge";
import { Button } from "primereact/button";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import usePagination from "../../hooks/usePagination";
import { getPayments } from "../../services/paymentService";
import PaymentFilterDialog from "./PaymentFilterDialog";
import useDialog from "../../hooks/useDialog";
import FormDialog from "../common/FormDialog";
import PaymentFormFields from "./PaymentFormFields";
import { createPayment } from "../../services/paymentService";
import { AuthContext } from "../../context/AuthContext";

/**
 * Payment list table component.
 * Displays financial transactions for rentals, supports date/amount filtering,
 * search, sorting, and recording new payments against returned rentals.
 */
export default function PaymentTable() {
  const { user } = useContext(AuthContext);
  const navigate = useNavigate();
  // Pagination state hook (first, rows, page)
  const { lazyState, onPage, reset } = usePagination(10);

  // Table records and total records count from backend
  const [payments, setPayments] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  // Loading state during API fetch
  const [loading, setLoading] = useState(false);
  // Sort parameters (defaults to latest payment first)
  const [sortField, setSortField] = useState("paymentdate");
  const [sortOrder, setSortOrder] = useState(-1);
  // Search query state
  const [search, setSearch] = useState("");
  const INIT_FILTERS = {
    minAmount: null,
    maxAmount: null,
    fromDate: null,
    toDate: null,
  };
  // Filter state and filter dialog visibility
  const [filters, setFilters] = useState(INIT_FILTERS);
  const [filterVisible, setFilterVisible] = useState(false);
  // Add payment dialog state
  const addDialog = useDialog();
  // Form state for creating a new payment
  const [form, setForm] = useState({
    rentalId: null,
    customerId: null,
    customerName: "",
    staffId: null,
    staffName: "",
    amount: null,
  });
  // Saving indicator and form validation errors
  const [saving, setSaving] = useState(false);
  const [formErrors, setFormErrors] = useState({});

  // Validate and submit a new payment to the backend
  const handleAdd = async () => {
    const errs = {};
    if (!form.rentalId) errs.rentalId = "Rental is required";
    if (!form.amount || form.amount <= 0)
      errs.amount = "Valid amount is required";
    if (Object.keys(errs).length > 0) {
      setFormErrors(errs);
      return;
    }

    setSaving(true);
    try {
      await createPayment({
        rentalId: form.rentalId,
        customerId: form.customerId,
        staffId: form.staffId,
        amount: form.amount,
      });
      addDialog.close();
      setForm({
        rentalId: null,
        customerId: null,
        customerName: "",
        staffId: null,
        staffName: "",
        amount: null,
      });
      setFormErrors({});
      loadPayments();
    } catch (err) {
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  // Reload payment records whenever pagination, sorting, search, or filters change
  useEffect(() => {
    loadPayments();
  }, [lazyState, sortField, sortOrder, search, filters]);

  // Fetch paginated payments list from API with current filters and user role restrictions
  const loadPayments = async () => {

    setLoading(true);
    try {
      const sortOrderStr = sortOrder === 1 ? "asc" : "desc";
      const res = await getPayments({
        page: lazyState.page + 1,
        pageSize: lazyState.rows,
        sortField,
        sortOrder: sortOrderStr,
        search,
        customerId: user?.role === "Customer" ? user.customerId : undefined,
        minAmount: filters.minAmount ?? undefined,
        maxAmount: filters.maxAmount ?? undefined,
        fromDate: filters.fromDate ? filters.fromDate.toISOString() : undefined,
        toDate: filters.toDate ? filters.toDate.toISOString() : undefined,
      });
      setPayments(res.data ?? []);
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

  // Amount column — green bold text
  const amountBody = (row) => (
    <span style={{ color: "#16a34a", fontWeight: 600 }}>
      ${row.amount?.toFixed(2)}
    </span>
  );

  // Date column — formatted
  const dateBody = (row) => (
    <span>
      {row.paymentDate
        ? new Date(row.paymentDate).toLocaleDateString("en-US", {
            year: "numeric",
            month: "short",
            day: "numeric",
          })
        : "—"}
    </span>
  );

  return (
    <div>
      {/* Modal dialog to record a new payment */}
      <FormDialog
        visible={addDialog.visible}
        onHide={() => {
          addDialog.close();
          setForm({
            rentalId: null,
            customerId: null,
            customerName: "",
            staffId: null,
            amount: null,
          });
          setFormErrors({});
        }}
        title="Add Payment"
        onSubmit={handleAdd}
        loading={saving}
        submitLabel="Add Payment"
      >
        <PaymentFormFields form={form} setForm={setForm} errors={formErrors} />
      </FormDialog>

      {/* Header bar with title and Add Payment button (for staff/admin) */}
      <PageHeader
        title={user?.role === "Customer" ? "My Payments" : "Payments"}
        onAdd={user?.role !== "Customer" ? addDialog.open : undefined}
        addLabel="Add Payment"
      />

      {/* Filter dialog for amount and date range */}
      <PaymentFilterDialog
        visible={filterVisible}
        onHide={() => setFilterVisible(false)}
        filters={filters}
        onApply={(f) => {
          setFilters(f);
          reset();
        }}
      />

      {/* Toolbar with live search and filter dialog button */}
      <div style={{ display: "flex", gap: "12px", marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={(v) => {
            setSearch(v);
            reset();
          }}
          placeholder="Search by movie, customer..."
        />
        <div style={{ position: "relative" }}>
          <Button
            label="Filters"
            icon="pi pi-sliders-h"
            outlined
            onClick={() => setFilterVisible(true)}
          />
          {Object.values(filters).filter((v) => v !== null).length > 0 && (
            <Badge
              value={Object.values(filters).filter((v) => v !== null).length}
              severity="danger"
              style={{ position: "absolute", top: "-8px", right: "-8px" }}
            />
          )}
        </div>
      </div>

      {/* Table displaying payments with server pagination and sorting */}
      <DataTable
        value={payments}
        paginator
        lazy
        loading={loading}

        first={lazyState.first}
        rows={lazyState.rows}
        totalRecords={totalRecords}
        onPage={onPage}
        rowsPerPageOptions={[5, 10, 20, 50]}
        sortField={sortField}
        sortOrder={sortOrder}
        onSort={onSort}
        removableSort
        emptyMessage="No payments found."
        onRowClick={(e) => navigate(`/payments/${e.data.paymentId}`)}
        rowClassName={() => "cursor-pointer"}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column
          field="paymentId"
          header="ID"
          sortable
          style={{ width: "80px" }}
        />
        <Column
          field="filmTitle"
          header="Movie"
          sortable
          style={{ minWidth: "180px" }}
        />
        {user?.role !== "Customer" && (
          <Column
            field="customerName"
            header="Customer"
            style={{ minWidth: "140px" }}
          />
        )}
        <Column
          field="staffName"
          header="Processed By"
          style={{ minWidth: "140px" }}
        />
        <Column
          field="rentalId"
          header="Rental ID"
          style={{ width: "100px" }}
        />
        <Column
          field="amount"
          header="Amount"
          sortable
          style={{ width: "110px" }}
          body={amountBody}
        />
        <Column
          field="paymentDate"
          header="Date"
          sortable
          style={{ width: "130px" }}
          body={dateBody}
        />
      </DataTable>
    </div>
  );
}
