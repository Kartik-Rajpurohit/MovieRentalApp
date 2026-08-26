import { useEffect, useState } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { Button } from "primereact/button";
import SearchBar from "../common/SearchBar";
import PageHeader from "../common/PageHeader";
import usePagination from "../../hooks/usePagination";
import { getRoles } from "../../services/roleService";
import { useNavigate } from "react-router-dom";

export default function RoleTable() {
  const { lazyState, onPage, reset } = usePagination(10);

  const [roles, setRoles] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    loadRoles();
  }, [lazyState, search]);

  const loadRoles = async () => {
    setLoading(true);
    try {
      const res = await getRoles(lazyState.page + 1, lazyState.rows, search);
      setRoles(res.data ?? []);
      setTotalRecords(res.totalRecords);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const onSearchChange = (val) => {
    setSearch(val);
    reset();
  };

  return (
    <div>
      <PageHeader title="Roles" onAdd={null} />

      <div style={{ marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={onSearchChange}
          placeholder="Search roles..."
        />
      </div>

      <DataTable
        value={roles}
        paginator
        lazy
        loading={loading}
        first={lazyState.first}
        rows={lazyState.rows}
        totalRecords={totalRecords}
        onPage={onPage}
        rowsPerPageOptions={[5, 10, 20]}
        emptyMessage="No roles found."
        onRowClick={(e) =>
          navigate(`/roles/${e.data.roleId}`, { state: e.data })
        }
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
      >
        <Column field="roleId" header="ID" style={{ width: "70px" }} />
        <Column
          field="roleName"
          header="Role Name"
          body={(r) => (
            <span style={{ textTransform: "capitalize" }}>{r.roleName}</span>
          )}
        />
        <Column
          field="createdAt"
          header="Created At"
          style={{ width: "160px" }}
          body={(r) => new Date(r.createdAt).toLocaleDateString()}
        />
        <Column
          field="updatedAt"
          header="Updated At"
          style={{ width: "160px" }}
          body={(r) =>
            r.updatedAt ? new Date(r.updatedAt).toLocaleDateString() : "—"
          }
        />
      </DataTable>
    </div>
  );
}
