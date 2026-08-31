import { useParams, useNavigate } from "react-router-dom";
import { useState, useEffect } from "react";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import AppLayout from "../../components/layout/AppLayout";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import SearchBar from "../../components/common/SearchBar";
import FormDialog from "../../components/common/FormDialog";
import ActorFormFields from "../../components/actors/ActorFormFields";
import usePagination from "../../hooks/usePagination";
import {
  getActorDetail,
  getFilmsByActor,
  updateActor,
  deleteActor,
} from "../../services/actorService";

const FIELD_LABEL = {
  margin: "0 0 4px 0",
  fontSize: "12px",
  color: "#6b7280",
  fontWeight: 500,
};
const FIELD_VALUE = {
  margin: 0,
  fontSize: "15px",
  color: "#111827",
  fontWeight: 600,
};

export default function ActorDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [actor, setActor] = useState(null);
  const [films, setFilms] = useState([]);
  const [totalRecords, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [filmsLoading, setFilmsLoading] = useState(false);
  const [search, setSearch] = useState("");
  const [editVisible, setEditVisible] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({ firstName: "", lastName: "" });
  const [errors, setErrors] = useState({});
  const { lazyState, onPage, reset } = usePagination(10);

  useEffect(() => {
    fetchActorDetail();
  }, [id]);
  useEffect(() => {
    fetchFilms();
  }, [id, lazyState, search]);

  const fetchActorDetail = async () => {
    try {
      const data = await getActorDetail(id);
      setActor(data);
      setForm({ firstName: data.firstName, lastName: data.lastName });
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchFilms = async () => {
    setFilmsLoading(true);
    try {
      const res = await getFilmsByActor(
        id,
        lazyState.page + 1,
        lazyState.rows,
        search,
      );
      setFilms(res.data ?? []);
      setTotal(res.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setFilmsLoading(false);
    }
  };

  const validate = () => {
    const e = {};
    if (!form.firstName?.trim()) e.firstName = "First name is required";
    if (!form.lastName?.trim()) e.lastName = "Last name is required";
    return e;
  };

  const handleUpdate = async () => {
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    setSaving(true);
    try {
      const updated = await updateActor({
        actorId: Number(id),
        firstName: form.firstName,
        lastName: form.lastName,
      });
      setActor(updated);
      setEditVisible(false);
      setErrors({});
    } catch (err) {
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = () => {
    confirmDialog({
      message: `Delete actor "${actor?.firstName} ${actor?.lastName}"? This will remove them from all films.`,
      header: "Delete Actor",
      icon: "pi pi-exclamation-triangle",
      acceptClassName: "p-button-danger",
      accept: async () => {
        await deleteActor(id);
        navigate("/actors");
      },
    });
  };

  if (loading)
    return (
      <AppLayout>
        <LoadingSpinner />
      </AppLayout>
    );

  return (
    <AppLayout>
      <ConfirmDialog />

      <FormDialog
        visible={editVisible}
        onHide={() => {
          setEditVisible(false);
          setErrors({});
        }}
        title="Edit Actor"
        onSubmit={handleUpdate}
        loading={saving}
        submitLabel="Save Changes"
      >
        <ActorFormFields form={form} setForm={setForm} errors={errors} />
      </FormDialog>

      <DetailPageHeader
        backPath="/actors"
        backLabel="Actors"
        title={actor ? `${actor.firstName} ${actor.lastName}` : `Actor ${id}`}
        subtitle={`${totalRecords} film${totalRecords !== 1 ? "s" : ""}`}
        actions={[
          { label: "Edit", icon: "pi pi-pencil", outlined: true, onClick: () => setEditVisible(true) },
          { label: "Delete", icon: "pi pi-trash", severity: "danger", outlined: true, onClick: handleDelete },
        ]}
      />
      <Card>
        {/* Info Grid */}
        {actor && (
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))",
              gap: "24px",
            }}
          >
            <div>
              <p style={FIELD_LABEL}>Actor ID</p>
              <p style={FIELD_VALUE}>{actor.actorId}</p>
            </div>
            <div>
              <p style={FIELD_LABEL}>First Name</p>
              <p style={FIELD_VALUE}>{actor.firstName}</p>
            </div>
            <div>
              <p style={FIELD_LABEL}>Last Name</p>
              <p style={FIELD_VALUE}>{actor.lastName}</p>
            </div>
            <div>
              <p style={FIELD_LABEL}>Total Films</p>
              <p style={FIELD_VALUE}>{actor.filmCount}</p>
            </div>
            <div>
              <p style={FIELD_LABEL}>Last Update</p>
              <p style={FIELD_VALUE}>
                {actor.lastUpdate
                  ? new Date(actor.lastUpdate).toLocaleDateString()
                  : "—"}
              </p>
            </div>
          </div>
        )}

        {/* Films List */}
        <div
          style={{
            borderTop: "1px solid #e5e7eb",
            paddingTop: "24px",
            marginTop: "28px",
          }}
        >
          <h3
            style={{
              margin: "0 0 16px 0",
              fontSize: "16px",
              fontWeight: 600,
              color: "#374151",
            }}
          >
            <i
              className="pi pi-video"
              style={{ marginRight: "8px", color: "#6366f1" }}
            />
            Films
          </h3>

          <SearchBar
            value={search}
            onChange={(v) => {
              setSearch(v);
              reset();
            }}
            placeholder="Search films..."
          />

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
            emptyMessage="No films found."
            onRowClick={(e) => navigate(`/movies/${e.data.filmId}`)}
            rowClassName={() => "cursor-pointer"}
            style={{ marginTop: "16px" }}
          >
            <Column field="filmId" header="ID" style={{ width: "70px" }} />
            <Column field="title" header="Title" />
            <Column
              field="releaseYear"
              header="Year"
              style={{ width: "80px" }}
            />
            <Column field="rating" header="Rating" style={{ width: "90px" }} />
            <Column
              field="rentalRate"
              header="Rate"
              style={{ width: "90px" }}
              body={(r) => `$${r.rentalRate}`}
            />
            <Column
              field="categories"
              header="Categories"
              body={(r) => r.categories?.join(", ") || "—"}
            />
          </DataTable>
        </div>
      </Card>
    </AppLayout>
  );
}
