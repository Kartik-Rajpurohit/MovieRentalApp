import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Tag } from "primereact/tag";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import { InputText } from "primereact/inputtext";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import LanguageDialog from "../../components/languages/LanguageDialog";
import { getLanguageById, getFilmsByLanguage, deleteLanguage } from "../../services/languageService";

const RATING_SEVERITY = {
  G: "success", PG: "info", "PG-13": "warning", R: "danger", "NC-17": "danger",
};

export default function LanguageDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [language, setLanguage] = useState(null);
  const [films, setFilms] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [pageLoading, setPageLoading] = useState(true);
  const [filmsLoading, setFilmsLoading] = useState(false);
  const [editVisible, setEditVisible] = useState(false);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(0);
  const [rows] = useState(10);

  useEffect(() => { loadLanguage(); }, [id]);
  useEffect(() => { loadFilms(); }, [id, page, search]);

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

  const loadFilms = async () => {
    setFilmsLoading(true);
    try {
      const data = await getFilmsByLanguage(id, page + 1, rows, search);
      setFilms(data.data ?? []);
      setTotalRecords(data.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setFilmsLoading(false);
    }
  };

  const handleDelete = () => {
    confirmDialog({
      message: `Delete language "${language?.name}"? Movies using this language will be affected.`,
      header: "Delete Language",
      icon: "pi pi-exclamation-triangle",
      acceptClassName: "p-button-danger",
      accept: async () => {
        await deleteLanguage(id);
        navigate("/languages");
      },
    });
  };

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
        subtitle={`${language?.filmCount} movie${language?.filmCount !== 1 ? "s" : ""}`}
        actions={[
          { label: "Edit", icon: "pi pi-pencil", outlined: true, onClick: () => setEditVisible(true) },
          { label: "Delete", icon: "pi pi-trash", severity: "danger", outlined: true, onClick: handleDelete },
        ]}
      />

      {/* Films in this language */}
      <Card title={`Movies in "${language?.name}"`}>
        {/* Search inside films */}
        <div style={{ marginBottom: "16px" }}>
          <InputText
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(0); }}
            placeholder="Search movies..."
            style={{ width: "300px" }}
          />
        </div>

        <DataTable
          value={films}
          paginator
          lazy
          loading={filmsLoading}
          first={page * rows}
          rows={rows}
          totalRecords={totalRecords}
          onPage={(e) => setPage(e.page)}
          rowsPerPageOptions={[5, 10, 20]}
          emptyMessage="No movies found."
          onRowClick={(e) => navigate(`/movies/${e.data.filmId}`)}
          rowClassName={() => "cursor-pointer"}
        >
          <Column field="title" header="Title" sortable />
          <Column field="releaseYear" header="Year" style={{ width: "90px" }} />
          <Column field="rentalRate" header="Rate ($)" style={{ width: "100px" }}
            body={(r) => `$${r.rentalRate?.toFixed(2)}`} />
          <Column field="length" header="Length" style={{ width: "90px" }}
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
