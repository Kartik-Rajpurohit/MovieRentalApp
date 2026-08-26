import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import SearchBar from "../../components/common/SearchBar";
import usePagination from "../../hooks/usePagination";
import { getCityById, getAddressesByCity } from "../../services/cityService";

const FIELD_LABEL = {
  margin: "0 0 4px 0",
  fontSize: "12px",
  color: "#6b7280",
  fontWeight: 500,
  textTransform: "uppercase",
  letterSpacing: "0.05em",
};
const FIELD_VALUE = {
  margin: 0,
  fontSize: "15px",
  color: "#111827",
  fontWeight: 600,
};

export default function CityDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [city, setCity] = useState(null);
  const [addresses, setAddresses] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(true);
  const [addressLoading, setAddressLoading] = useState(false);
  const [search, setSearch] = useState("");
  const { lazyState, onPage, reset } = usePagination(10);

  useEffect(() => {
    fetchCity();
  }, [id]);
  useEffect(() => {
    fetchAddresses();
  }, [id, lazyState, search]);

  const fetchCity = async () => {
    try {
      const data = await getCityById(id);
      setCity(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchAddresses = async () => {
    setAddressLoading(true);
    try {
      const res = await getAddressesByCity(
        id,
        lazyState.page + 1,
        lazyState.rows,
        search,
      );
      setAddresses(res.data ?? []);
      setTotalRecords(res.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setAddressLoading(false);
    }
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
      <DetailPageHeader
        backPath="/cities"
        backLabel="Cities"
        title={city?.name ?? `City ${id}`}
        subtitle={city ? `${city.countryName} · ${totalRecords} addresses` : ""}
      />

      <Card>
        {/* Header */}
        <div style={{ display: "flex", alignItems: "center", gap: "16px", marginBottom: "32px", flexWrap: "wrap" }}>
          <div style={{ width: "64px", height: "64px", borderRadius: "50%", background: "#ede9fe", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <i className="pi pi-map-marker" style={{ fontSize: "1.8rem", color: "#6366f1" }} />
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: "22px", fontWeight: 600 }}>
              {city?.name}
            </h2>
            <span style={{ color: "#6b7280", fontSize: "14px" }}>
              {city?.countryName} · {totalRecords} addresses
            </span>
          </div>
        </div>

        {/* Info Grid */}
        {city && (
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))",
              gap: "24px",
            }}
          >
            <div>
              <p style={FIELD_LABEL}>City ID</p>
              <p style={FIELD_VALUE}>{city.cityId}</p>
            </div>
            <div>
              <p style={FIELD_LABEL}>City Name</p>
              <p style={FIELD_VALUE}>{city.name}</p>
            </div>
            <div>
              <p style={FIELD_LABEL}>Country</p>
              <p style={FIELD_VALUE}>{city.countryName}</p>
            </div>
            <div>
              <p style={FIELD_LABEL}>Total Addresses</p>
              <p style={FIELD_VALUE}>{totalRecords}</p>
            </div>
            <div>
              <p style={FIELD_LABEL}>Last Update</p>
              <p style={FIELD_VALUE}>
                {city.lastUpdate
                  ? new Date(city.lastUpdate).toLocaleDateString()
                  : "—"}
              </p>
            </div>
          </div>
        )}

        {/* Addresses List */}
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
              className="pi pi-home"
              style={{ marginRight: "8px", color: "#6366f1" }}
            />
            Addresses
          </h3>
          <SearchBar
            value={search}
            onChange={(v) => {
              setSearch(v);
              reset();
            }}
            placeholder="Search addresses..."
          />
          <DataTable
            value={addresses}
            loading={addressLoading}
            paginator
            lazy
            first={lazyState.first}
            rows={lazyState.rows}
            totalRecords={totalRecords}
            onPage={onPage}
            rowsPerPageOptions={[5, 10, 20]}
            emptyMessage="No addresses found."
            style={{ marginTop: "16px" }}
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
          >
            <Column
              field="addressId"
              header="ID"
              style={{ width: "70px" }}
            />
            <Column field="street" header="Street" />
            <Column field="district" header="District" />
            <Column
              field="postalCode"
              header="Postal Code"
              style={{ width: "120px" }}
            />
            <Column
              field="phone"
              header="Phone"
              style={{ width: "140px" }}
            />
          </DataTable>
        </div>
      </Card>
    </AppLayout>
  );
}
