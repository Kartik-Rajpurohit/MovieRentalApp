import { useEffect, useState, useContext } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Tag } from "primereact/tag";
import { ConfirmDialog } from "primereact/confirmdialog";
import { InputText } from "primereact/inputtext";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import LanguageDialog from "../../components/languages/LanguageDialog";
import useDeleteEntity from "../../hooks/useDeleteEntity";
import { getLanguageById, getMoviesByLanguage, deleteLanguage } from "../../services/languageService";
import { AuthContext } from "../../context/AuthContext";
import { useToastContext } from "../../context/ToastContext";

const RATING_SEVERITY = {
  G: "success", PG: "info", "PG-13": "warning", R: "danger", "NC-17": "danger",
};

// Displays details for a specific language and lists all movies available in this language
export default function LanguageDetailPage() {
  // Read language ID from route parameters
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useContext(AuthContext);
  const { showSuccess, showError } = useToastContext();

  // State holding language profile information
  const [language, setLanguage] = useState(null);
  // Paginated movies in this language
  const [movies, setMovies] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [pageLoading, setPageLoading] = useState(true);
  const [moviesLoading, setMoviesLoading] = useState(false);
  // Controls visibility of the edit language dialog
  const [editVisible, setEditVisible] = useState(false);
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState("title");
  const [sortOrder, setSortOrder] = useState(1);
  const [page, setPage] = useState(0);
  const [rows, setRows] = useState(10);

  // Fetch language details when the ID changes
  useEffect(() => { loadLanguage(); }, [id]);
  // Fetch movies whenever ID, pagination page, rows, sorting, or search query change
  useEffect(() => { loadMovies(); }, [id, page, rows, search, sortField, sortOrder]);

  // Load language details from the backend
  const loadLanguage = async () => {
    setPageLoading(true);
    try {
      const data = await getLanguageById(id);
      setLanguage(data);
    } catch (err) {
      console.error(err);
    } finally {
      setPageLoading(false);
    }
  };

  // Load movies associated with this language with pagination, search, and sorting
  const loadMovies = async () => {
    setMoviesLoading(true);
    try {
      const sortOrderStr = sortOrder === 1 ? "asc" : "desc";
      const data = await getMoviesByLanguage(
        id,
        page + 1,
        rows,
        search,
        sortField,
        sortOrderStr,
      );
      setMovies(data.data ?? []);
      setTotalRecords(data.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setMoviesLoading(false);
    }
  };

  // Handle column sort click
  const onSort = (e) => {
    setSortField(e.sortField);
    setSortOrder(e.sortOrder);
    setPage(0);
  };

  const { confirmDelete } = useDeleteEntity({
    deleteFn: deleteLanguage,
    redirectPath: "/languages",
    entityName: "Language",
  });

  // Prompt confirmation dialog before deleting this language
  const handleDelete = () => {
    confirmDelete(
      id,
      language?.name,
      `Delete language "${language?.name}"? Movies using this language will be affected.`
    );
  };

  // Show loading spinner while language profile is loading
  if (pageLoading) {
    return (
      <AppLayout>
        <LoadingSpinner />
      </AppLayout>
    );
  }


  return (
    <AppLayout>
      <ConfirmDialog />

      <LanguageDialog
        visible={editVisible}
        onHide={() => setEditVisible(false)}
        onSuccess={loadLanguage}
        mode="edit"
        language={language}
      />

      <DetailPageHeader
        backPath="/languages"
        backLabel="Languages"
        title={language?.name}
        subtitle={`${language?.movieCount} movie${language?.movieCount !== 1 ? "s" : ""}`}
        actions={user?.role === "Admin" ? [
          { label: "Edit", icon: "pi pi-pencil", outlined: true, onClick: () => setEditVisible(true) },
          { label: "Delete", icon: "pi pi-trash", severity: "danger", outlined: true, onClick: handleDelete },
        ] : []}
      />

      {/* Movies in this language */}
      <Card title={`Movies in "${language?.name}"`}>
        {/* Search inside movies */}
        <div style={{ marginBottom: "16px" }}>
          <InputText
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(0); }}
            placeholder="Search movies..."
            style={{ width: "300px" }}
          />
        </div>

        <DataTable
          value={movies}
          paginator
          lazy
          loading={moviesLoading}
          first={page * rows}
          rows={rows}
          totalRecords={totalRecords}
          onPage={(e) => {
            setPage(e.page);
            setRows(e.rows);
          }}
          sortField={sortField}
          sortOrder={sortOrder}
          onSort={onSort}
          rowsPerPageOptions={[5, 10, 20]}
          emptyMessage="No movies found."
          onRowClick={(e) => navigate(`/movies/${e.data.movieId}`)}
          rowClassName={() => "cursor-pointer"}
        >
          <Column field="title" header="Title" sortable />
          <Column field="releaseYear" header="Year" sortable style={{ width: "90px" }} />
          <Column field="rentalRate" header="Rate ($)" sortable style={{ width: "100px" }}
            body={(r) => `$${r.rentalRate?.toFixed(2)}`} />
          <Column field="length" header="Length" sortable style={{ width: "90px" }}
            body={(r) => r.length ? `${r.length} min` : "—"} />
          <Column field="rating" header="Rating" style={{ width: "90px" }}
            body={(r) => r.rating
              ? <Tag value={r.rating} severity={RATING_SEVERITY[r.rating] ?? "info"} />
              : <span style={{ color: "#9ca3af" }}>—</span>} />
        </DataTable>
      </Card>
    </AppLayout>
  );
}
