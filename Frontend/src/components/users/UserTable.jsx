import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import StatusTag from "../common/StatusTag";
import PageHeader from "../common/PageHeader";
import UserToolbar from "./UserToolbar";
import usePagination from "../../hooks/usePagination";
import { getUsers } from "../../services/userService";

// Default filter criteria for user accounts
const INIT_FILTERS = { name: "", email: "", role: null, isActive: null };

// Displays the paginated user accounts table with search, sorting, filtering, and row click navigation.
export default function UserTable() {
  const navigate = useNavigate();
  // Pagination state hook (first, rows, page)
  const { lazyState, onPage, reset } = usePagination(10);

  // User records and total count from API
  const [users, setUsers] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  // Loading indicator for table queries
  const [loading, setLoading] = useState(false);
  // Sort parameters
  const [sortField, setSortField] = useState("id");
  const [sortOrder, setSortOrder] = useState(-1);
  // Search query text
  const [search, setSearch] = useState("");
  // Active filter criteria
  const [filters, setFilters] = useState(INIT_FILTERS);

  // Reload user list whenever pagination, sorting, search, or filters change
  useEffect(() => {
    loadUsers();
  }, [lazyState, sortField, sortOrder, search, filters]);

  // Fetch paginated user records from the user API service
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

  // Handle column header click for sorting
  const onSort = (e) => {
    setSortField(e.sortField);
    setSortOrder(e.sortOrder);
    reset();
  };

  // Update search text and reset pagination to page 1
  const onSearchChange = (val) => {
    setSearch(val);
    reset();
  };
  // Apply new filter criteria and reset pagination to page 1
  const onFiltersChange = (f) => {
    setFilters(f);
    reset();
  };


  return (
    <div>
      {/* Header bar with title */}
      <PageHeader title="Users" />

      {/* Toolbar with search bar and filter dialog trigger */}
      <UserToolbar
        search={search}
        onSearchChange={onSearchChange}
        filters={filters}
        onFiltersChange={onFiltersChange}
      />

      {/* Table displaying user accounts with server pagination and sorting */}
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
