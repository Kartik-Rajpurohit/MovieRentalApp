import { useEffect, useState, useContext } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { useNavigate } from "react-router-dom";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import CategoryDialog from "./CategoryDialog";
import usePagination from "../../hooks/usePagination";
import { getCategories } from "../../services/categoryService";
import { AuthContext } from "../../context/AuthContext";

// Displays movie categories in a DataTable with search, server-side pagination, sorting, and add action
export default function CategoryTable() {
  const { user } = useContext(AuthContext);
  const navigate = useNavigate();
  // Server-side pagination hook
  const { lazyState, onPage, reset } = usePagination(10);

  // Table records and total records state
  const [categories, setCategories] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [sortField, setSortField] = useState("name");
  const [sortOrder, setSortOrder] = useState(1);
  const [search, setSearch] = useState("");
  // Controls visibility of the Add Category modal dialog
  const [dialogVisible, setDialogVisible] = useState(false);

  // Reload categories whenever pagination, sorting, or search change
  useEffect(() => {
    loadCategories();
  }, [lazyState, sortField, sortOrder, search]);

  // Load paginated and sorted categories from the backend service
  const loadCategories = async () => {
    setLoading(true);
    try {
      const sortOrderStr = sortOrder === 1 ? "asc" : "desc";
      const res = await getCategories(
        lazyState.page + 1,
        lazyState.rows,
        search,
        sortField,
        sortOrderStr,
      );
      setCategories(res.data ?? []);
      setTotalRecords(res.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  // Handle column header click to toggle sort field/order
  const onSort = (e) => {
    setSortField(e.sortField);
    setSortOrder(e.sortOrder);
    reset();
  };


  return (
    <div>
      <PageHeader
        title="Categories"
        addLabel="Add Category"
        onAdd={user?.role !== "Customer" ? () => setDialogVisible(true) : undefined}
      />

      <div style={{ display: "flex", gap: "12px", marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={(v) => {
            setSearch(v);
            reset();
          }}
          placeholder="Search categories..."
        />
      </div>

      <CategoryDialog
        visible={dialogVisible}
        onHide={() => setDialogVisible(false)}
        onSuccess={loadCategories}
        mode="add"
      />

      <DataTable
        value={categories}
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
        emptyMessage="No categories found."
        onRowClick={(e) => navigate(`/categories/${e.data.categoryId}`)}
        rowClassName={() => "cursor-pointer"}
      >
        <Column field="name" header="Name" sortable />
        <Column field="filmCount" header="Movies" style={{ width: "100px" }} />
        <Column
          field="lastUpdate"
          header="Last Updated"
          style={{ width: "160px" }}
          body={(r) => new Date(r.lastUpdate).toLocaleDateString()}
        />
      </DataTable>
    </div>
  );
}
