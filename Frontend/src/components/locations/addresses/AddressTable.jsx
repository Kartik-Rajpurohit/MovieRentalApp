import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import PageHeader from "../../common/PageHeader";
import SearchBar from "../../common/SearchBar";
import usePagination from "../../../hooks/usePagination";
import { getAddresses } from "../../../services/addressService";

export default function AddressTable() {
  const navigate = useNavigate();
  const { lazyState, onPage, reset } = usePagination(10);
  const [addresses, setAddresses] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState("addressId");
  const [sortOrder, setSortOrder] = useState(1);

  useEffect(() => { loadAddresses(); }, [lazyState, search, sortField, sortOrder]);

  const loadAddresses = async () => {
    setLoading(true);
    try {
      const res = await getAddresses({
        page: lazyState.page + 1,
        pageSize: lazyState.rows,
        search,
        sortField,
        sortOrder: sortOrder === 1 ? "asc" : "desc",
      });
      setAddresses(res.data ?? []);
      setTotalRecords(res.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <PageHeader title="Addresses" />
      <div style={{ marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={(v) => { setSearch(v); reset(); }}
          placeholder="Search by street, district or city..."
        />
      </div>
      <DataTable
        value={addresses}
        paginator lazy loading={loading}
        first={lazyState.first} rows={lazyState.rows}
        totalRecords={totalRecords} onPage={onPage}
        onSort={(e) => { setSortField(e.sortField); setSortOrder(e.sortOrder); reset(); }}
        sortField={sortField} sortOrder={sortOrder}
        rowsPerPageOptions={[5, 10, 20]}
        emptyMessage="No addresses found."
        onRowClick={(e) => navigate(`/addresses/${e.data.addressId}`)}
        rowClassName={() => "cursor-pointer"}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column field="addressId" header="ID" sortable style={{ width: "70px" }} />
        <Column field="street" header="Street" sortable />
        <Column field="district" header="District" sortable />
        <Column field="cityName" header="City" sortable="city" />
        <Column field="countryName" header="Country" />
        <Column field="postalCode" header="Postal Code" style={{ width: "120px" }} />
        <Column field="phone" header="Phone" style={{ width: "140px" }} />
      </DataTable>
    </div>
  );
}
