import { useEffect, useState, useContext } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import FormDialog from "../common/FormDialog";
import ActorFormFields from "./ActorFormFields";
import useDialog from "../../hooks/useDialog";
import usePagination from "../../hooks/usePagination";
import {
  getActors,
  createActor,
} from "../../services/actorService";
import { AuthContext } from "../../context/AuthContext";

const EMPTY_FORM = { firstName: "", lastName: "" };

// Displays the list of actors in a data table with search, server-side pagination, sorting, and actor creation
export default function ActorTable() {
  const { user } = useContext(AuthContext);
  const navigate = useNavigate();
  // Dialog visibility hook for the Add Actor dialog
  const addDialog = useDialog();
  // Server-side pagination hook
  const { lazyState, onPage, reset } = usePagination(10);

  // Table records and total count
  const [actors, setActors] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState("actorId");
  const [sortOrder, setSortOrder] = useState(1);
  // Form state for creating a new actor
  const [form, setForm] = useState(EMPTY_FORM);
  const [errors, setErrors] = useState({});

  // Reload actors when pagination, search, or sorting change
  useEffect(() => {
    loadActors();
  }, [lazyState, search, sortField, sortOrder]);

  // Load paginated and sorted actor records from the backend API
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

  // Handle search query change and reset pagination to page 1
  const onSearchChange = (val) => {
    setSearch(val);
    reset();
  };

  // Handle column sorting and reload data
  const onSort = (e) => {
    setSortField(e.sortField);
    setSortOrder(e.sortOrder);
    reset();
  };

  // Validate form before submitting new actor
  const validate = () => {
    const e = {};
    if (!form.firstName?.trim()) e.firstName = "First name is required";
    if (!form.lastName?.trim()) e.lastName = "Last name is required";
    return e;
  };

  // Submit new actor to the backend API and refresh list
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


  return (
    <div>
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

      <PageHeader
        title="Actors"
        onAdd={user?.role !== "Customer" ? addDialog.open : undefined}
        addLabel="Add Actor"
      />

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
          header="Movies"
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
