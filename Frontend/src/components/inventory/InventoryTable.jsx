import { useEffect, useState } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Tag } from "primereact/tag";
import { useNavigate } from "react-router-dom";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import InventoryDialog from "./InventoryDialog";
import usePagination from "../../hooks/usePagination";
import { getInventory } from "../../services/inventoryService";

export default function InventoryTable() {
  const navigate = useNavigate();
  const { lazyState, onPage, reset } = usePagination(10);

  const [inventory, setInventory] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [sortField, setSortField] = useState("");
  const [sortOrder, setSortOrder] = useState(1);
  const [search, setSearch] = useState("");
  const [storeFilter, setStoreFilter] = useState("");
  const [dialogVisible, setDialogVisible] = useState(false);

  useEffect(() => { loadInventory(); }, [lazyState, sortField, sortOrder, search, storeFilter]);

  const loadInventory = async () => {
    setLoading(true);
    try {
      const sortOrderStr = sortOrder === 1 ? "asc" : "desc";
      const res = await getInventory({
        page: lazyState.page + 1,
        pageSize: lazyState.rows,
        search,
        sortField,
        sortOrder: sortOrderStr,
        storeId: storeFilter || undefined,
      });
      setInventory(res.data ?? []);
      setTotalRecords(res.totalRecords ?? 0);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const onSort = (e) => { setSortField(e.sortField); setSortOrder(e.sortOrder); reset(); };

  return (
    <div>
      <PageHeader
        title="Inventory"
        addLabel="Add Copy"
        onAdd={() => setDialogVisible(true)}
      />

      <div style={{ display: "flex", gap: "12px", marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={(v) => { setSearch(v); reset(); }}
          placeholder="Search by film title or ID..."
        />
      </div>

      <InventoryDialog
        visible={dialogVisible}
        onHide={() => setDialogVisible(false)}
        onSuccess={loadInventory}
        mode="add"
      />

      <DataTable
        value={inventory}
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
        emptyMessage="No inventory found."
        onRowClick={(e) => navigate(`/inventory/${e.data.inventoryId}`)}
        rowClassName={() => "cursor-pointer"}
      >
        <Column field="inventoryId" header="ID" sortable style={{ width: "80px" }} />
        <Column field="filmTitle" header="Film" sortable />
        <Column field="storeId" header="Store" style={{ width: "90px" }}
          body={(r) => `Store ${r.storeId}`} />
        <Column field="isAvailable" header="Status" style={{ width: "110px" }}
          body={(r) => (
            <Tag
              value={r.isAvailable ? "Available" : "Rented"}
              severity={r.isAvailable ? "success" : "warning"}
            />
          )}
        />
        <Column field="lastUpdate" header="Last Updated" style={{ width: "140px" }}
          body={(r) => new Date(r.lastUpdate).toLocaleDateString()} />
      </DataTable>
    </div>
  );
}
