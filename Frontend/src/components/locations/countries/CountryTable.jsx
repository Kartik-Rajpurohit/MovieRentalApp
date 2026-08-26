import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import PageHeader from "../../common/PageHeader";
import SearchBar from "../../common/SearchBar";
import FormDialog from "../../common/FormDialog";
import CountryFormFields from "./CountryFormFields";
import useDialog from "../../../hooks/useDialog";
import usePagination from "../../../hooks/usePagination";
import { getCountries, createCountry } from "../../../services/countryService";

const EMPTY_FORM = { name: "" };

export default function CountryTable() {
  const navigate = useNavigate();
  const addDialog = useDialog();
  const { lazyState, onPage, reset } = usePagination(10);

  const [countries, setCountries] = useState([]);
  const [totalRecords, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState("");
  const [sortOrder, setSortOrder] = useState(1);
  const [form, setForm] = useState(EMPTY_FORM);

  useEffect(() => {
    loadCountries();
  }, [lazyState, search, sortField, sortOrder]);

  const loadCountries = async () => {
    setLoading(true);
    try {
      const res = await getCountries(
        lazyState.page + 1,
        lazyState.rows,
        search,
        sortField,
        sortOrder === 1 ? "asc" : "desc",
      );
      setCountries(res.data ?? []);
      setTotal(res.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const onSort = (e) => {
    setSortField(e.sortField);
    setSortOrder(e.sortOrder);
    reset();
  };
  const onSearchChange = (v) => {
    setSearch(v);
    reset();
  };

  const handleAdd = async () => {
    if (!form.name.trim()) return;
    setSaving(true);
    try {
      await createCountry({ name: form.name });
      addDialog.close();
      setForm(EMPTY_FORM);
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
        }}
        title="Add Country"
        onSubmit={handleAdd}
        loading={saving}
        submitLabel="Add Country"
      >
        <CountryFormFields form={form} setForm={setForm} />
      </FormDialog>

      <PageHeader
        title="Countries"
        onAdd={addDialog.open}
        addLabel="Add Country"
      />

      <div style={{ marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={onSearchChange}
          placeholder="Search countries..."
        />
      </div>

      <DataTable
        value={countries}
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
        removableSort
        onRowClick={(e) => navigate(`/countries/${e.data.countryId}`)}
        rowClassName={() => "cursor-pointer"}
        emptyMessage="No countries found."
        tableStyle={{ minWidth: "40rem", tableLayout: "auto" }}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column
          field="countryId"
          header="ID"
          style={{ width: "80px" }}
          sortable
        />
        <Column field="name" header="Country Name" sortable />
        <Column
          field="cityCount"
          header="Cities"
          style={{ width: "100px" }}
          sortable
        />
        <Column
          field="lastUpdate"
          header="Last Update"
          style={{ width: "150px" }}
          body={(r) => new Date(r.lastUpdate).toLocaleDateString()}
        />
      </DataTable>
    </div>
  );
}
