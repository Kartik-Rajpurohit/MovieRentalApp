import { useContext, useCallback } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { useNavigate } from "react-router-dom";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import LanguageDialog from "./LanguageDialog";
import useListingResource from "../../hooks/useListingResource";
import { getLanguages } from "../../services/languageService";
import { AuthContext } from "../../context/AuthContext";

// Displays the languages catalog in a DataTable with server-side pagination, search, and sorting
export default function LanguageTable() {
  const { user } = useContext(AuthContext);
  const navigate = useNavigate();

  const fetchFn = useCallback(
    ({ page, pageSize, search, sortField, sortOrder }) =>
      getLanguages(page, pageSize, search, sortField, sortOrder),
    []
  );

  const {
    items: languages,
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
    defaultSortField: "languageId",
    defaultSortOrder: -1,
  });

  return (
    <div>
      <PageHeader
        title="Languages"
        addLabel="Add Language"
        onAdd={user?.role !== "Customer" ? () => addDialog.open() : undefined}
      />

      <div style={{ display: "flex", gap: "12px", marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={onSearchChange}
          placeholder="Search languages..."
        />
      </div>

      <LanguageDialog
        visible={addDialog.visible}
        onHide={() => addDialog.close()}
        onSuccess={reload}
        mode="add"
      />

      <DataTable
        value={languages}
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
        emptyMessage="No languages found."
        onRowClick={(e) => navigate(`/languages/${e.data.languageId}`)}
        rowClassName={() => "cursor-pointer"}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column field="name" header="Name" sortable />
        <Column
          field="movieCount"
          header="Movies"
          style={{ width: "100px" }}
          sortable
        />
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
