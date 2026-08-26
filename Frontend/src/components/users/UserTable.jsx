import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import StatusTag from "../common/StatusTag";
import PageHeader from "../common/PageHeader";
import UserToolbar from "./UserToolbar";
import usePagination from "../../hooks/usePagination";
import { getUsers } from "../../services/userService";

const INIT_FILTERS = { name: "", email: "", role: null, isActive: null };

export default function UserTable() {
  const navigate = useNavigate();
  const { lazyState, onPage, reset } = usePagination(10);

  const [users, setUsers] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [sortField, setSortField] = useState("id");
  const [sortOrder, setSortOrder] = useState(-1);
  const [search, setSearch] = useState("");
  const [filters, setFilters] = useState(INIT_FILTERS);

  useEffect(() => {
    loadUsers();
  }, [lazyState, sortField, sortOrder, search, filters]);

  const loadUsers = async () => {
    setLoading(true);
    try {
      const backendSortField = sortField === "fullName" ? "name" : sortField;
      const sortOrderStr = sortField ? (sortOrder === 1 ? "asc" : "desc") : "";

      const res = await getUsers(
        lazyState.page + 1,
        lazyState.rows,
        backendSortField,
        sortOrderStr,
        filters.name,
        filters.email,
        filters.role,
        search,
        filters.isActive,
      );

      const mapped = (res.data ?? []).map((u) => ({
        ...u,
        fullName:
          u.fullName ?? `${u.firstName ?? ""} ${u.lastName ?? ""}`.trim(),
      }));
      setUsers(mapped);
      setTotalRecords(res.totalRecords);
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

  const onSearchChange = (val) => {
    setSearch(val);
    reset();
  };
  const onFiltersChange = (f) => {
    setFilters(f);
    reset();
  };

  return (
    <div>
      <PageHeader title="Users" />

      <UserToolbar
        search={search}
        onSearchChange={onSearchChange}
        filters={filters}
        onFiltersChange={onFiltersChange}
      />

      <DataTable
        value={users}
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
        emptyMessage="No users found."
        onRowClick={(e) => navigate(`/users/${e.data.userId}`)}
        rowClassName={() => "cursor-pointer"}
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
      >
        <Column field="fullName" header="Full Name" sortable />
        <Column field="email" header="Email" sortable />
        <Column
          field="role"
          header="Role"
          sortable
          style={{ width: "130px" }}
          body={(r) => (
            <span style={{ textTransform: "capitalize" }}>{r.roleName}</span>
          )}
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
