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

const INIT_FILTERS = { customerId: null, staffId: null, rentalId: null };

export default function PaymentTable() {
  const navigate = useNavigate();
  const { lazyState, onPage, reset } = usePagination(10);

  const [payments, setPayments] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [sortField, setSortField] = useState("paymentdate");
  const [sortOrder, setSortOrder] = useState(-1);
  const [search, setSearch] = useState("");
  const [filters] = useState(INIT_FILTERS);

  useEffect(() => {
    loadPayments();
  }, [lazyState, sortField, sortOrder, search, filters]);

  const loadPayments = async () => {
    setLoading(true);
    try {
      const sortOrderStr = sortOrder === 1 ? "asc" : "desc";
      const res = await getPayments(
        lazyState.page + 1,
        lazyState.rows,
        sortField,
        sortOrderStr,
        search,
        filters.customerId,
        filters.staffId,
        filters.rentalId
      );
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

      {/* Toolbar */}
      <div style={{ display: "flex", gap: "12px", marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={(v) => { setSearch(v); reset(); }}
          placeholder="Search by film, customer..."
        />
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
        <Column field="paymentId" header="ID" sortable style={{ width: "80px" }} />
        <Column field="filmTitle" header="Film" sortable style={{ minWidth: "180px" }} />
        <Column field="customerName" header="Customer" style={{ minWidth: "140px" }} />
        <Column field="staffName" header="Processed By" style={{ minWidth: "140px" }} />
        <Column field="rentalId" header="Rental ID" style={{ width: "100px" }} />
        <Column field="amount" header="Amount" sortable style={{ width: "110px" }} body={amountBody} />
        <Column field="paymentDate" header="Date" sortable style={{ width: "130px" }} body={dateBody} />
      </DataTable>
    </div>
  );
}
