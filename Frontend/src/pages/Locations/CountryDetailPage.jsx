import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import FormDialog from "../../components/common/FormDialog";
import CountryFormFields from "../../components/locations/countries/CountryFormFields";
import { FIELD_LABEL, FIELD_VALUE } from "../../utils/constants";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import {
  getCountryById,
  updateCountry,
  deleteCountry,
} from "../../services/countryService";

export default function CountryDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [country, setCountry] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editVisible, setEditVisible] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({ name: "" });

  useEffect(() => {
    loadCountry();
  }, [id]);

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
          { label: "Edit", icon: "pi pi-pencil", outlined: true, onClick: () => setEditVisible(true) },
          { label: "Delete", icon: "pi pi-trash", severity: "danger", outlined: true, onClick: handleDelete },
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
    </AppLayout>
  );
}
