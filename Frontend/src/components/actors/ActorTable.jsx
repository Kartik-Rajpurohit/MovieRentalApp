import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import FormDialog from "../common/FormDialog";
import ActorFormFields from "./ActorFormFields";
import useDialog from "../../hooks/useDialog";
import usePagination from "../../hooks/usePagination";
import {
  getActors,
  createActor,
  deleteActor,
} from "../../services/actorService";

const EMPTY_FORM = { firstName: "", lastName: "" };

export default function ActorTable() {
  const navigate = useNavigate();
  const addDialog = useDialog();
  const { lazyState, onPage, reset } = usePagination(10);

  const [actors, setActors] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState("actorId");
  const [sortOrder, setSortOrder] = useState(1);
  const [form, setForm] = useState(EMPTY_FORM);
  const [errors, setErrors] = useState({});

  useEffect(() => {
    loadActors();
  }, [lazyState, search, sortField, sortOrder]);

  const loadActors = async () => {
    setLoading(true);
    try {
      const res = await getActors(
        lazyState.page + 1,
        lazyState.rows,
        search,
        sortField,
        sortOrder === 1 ? "asc" : "desc",
      );
      setActors(res.data ?? []);
      setTotalRecords(res.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const onSearchChange = (val) => {
    setSearch(val);
    reset();
  };
  const onSort = (e) => {
    setSortField(e.sortField);
    setSortOrder(e.sortOrder);
    reset();
  };

  const validate = () => {
    const e = {};
    if (!form.firstName?.trim()) e.firstName = "First name is required";
    if (!form.lastName?.trim()) e.lastName = "Last name is required";
    return e;
  };

  const handleAdd = async () => {
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    setSaving(true);
    try {
      await createActor({ firstName: form.firstName, lastName: form.lastName });
      addDialog.close();
      setForm(EMPTY_FORM);
      setErrors({});
      reset();
    } catch (err) {
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = (actor, e) => {
    e.stopPropagation();
    confirmDialog({
      message: `Delete actor "${actor.firstName} ${actor.lastName}"?`,
      header: "Delete Actor",
      icon: "pi pi-exclamation-triangle",
      acceptClassName: "p-button-danger",
      accept: async () => {
        await deleteActor(actor.actorId);
        loadActors();
      },
    });
  };

  return (
    <div>
      <ConfirmDialog />

      <FormDialog
        visible={addDialog.visible}
        onHide={() => {
          addDialog.close();
          setForm(EMPTY_FORM);
          setErrors({});
        }}
        title="Add Actor"
        onSubmit={handleAdd}
        loading={saving}
        submitLabel="Add Actor"
      >
        <ActorFormFields form={form} setForm={setForm} errors={errors} />
      </FormDialog>

      <PageHeader title="Actors" onAdd={addDialog.open} addLabel="Add Actor" />

      <div style={{ marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={onSearchChange}
          placeholder="Search actors..."
        />
      </div>

      <DataTable
        value={actors}
        paginator
        lazy
        loading={loading}
        first={lazyState.first}
        rows={lazyState.rows}
        totalRecords={totalRecords}
        onPage={onPage}
        onSort={onSort}
        sortField={sortField}
        sortOrder={sortOrder}
        removableSort
        rowsPerPageOptions={[5, 10, 20]}
        emptyMessage="No actors found."
        onRowClick={(e) => navigate(`/actors/${e.data.actorId}`)}
        rowClassName={() => "cursor-pointer"}
        tableStyle={{ minWidth: "40rem", tableLayout: "auto" }}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column
          field="actorId"
          header="ID"
          sortable
          style={{ width: "70px" }}
        />
        <Column
          header="Full Name"
          sortable
          field="fullName"
          body={(r) => `${r.firstName} ${r.lastName}`}
        />
        <Column
          field="filmCount"
          header="Films"
          sortable
          style={{ width: "100px" }}
        />
        <Column
          field="lastUpdate"
          header="Last Update"
          style={{ width: "140px" }}
          body={(r) =>
            r.lastUpdate ? new Date(r.lastUpdate).toLocaleDateString() : "—"
          }
        />
      </DataTable>
    </div>
  );
}
