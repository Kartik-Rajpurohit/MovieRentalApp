import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Tag } from "primereact/tag";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import CategoryDialog from "../../components/categories/CategoryDialog";
import SearchBar from "../../components/common/SearchBar";
import usePagination from "../../hooks/usePagination";
import {
  getCategoryById,
  deleteCategory,
  getFilmsByCategory,
} from "../../services/categoryService";

const RATING_SEVERITY = {
  G: "success",
  PG: "info",
  "PG-13": "warning",
  R: "danger",
  "NC-17": "danger",
};

export default function CategoryDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [category, setCategory] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editVisible, setEditVisible] = useState(false);

  const [films, setFilms] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [filmsLoading, setFilmsLoading] = useState(false);
  const [search, setSearch] = useState("");
  const { lazyState, onPage, reset } = usePagination(10);

  useEffect(() => {
    loadCategory();
  }, [id]);

  useEffect(() => {
    loadFilms();
  }, [id, lazyState, search]);

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

  const loadFilms = async () => {
    setFilmsLoading(true);
    try {
      const res = await getFilmsByCategory(id, lazyState.page + 1, lazyState.rows, search);
      setFilms(res.data ?? []);
      setTotalRecords(res.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setFilmsLoading(false);
    }
  };

  const onSearchChange = (val) => {
    setSearch(val);
    reset();
  };

  const handleDelete = () => {
    confirmDialog({
      message: `Delete category "${category?.name}"? This will not delete associated movies.`,
      header: "Delete Category",
      icon: "pi pi-exclamation-triangle",
      acceptClassName: "p-button-danger",
      accept: async () => {
        await deleteCategory(id);
        navigate("/categories");
      },
    });
  };

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
        actions={[
          { label: "Edit", icon: "pi pi-pencil", outlined: true, onClick: () => setEditVisible(true) },
          { label: "Delete", icon: "pi pi-trash", severity: "danger", outlined: true, onClick: handleDelete },
        ]}
      />

      {/* Movies in this category */}
      <Card title={`Movies in "${category?.name}"`}>
        <SearchBar value={search} onChange={onSearchChange} placeholder="Search movies..." />

        <DataTable
          value={films}
          loading={filmsLoading}
          paginator
          lazy
          first={lazyState.first}
          rows={lazyState.rows}
          totalRecords={totalRecords}
          onPage={onPage}
          rowsPerPageOptions={[5, 10, 20]}
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
          currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
          emptyMessage="No movies in this category."
          onRowClick={(e) => navigate(`/movies/${e.data.filmId}`)}
          rowClassName={() => "cursor-pointer"}
          style={{ marginTop: "16px" }}
        >
          <Column field="title" header="Title" sortable />
          <Column
            field="releaseYear"
            header="Year"
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
            style={{ width: "100px" }}
            body={(r) => `$${r.rentalRate?.toFixed(2)}`}
          />
          <Column
            field="length"
            header="Length"
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