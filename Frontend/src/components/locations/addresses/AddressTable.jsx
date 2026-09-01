import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import PageHeader from "../../common/PageHeader";
import SearchBar from "../../common/SearchBar";
import usePagination from "../../../hooks/usePagination";
import { getAddresses } from "../../../services/addressService";
import { Button } from "primereact/button";
import { Badge } from "primereact/badge";
import AddressFilterDialog from "./AddressFilterDialog";
import useDialog from "../../../hooks/useDialog";
import FormDialog from "../../common/FormDialog";
import AddressFormFields from "./AddressFormFields";
import { createAddress } from "../../../services/addressService";

export default function AddressTable() {
  const navigate = useNavigate();
  const { lazyState, onPage, reset } = usePagination(10);
  const [addresses, setAddresses] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState("addressId");
  const [sortOrder, setSortOrder] = useState(1);
  const INIT_FILTERS = { city: null, postalCode: null };
  const [filters, setFilters] = useState(INIT_FILTERS);
  const [filterVisible, setFilterVisible] = useState(false);
  const addDialog = useDialog();
  const [form, setForm] = useState({
    cityId: null,
    street: "",
    postalCode: "",
    phone: "",
  });
  const [saving, setSaving] = useState(false);
  const [formErrors, setFormErrors] = useState({});

  const handleAdd = async () => {
    const errs = {};
    if (!form.cityId) errs.cityId = "City is required";
    if (!form.street?.trim()) errs.street = "Street is required";
    if (Object.keys(errs).length > 0) {
      setFormErrors(errs);
      return;
    }

    setSaving(true);
    try {
      await createAddress({
        cityId: form.cityId,
        street: form.street,
        postalCode: form.postalCode || null,
        phone: form.phone || "",
      });
      addDialog.close();
      setForm({
        cityId: null,
        street: "",
        postalCode: "",
        phone: "",
      });
      setFormErrors({});
      loadAddresses();
    } catch (err) {
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  useEffect(() => {
    loadAddresses();
  }, [lazyState, search, sortField, sortOrder, filters]);

  const loadAddresses = async () => {
    setLoading(true);
    try {
      const res = await getAddresses({
        page: lazyState.page + 1,
        pageSize: lazyState.rows,
        search,
        sortField,
        sortOrder: sortOrder === 1 ? "asc" : "desc",
        city: filters.city ?? undefined,
        postalCode: filters.postalCode ?? undefined,
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
      <FormDialog
        visible={addDialog.visible}
        onHide={() => {
          addDialog.close();
          setForm({
            cityId: null,
            street: "",
            postalCode: "",
            phone: "",
          });
          setFormErrors({});
        }}
        title="Add Address"
        onSubmit={handleAdd}
        loading={saving}
        submitLabel="Add Address"
      >
        <AddressFormFields form={form} setForm={setForm} errors={formErrors} />
      </FormDialog>

      <PageHeader
        title="Addresses"
        onAdd={addDialog.open}
        addLabel="Add Address"
      />
      <AddressFilterDialog
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
          placeholder="Search by street or city..."
        />
        <div style={{ position: "relative" }}>
          <Button
            label="Filters"
            icon="pi pi-sliders-h"
            outlined
            onClick={() => setFilterVisible(true)}
          />
          {Object.values(filters).filter((v) => v !== null).length > 0 && (
            <Badge
              value={Object.values(filters).filter((v) => v !== null).length}
              severity="danger"
              style={{ position: "absolute", top: "-8px", right: "-8px" }}
            />
          )}
        </div>
      </div>
      <DataTable
        value={addresses}
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
        emptyMessage="No addresses found."
        onRowClick={(e) => navigate(`/addresses/${e.data.addressId}`)}
        rowClassName={() => "cursor-pointer"}
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column
          field="addressId"
          header="ID"
          sortable
          style={{ width: "70px" }}
        />
        <Column field="street" header="Street" sortable />
        <Column field="cityName" header="City" sortable="city" />
        <Column field="countryName" header="Country" />
        <Column
          field="postalCode"
          header="Postal Code"
          style={{ width: "120px" }}
        />
        <Column field="phone" header="Phone" style={{ width: "140px" }} />
      </DataTable>
    </div>
  );
}
