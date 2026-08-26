import { useEffect, useState } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Badge } from "primereact/badge";
import { Button } from "primereact/button";
import { useNavigate } from "react-router-dom";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import CategoryDialog from "./CategoryDialog";
import usePagination from "../../hooks/usePagination";
import { getCategories } from "../../services/categoryService";

export default function CategoryTable() {
  const navigate = useNavigate();
  const { lazyState, onPage, reset } = usePagination(10);

  const [categories, setCategories] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [sortField, setSortField] = useState("name");
  const [sortOrder, setSortOrder] = useState(1);
  const [search, setSearch] = useState("");
  const [dialogVisible, setDialogVisible] = useState(false);

  useEffect(() => {
    loadCategories();
  }, [lazyState, sortField, sortOrder, search]);

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
        onAdd={() => setDialogVisible(true)}
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
