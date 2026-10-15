import { useContext, useCallback } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { useNavigate } from "react-router-dom";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import CategoryDialog from "./CategoryDialog";
import useListingResource from "../../hooks/useListingResource";
import { getCategories } from "../../services/categoryService";
import { AuthContext } from "../../context/AuthContext";

// Displays movie categories in a DataTable with search, server-side pagination, sorting, and add action
export default function CategoryTable() {
  const { user } = useContext(AuthContext);
  const navigate = useNavigate();

  const fetchFn = useCallback(
    ({ page, pageSize, search, sortField, sortOrder }) =>
      getCategories(page, pageSize, search, sortField, sortOrder),
    []
  );

  const {
    items: categories,
    totalRecords,
    loading,
    search,
    onSearchChange,
    sortField,
    sortOrder,
    onSort,
    lazyState,
    onPage,
    addDialog,
    reload,
  } = useListingResource({
    fetchFn,
    defaultSortField: "categoryId",
    defaultSortOrder: -1,
  });

  return (
    <div>
      <PageHeader
        title="Categories"
        addLabel="Add Category"
        onAdd={user?.role !== "Customer" ? () => addDialog.open() : undefined}
      />

      <div style={{ display: "flex", gap: "12px", marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={onSearchChange}
          placeholder="Search categories..."
        />
      </div>

      <CategoryDialog
        visible={addDialog.visible}
        onHide={() => addDialog.close()}
        onSuccess={reload}
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
        <Column field="movieCount" header="Movies" style={{ width: "100px" }} />
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
