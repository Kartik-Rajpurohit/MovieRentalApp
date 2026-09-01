import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import PageHeader from "../../common/PageHeader";
import SearchBar from "../../common/SearchBar";
import usePagination from "../../../hooks/usePagination";
import { getCities } from "../../../services/cityService";
import { Button } from "primereact/button";
import { Badge } from "primereact/badge";
import CityFilterDialog from "./CityFilterDialog";
import useDialog from "../../../hooks/useDialog";
import FormDialog from "../../common/FormDialog";
import CityFormFields from "./CityFormFields";
import { createCity } from "../../../services/cityService";

export default function CityTable() {
  const navigate = useNavigate();
  const { lazyState, onPage, reset } = usePagination(10);
  const [cities, setCities] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState("cityId");
  const [sortOrder, setSortOrder] = useState(1);
  const INIT_FILTERS = { countryId: null };
  const [filters, setFilters] = useState(INIT_FILTERS);
  const [filterVisible, setFilterVisible] = useState(false);
  const addDialog = useDialog();
  const [form, setForm] = useState({ name: "", countryId: null });
  const [saving, setSaving] = useState(false);
  const [formErrors, setFormErrors] = useState({});

  const handleAdd = async () => {
    const errs = {};
    if (!form.name?.trim()) errs.name = "City name is required";
    if (!form.countryId) errs.countryId = "Country is required";
    if (Object.keys(errs).length > 0) {
      setFormErrors(errs);
      return;
    }

    setSaving(true);
    try {
      await createCity({ name: form.name, countryId: form.countryId });
      addDialog.close();
      setForm({ name: "", countryId: null });
      setFormErrors({});
      loadCities();
    } catch (err) {
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  useEffect(() => {
    loadCities();
  }, [lazyState, search, sortField, sortOrder, filters]);

  const loadCities = async () => {
    setLoading(true);
    try {
      const res = await getCities({
        page: lazyState.page + 1,
        pageSize: lazyState.rows,
        search,
        sortField,
        sortOrder: sortOrder === 1 ? "asc" : "desc",
        countryId: filters.countryId ?? undefined,
      });
      setCities(res.data ?? []);
      setTotalRecords(res.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <FormDialog
        visible={addDialog.visible}
        onHide={() => {
          addDialog.close();
          setForm({ name: "", countryId: null });
          setFormErrors({});
        }}
        title="Add City"
        onSubmit={handleAdd}
        loading={saving}
        submitLabel="Add City"
      >
        <CityFormFields form={form} setForm={setForm} errors={formErrors} />
      </FormDialog>

      <PageHeader title="Cities" onAdd={addDialog.open} addLabel="Add City" />
      <CityFilterDialog
        visible={filterVisible}
        onHide={() => setFilterVisible(false)}
        filters={filters}
        onApply={(f) => {
          setFilters(f);
          reset();
        }}
      />
      <div style={{ display: "flex", gap: "12px", marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={(v) => {
            setSearch(v);
            reset();
          }}
          placeholder="Search cities..."
        />
        <div style={{ position: "relative" }}>
          <Button
            label="Filters"
            icon="pi pi-sliders-h"
            outlined
            onClick={() => setFilterVisible(true)}
          />
          {filters.countryId !== null && (
            <Badge
              value={1}
              severity="danger"
              style={{ position: "absolute", top: "-8px", right: "-8px" }}
            />
          )}
        </div>
      </div>
      <DataTable
        value={cities}
        paginator
        lazy
        loading={loading}
        first={lazyState.first}
        rows={lazyState.rows}
        totalRecords={totalRecords}
        onPage={onPage}
        onSort={(e) => {
          setSortField(e.sortField);
          setSortOrder(e.sortOrder);
          reset();
        }}
        sortField={sortField}
        sortOrder={sortOrder}
        rowsPerPageOptions={[5, 10, 20]}
        emptyMessage="No cities found."
        onRowClick={(e) => navigate(`/cities/${e.data.cityId}`)}
        rowClassName={() => "cursor-pointer"}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column field="cityId" header="ID" sortable style={{ width: "80px" }} />
        <Column field="name" header="City" sortable />
        <Column field="countryName" header="Country" sortable="country" />
        <Column
          field="addressCount"
          header="Addresses"
          sortable="addresscount"
          style={{ width: "120px" }}
        />
        <Column
          field="lastUpdate"
          header="Last Update"
          style={{ width: "130px" }}
          body={(r) =>
            r.lastUpdate ? new Date(r.lastUpdate).toLocaleDateString() : "—"
          }
        />
      </DataTable>
    </div>
  );
}
