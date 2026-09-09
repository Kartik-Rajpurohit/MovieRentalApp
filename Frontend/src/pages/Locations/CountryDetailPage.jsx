import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import FormDialog from "../../components/common/FormDialog";
import CountryFormFields from "../../components/locations/countries/CountryFormFields";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import SearchBar from "../../components/common/SearchBar";
import usePagination from "../../hooks/usePagination";
import { getCities } from "../../services/cityService";
import { FIELD_LABEL, FIELD_VALUE } from "../../utils/constants";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import {
  getCountryById,
  updateCountry,
  deleteCountry,
} from "../../services/countryService";

// Displays details for a single country, provides editing/deletion, and lists its associated cities
export default function CountryDetailPage() {
  // Read country ID from route parameters
  const { id } = useParams();
  const navigate = useNavigate();

  // State holding country profile data
  const [country, setCountry] = useState(null);
  const [loading, setLoading] = useState(true);
  // Controls visibility of the edit country dialog
  const [editVisible, setEditVisible] = useState(false);
  const [saving, setSaving] = useState(false);
  // Form state for updating country name
  const [form, setForm] = useState({ name: "" });
  // Paginated cities located in this country
  const [cities, setCities] = useState([]);
  const [totalCities, setTotalCities] = useState(0);
  const [citiesLoading, setCitiesLoading] = useState(false);
  const [citySearch, setCitySearch] = useState("");
  // Reusable pagination hook for the cities table
  const { lazyState, onPage, reset } = usePagination(10);

  // Load country details when the country ID changes
  useEffect(() => {
    loadCountry();
  }, [id]);

  // Reload cities whenever country ID, pagination, or search query change
  useEffect(() => {
    if (id) loadCities();
  }, [id, lazyState, citySearch]);

  // Load cities located in this country with pagination and search
  const loadCities = async () => {
    setCitiesLoading(true);
    try {
      const res = await getCities({
        countryId: id,
        page: lazyState.page + 1,
        pageSize: lazyState.rows,
        search: citySearch,
      });
      setCities(res.data ?? []);
      setTotalCities(res.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setCitiesLoading(false);
    }
  };

  // Load country details from the backend service
  const loadCountry = async () => {
    setLoading(true);
    try {
      const data = await getCountryById(id);
      setCountry(data);
      setForm({ name: data.name });
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  // Submit updated country name to the backend
  const handleUpdate = async () => {
    setSaving(true);
    try {
      const updated = await updateCountry({
        countryId: Number(id),
        name: form.name,
      });
      setCountry(updated);
      setEditVisible(false);
    } catch (err) {
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  // Prompt confirmation before deleting the country
  const handleDelete = () => {
    confirmDialog({
      message: `Delete country "${country?.name}"? All associated cities will also be affected.`,
      header: "Delete Country",
      icon: "pi pi-exclamation-triangle",
      acceptClassName: "p-button-danger",
      accept: async () => {
        await deleteCountry(id);
        navigate("/countries");
      },
    });
  };

  // Show loading spinner while country details are loading
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

      <FormDialog
        visible={editVisible}
        onHide={() => setEditVisible(false)}
        title="Edit Country"
        onSubmit={handleUpdate}
        loading={saving}
        submitLabel="Save Changes"
      >
        <CountryFormFields form={form} setForm={setForm} />
      </FormDialog>

      <DetailPageHeader
        backPath="/countries"
        backLabel="Countries"
        title={country?.name}
        subtitle={`${country?.cityCount} cities`}
        actions={[
          {
            label: "Edit",
            icon: "pi pi-pencil",
            outlined: true,
            onClick: () => setEditVisible(true),
          },
          {
            label: "Delete",
            icon: "pi pi-trash",
            severity: "danger",
            outlined: true,
            onClick: handleDelete,
          },
        ]}
      />

      <Card>
        {/* Info Grid */}
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))",
            gap: "24px",
          }}
        >
          <div>
            <p style={FIELD_LABEL}>Country ID</p>
            <p style={FIELD_VALUE}>{country?.countryId}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Country Name</p>
            <p style={FIELD_VALUE}>{country?.name}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Total Cities</p>
            <p style={FIELD_VALUE}>{country?.cityCount}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Last Update</p>
            <p style={FIELD_VALUE}>
              {country?.lastUpdate
                ? new Date(country.lastUpdate).toLocaleDateString()
                : "—"}
            </p>
          </div>
        </div>
      </Card>
      {/* Cities in this Country */}
      <Card
        style={{ marginTop: "24px" }}
        title={`Cities in "${country?.name}"`}
      >
        <div style={{ marginBottom: "16px" }}>
          <SearchBar
            value={citySearch}
            onChange={(v) => {
              setCitySearch(v);
              reset();
            }}
            placeholder="Search cities..."
          />
        </div>

        <DataTable
          value={cities}
          loading={citiesLoading}
          paginator
          lazy
          first={lazyState.first}
          rows={lazyState.rows}
          totalRecords={totalCities}
          onPage={onPage}
          rowsPerPageOptions={[5, 10, 20]}
          emptyMessage="No cities found."
          onRowClick={(e) => navigate(`/cities/${e.data.cityId}`)}
          rowClassName={() => "cursor-pointer"}
          tableStyle={{ minWidth: "30rem", tableLayout: "auto" }}
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
          currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
        >
          <Column field="cityId" header="ID" style={{ width: "70px" }} />
          <Column field="name" header="City Name" sortable />
          <Column
            field="addressCount"
            header="Addresses"
            style={{ width: "120px" }}
          />
          <Column
            field="lastUpdate"
            header="Last Update"
            style={{ width: "140px" }}
            body={(r) => new Date(r.lastUpdate).toLocaleDateString()}
          />
        </DataTable>
      </Card>
    </AppLayout>
  );
}
