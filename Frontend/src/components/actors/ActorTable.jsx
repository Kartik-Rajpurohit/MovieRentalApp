import { useState, useContext, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import FormDialog from "../common/FormDialog";
import ActorFormFields from "./ActorFormFields";
import useListingResource from "../../hooks/useListingResource";
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

  const fetchFn = useCallback(
    ({ page, pageSize, search, sortField, sortOrder }) =>
      getActors(page, pageSize, search, sortField, sortOrder),
    []
  );

  const {
    items: actors,
    totalRecords,
    loading,
    search,
    onSearchChange,
    sortField,
    sortOrder,
    onSort,
    lazyState,
    onPage,
    resetPagination,
    addDialog,
    reload,
  } = useListingResource({
    fetchFn,
    defaultSortField: "actorId",
    defaultSortOrder: -1,
  });

  const [saving, setSaving] = useState(false);
  // Form state for creating a new actor
  const [form, setForm] = useState(EMPTY_FORM);
  const [errors, setErrors] = useState({});

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
      resetPagination();
      reload();
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
          field="movieCount"
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
