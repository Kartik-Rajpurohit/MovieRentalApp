import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import PageHeader from "../../common/PageHeader";
import SearchBar from "../../common/SearchBar";
import usePagination from "../../../hooks/usePagination";
import { getCities } from "../../../services/cityService";

export default function CityTable() {
  const navigate = useNavigate();
  const { lazyState, onPage, reset } = usePagination(10);
  const [cities, setCities] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState("cityId");
  const [sortOrder, setSortOrder] = useState(1);

  useEffect(() => {
    loadCities();
  }, [lazyState, search, sortField, sortOrder]);

  const loadCities = async () => {
    setLoading(true);
    try {
      const res = await getCities({
        page: lazyState.page + 1,
        pageSize: lazyState.rows,
        search,
        sortField,
        sortOrder: sortOrder === 1 ? "asc" : "desc",
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
      <PageHeader title="Cities" />
      <div style={{ marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={(v) => {
            setSearch(v);
            reset();
          }}
          placeholder="Search cities..."
        />
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
