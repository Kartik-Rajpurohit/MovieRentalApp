import { useEffect, useState } from "react";
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

export default function PaymentTable() {
  const navigate = useNavigate();
  const { lazyState, onPage, reset } = usePagination(10);

  const [payments, setPayments] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [sortField, setSortField] = useState("paymentdate");
  const [sortOrder, setSortOrder] = useState(-1);
  const [search, setSearch] = useState("");
  const INIT_FILTERS = {
    minAmount: null,
    maxAmount: null,
    fromDate: null,
    toDate: null,
  };
  const [filters, setFilters] = useState(INIT_FILTERS);
  const [filterVisible, setFilterVisible] = useState(false);

  useEffect(() => {
    loadPayments();
  }, [lazyState, sortField, sortOrder, search, filters]);

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
      <PageHeader title="Payments" />

      <PaymentFilterDialog
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
          placeholder="Search by film, customer..."
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

      {/* Table — row click navigates to detail */}
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
          header="Film"
          sortable
          style={{ minWidth: "180px" }}
        />
        <Column
          field="customerName"
          header="Customer"
          style={{ minWidth: "140px" }}
        />
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
