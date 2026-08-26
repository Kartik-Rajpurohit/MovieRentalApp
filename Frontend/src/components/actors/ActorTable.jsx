import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import { Badge } from "primereact/badge";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import usePagination from "../../hooks/usePagination";
import { getActors } from "../../services/actorService";

export default function ActorTable() {
  const navigate = useNavigate();
  const { lazyState, onPage, reset } = usePagination(10);

  const [actors, setActors] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState("actorId");
  const [sortOrder, setSortOrder] = useState(1);

  useEffect(() => {
    loadActors();
  }, [lazyState, search, sortField, sortOrder]);

  const loadActors = async () => {
    setLoading(true);
    try {
      const res = await getActors(
        lazyState.page + 1,
        lazyState.rows,
        search,
        sortField,
        sortOrder === 1 ? "asc" : "desc"
      );
      setActors(res.data ?? []);
      setTotalRecords(res.totalRecords ?? 0);
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

  const onSort = (e) => {
    setSortField(e.sortField);
    setSortOrder(e.sortOrder);
    reset();
  };

  return (
    <div>
      <PageHeader title="Actors" />

      <div style={{ display: "flex", alignItems: "center", gap: "12px", marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={onSearchChange}
          placeholder="Search actors..."
        />
      </div>

      <DataTable
        value={actors}
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
        rowsPerPageOptions={[5, 10, 20]}
        emptyMessage="No actors found."
        onRowClick={(e) => navigate(`/actors/${e.data.actorId}`)}
        rowClassName={() => "cursor-pointer"}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column field="actorId" header="ID" sortable style={{ width: "70px" }} />
        <Column
          field="fullName"
          header="Full Name"
          sortable
          body={(r) => `${r.firstName} ${r.lastName}`}
        />
        <Column field="filmCount" header="Films" sortable style={{ width: "100px" }} />
        <Column
          field="lastUpdate"
          header="Last Update"
          style={{ width: "140px" }}
          body={(r) => r.lastUpdate ? new Date(r.lastUpdate).toLocaleDateString() : "—"}
        />
      </DataTable>
    </div>
  );
}