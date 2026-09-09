import { useEffect, useState, useContext } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Tag } from "primereact/tag";
import { useNavigate } from "react-router-dom";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import { Badge } from "primereact/badge";
import { Button } from "primereact/button";
import MovieFilterDialog from "./MovieFilterDialog";
import MovieDialog from "./MovieDialog";
import usePagination from "../../hooks/usePagination";
import useFilters from "../../hooks/useFilters";
import { getMovies } from "../../services/movieService";
import { AuthContext } from "../../context/AuthContext";

const INIT_FILTERS = {
  languageId: null,
  categoryId: null,
  rating: null,
  releaseYear: null,
  minRentalRate: null,
  maxRentalRate: null,
  minLength: null,
  maxLength: null,
};

// MPAA rating color map
const RATING_SEVERITY = {
  G: "success",
  PG: "info",
  "PG-13": "warning",
  R: "danger",
  "NC-17": "danger",
};

/**
 * Movie catalogue table component.
 * Displays film titles, release year, duration, MPAA rating, and rental rates with pagination,
 * multi-criteria filtering, and role-based edit/create dialog triggers.
 */
export default function MovieTable() {
  const { user } = useContext(AuthContext);
  const navigate = useNavigate();
  // Pagination state hook (first, rows, page)
  const { lazyState, onPage, reset } = usePagination(10);
  // Filtering state hook holding movie filter criteria
  const { filters, setFilters } = useFilters(INIT_FILTERS);

  // Movie records and total count from API
  const [movies, setMovies] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  // Loading indicator for table data fetch
  const [loading, setLoading] = useState(false);
  // Sorting fields and direction
  const [sortField, setSortField] = useState("title");
  const [sortOrder, setSortOrder] = useState(1);
  // Search query text
  const [search, setSearch] = useState("");
  // Dialog visibility states for filters and create movie
  const [filterVisible, setFilterVisible] = useState(false);
  const [dialogVisible, setDialogVisible] = useState(false);

  // Reload movies list when pagination, sort, search, or filters change
  useEffect(() => {
    loadMovies();
  }, [lazyState, sortField, sortOrder, search, filters]);

  // Fetch paginated movie records with applied filters and sorting
  const loadMovies = async () => {
    setLoading(true);
    try {
      const sortOrderStr = sortOrder === 1 ? "asc" : "desc";
      const res = await getMovies(
        lazyState.page + 1,
        lazyState.rows,
        sortField,
        sortOrderStr,
        search,
        filters.languageId,
        filters.categoryId,
        filters.rating,
        filters.releaseYear,
        filters.minRentalRate,
        filters.maxRentalRate,
        filters.minLength,
        filters.maxLength,
      );
      setMovies(res.data ?? []);
      setTotalRecords(res.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  // Handle column sort click
  const onSort = (e) => {
    setSortField(e.sortField);
    setSortOrder(e.sortOrder);
    reset();
  };

  // Count active filter criteria to display badge count
  const activeFilterCount = Object.values(filters).filter(Boolean).length;


  // Rating badge column — fixed width tag
  const ratingBody = (row) =>
    row.rating ? (
      <Tag
        value={row.rating}
        severity={RATING_SEVERITY[row.rating] ?? "info"}
        style={{ width: "fit-content" }}
      />
    ) : (
      <span style={{ color: "#9ca3af" }}>—</span>
    );

  // Categories column — show first 2 then +N
  const categoriesBody = (row) => {
    const cats = row.categories ?? [];
    if (cats.length === 0) return <span style={{ color: "#9ca3af" }}>—</span>;
    return (
      <div style={{ display: "flex", gap: "4px", flexWrap: "wrap" }}>
        {cats.slice(0, 2).map((c) => (
          <Tag key={c} value={c} severity="secondary" />
        ))}
        {cats.length > 2 && (
          <Tag value={`+${cats.length - 2}`} severity="secondary" />
        )}
      </div>
    );
  };

  return (
    <div>
      {/* Header bar with Add Movie button (visible for Admin only) */}
      <PageHeader
        title="Movies"
        addLabel="Add Movie"
        onAdd={user?.role === "Admin" ? () => setDialogVisible(true) : undefined}
      />

      {/* Toolbar with live search and filter dialog toggle button */}
      <div
        style={{
          display: "flex",
          gap: "12px",
          marginBottom: "16px",
          alignItems: "center",
        }}
      >
        <SearchBar
          value={search}
          onChange={(v) => {
            setSearch(v);
            reset();
          }}
          placeholder="Search movies..."
        />
        <div style={{ position: "relative" }}>
          <Button
            label="Filters"
            icon="pi pi-sliders-h"
            outlined
            onClick={() => setFilterVisible(true)}
          />
          {activeFilterCount > 0 && (
            <Badge
              value={activeFilterCount}
              severity="danger"
              style={{ position: "absolute", top: "-8px", right: "-8px" }}
            />
          )}
        </div>
      </div>

      {/* Multi-criteria filter dialog */}
      <MovieFilterDialog
        visible={filterVisible}
        onHide={() => setFilterVisible(false)}
        filters={filters}
        onApply={(f) => {
          setFilters(f);
          reset();
        }}
      />

      {/* Modal dialog to create a new movie */}
      <MovieDialog
        visible={dialogVisible}
        onHide={() => setDialogVisible(false)}
        onSuccess={loadMovies}
        mode="add"
      />

      {/* Table displaying movie catalogue with server pagination and sorting */}
      <DataTable
        value={movies}
        paginator
        lazy
        loading={loading}

        first={lazyState.first}
        rows={lazyState.rows}
        totalRecords={totalRecords}
        onPage={onPage}
        rowsPerPageOptions={[5, 10, 20, 50]}
        sortField={sortField}
        sortOrder={sortOrder}
        onSort={onSort}
        removableSort
        emptyMessage="No movies found."
        onRowClick={(e) => navigate(`/movies/${e.data.filmId}`)}
        rowClassName={() => "cursor-pointer"}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column
          field="title"
          header="Title"
          sortable
          style={{ minWidth: "200px" }}
        />
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
          style={{ width: "100px" }}
          body={ratingBody}
        />
        <Column
          header="Categories"
          style={{ minWidth: "160px" }}
          body={categoriesBody}
        />
      </DataTable>
    </div>
  );
}
