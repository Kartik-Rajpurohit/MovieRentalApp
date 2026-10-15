import { useEffect, useState, useContext } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Tag } from "primereact/tag";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { ConfirmDialog } from "primereact/confirmdialog";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import CategoryDialog from "../../components/categories/CategoryDialog";
import SearchBar from "../../components/common/SearchBar";
import usePagination from "../../hooks/usePagination";
import useDeleteEntity from "../../hooks/useDeleteEntity";
import {
  getCategoryById,
  deleteCategory,
  getMoviesByCategory,
} from "../../services/categoryService";
import { AuthContext } from "../../context/AuthContext";
import { useToastContext } from "../../context/ToastContext";

const RATING_SEVERITY = {
  G: "success",
  PG: "info",
  "PG-13": "warning",
  R: "danger",
  "NC-17": "danger",
};

// Displays details for a single movie category and lists all movies belonging to it
export default function CategoryDetailPage() {
  // Read category ID from route params
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useContext(AuthContext);
  const { showSuccess, showError } = useToastContext();

  // Category profile details state
  const [category, setCategory] = useState(null);
  const [loading, setLoading] = useState(true);
  // Controls visibility of the edit category dialog
  const [editVisible, setEditVisible] = useState(false);

  // Paginated movies belonging to this category
  const [movies, setMovies] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [moviesLoading, setMoviesLoading] = useState(false);
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState("title");
  const [sortOrder, setSortOrder] = useState(1);
  // Reusable pagination hook for movies list
  const { lazyState, onPage, reset } = usePagination(10);

  // Load category details when the category ID changes
  useEffect(() => {
    loadCategory();
  }, [id]);

  // Reload movies whenever category ID, pagination, sorting, or search query change
  useEffect(() => {
    loadMovies();
  }, [id, lazyState, search, sortField, sortOrder]);

  // Load category details from the backend
  const loadCategory = async () => {
    setLoading(true);
    try {
      const data = await getCategoryById(id);
      setCategory(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  // Load movies in this category with pagination, search, and sorting
  const loadMovies = async () => {
    setMoviesLoading(true);
    try {
      const sortOrderStr = sortOrder === 1 ? "asc" : "desc";
      const res = await getMoviesByCategory(
        id,
        lazyState.page + 1,
        lazyState.rows,
        search,
        sortField,
        sortOrderStr,
      );
      setMovies(res.data ?? []);
      setTotalRecords(res.totalRecords ?? 0);
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
    reset();
  };

  // Update search query and reset pagination to the first page
  const onSearchChange = (val) => {
    setSearch(val);
    reset();
  };

  const { confirmDelete } = useDeleteEntity({
    deleteFn: deleteCategory,
    redirectPath: "/categories",
    entityName: "Category",
  });

  // Prompt confirmation before deleting the category
  const handleDelete = () => {
    confirmDelete(
      id,
      category?.name,
      `Delete category "${category?.name}"? This will not delete associated movies.`
    );
  };

  // Show loading spinner while category details are being fetched
  if (loading) {
    return (
      <AppLayout>
        <LoadingSpinner />
      </AppLayout>
    );
  }


  return (
    <AppLayout>
      <ConfirmDialog />

      <CategoryDialog
        visible={editVisible}
        onHide={() => setEditVisible(false)}
        onSuccess={loadCategory}
        mode="edit"
        category={category}
      />

      <DetailPageHeader
        backPath="/categories"
        backLabel="Categories"
        title={category?.name}
        subtitle={`${totalRecords} movie${totalRecords !== 1 ? "s" : ""}`}
        actions={user?.role === "Admin" ? [
          { label: "Edit", icon: "pi pi-pencil", outlined: true, onClick: () => setEditVisible(true) },
          { label: "Delete", icon: "pi pi-trash", severity: "danger", outlined: true, onClick: handleDelete },
        ] : []}
      />

      {/* Movies in this category */}
      <Card title={`Movies in "${category?.name}"`}>
        <SearchBar value={search} onChange={onSearchChange} placeholder="Search movies..." />

        <DataTable
          value={movies}
          loading={moviesLoading}
          paginator
          lazy
          first={lazyState.first}
          rows={lazyState.rows}
          totalRecords={totalRecords}
          onPage={onPage}
          sortField={sortField}
          sortOrder={sortOrder}
          onSort={onSort}
          rowsPerPageOptions={[5, 10, 20]}
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
          currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
          emptyMessage="No movies in this category."
          onRowClick={(e) => navigate(`/movies/${e.data.movieId}`)}
          rowClassName={() => "cursor-pointer"}
          style={{ marginTop: "16px" }}
        >
          <Column field="title" header="Title" sortable />
          <Column
            field="releaseYear"
            header="Year"
            sortable
            style={{ width: "90px" }}
          />
          <Column
            field="languageName"
            header="Language"
            style={{ width: "110px" }}
          />
          <Column
            field="rentalRate"
            header="Rate ($)"
            sortable
            style={{ width: "100px" }}
            body={(r) => `$${r.rentalRate?.toFixed(2)}`}
          />
          <Column
            field="length"
            header="Length"
            sortable
            style={{ width: "90px" }}
            body={(r) => (r.length ? `${r.length} min` : "—")}
          />
          <Column
            field="rating"
            header="Rating"
            style={{ width: "90px" }}
            body={(r) =>
              r.rating ? (
                <Tag
                  value={r.rating}
                  severity={RATING_SEVERITY[r.rating] ?? "info"}
                />
              ) : (
                <span style={{ color: "#9ca3af" }}>—</span>
              )
            }
          />
        </DataTable>
      </Card>
    </AppLayout>
  );
}