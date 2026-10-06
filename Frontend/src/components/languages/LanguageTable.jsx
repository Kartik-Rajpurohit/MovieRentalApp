import { useEffect, useState, useContext } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { useNavigate } from "react-router-dom";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import LanguageDialog from "./LanguageDialog";
import usePagination from "../../hooks/usePagination";
import { getLanguages } from "../../services/languageService";
import { AuthContext } from "../../context/AuthContext";

// Displays the languages catalog in a DataTable with server-side pagination, search, and sorting
export default function LanguageTable() {
  const { user } = useContext(AuthContext);
  const navigate = useNavigate();
  // Server-side pagination hook
  const { lazyState, onPage, reset } = usePagination(10);

  // Table records and total records state
  const [languages, setLanguages] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [sortField, setSortField] = useState("languageId");
  const [sortOrder, setSortOrder] = useState(-1);
  const [search, setSearch] = useState("");
  // Controls visibility of the Add Language dialog
  const [dialogVisible, setDialogVisible] = useState(false);

  // Reload languages whenever pagination, sorting, or search change
  useEffect(() => {
    loadLanguages();
  }, [lazyState, sortField, sortOrder, search]);

  // Load paginated and sorted languages from the backend service
  const loadLanguages = async () => {
    setLoading(true);
    try {
      const sortOrderStr = sortOrder === 1 ? "asc" : "desc";
      const res = await getLanguages(
        lazyState.page + 1,
        lazyState.rows,
        search,
        sortField,
        sortOrderStr,
      );
      setLanguages(res.data ?? []);
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

  const onSearchChange = (val) => {
    setSearch(val);
    reset();
  };

  return (
    <div>
      <PageHeader
        title="Languages"
        addLabel="Add Language"
        onAdd={user?.role !== "Customer" ? () => setDialogVisible(true) : undefined}
      />

      <div style={{ display: "flex", gap: "12px", marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={onSearchChange}
          placeholder="Search languages..."
        />
      </div>

      <LanguageDialog
        visible={dialogVisible}
        onHide={() => setDialogVisible(false)}
        onSuccess={loadLanguages}
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
