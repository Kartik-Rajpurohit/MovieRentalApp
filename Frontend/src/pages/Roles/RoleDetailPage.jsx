import { useParams, useNavigate, useLocation } from "react-router-dom";
import { useState, useEffect } from "react";
import { Card } from "primereact/card";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import AppLayout from "../../components/layout/AppLayout";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import StatusTag from "../../components/common/StatusTag";
import { getUsers } from "../../services/userService";
import { FIELD_LABEL, FIELD_VALUE } from "../../utils/constants";
import SearchBar from "../../components/common/SearchBar";
import usePagination from "../../hooks/usePagination";

export default function RoleDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const location = useLocation();

  // RoleTable se state pass hogi — role info ke liye
  const roleInfo = location.state;

  const [users, setUsers] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const { lazyState, onPage, reset } = usePagination(10);

  useEffect(() => {
    fetchUsers();
  }, [id, lazyState, search]);

  const fetchUsers = async () => {
    setLoading(true);
    try {
      const res = await getUsers(
        lazyState.page + 1,
        lazyState.rows,
        "",
        "",
        "",
        "",
        id,
        search,
        null,
      );

      const mapped = (res.data ?? []).map((u) => ({
        ...u,
        fullName:
          u.fullName ?? `${u.firstName ?? ""} ${u.lastName ?? ""}`.trim(),
      }));
      setUsers(mapped);
      setTotalRecords(res.totalRecords ?? 0);
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
    <AppLayout>
      <DetailPageHeader
        backPath="/roles"
        backLabel="Roles"
        title={roleInfo?.roleName ?? `Role ${id}`}
        subtitle={`${totalRecords} users assigned`}
      />

      <Card>
        {/* Info Grid */}
        {roleInfo && (
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))",
              gap: "24px",
            }}
          >
            <div>
              <p style={FIELD_LABEL}>Role ID</p>
              <p style={FIELD_VALUE}>{roleInfo.roleId}</p>
            </div>
            <div>
              <p style={FIELD_LABEL}>Role Name</p>
              <p style={{ ...FIELD_VALUE, textTransform: "capitalize" }}>
                {roleInfo.roleName}
              </p>
            </div>
            <div>
              <p style={FIELD_LABEL}>Total Users</p>
              <p style={FIELD_VALUE}>{totalRecords}</p>
            </div>
            <div>
              <p style={FIELD_LABEL}>Created At</p>
              <p style={FIELD_VALUE}>
                {roleInfo.createdAt
                  ? new Date(roleInfo.createdAt).toLocaleDateString()
                  : "—"}
              </p>
            </div>
          </div>
        )}

        {/* Users List */}
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
              className="pi pi-users"
              style={{ marginRight: "8px", color: "#6366f1" }}
            />
            Users with this Role
          </h3>

          <SearchBar
            value={search}
            onChange={onSearchChange}
            placeholder="Search users..."
          />

          <DataTable
            value={users}
            loading={loading}
            paginator
            lazy
            first={lazyState.first}
            rows={lazyState.rows}
            totalRecords={totalRecords}
            onPage={onPage}
            rowsPerPageOptions={[5, 10, 20]}
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
            currentPageReportTemplate="Showing {first} to {last} of {totalRecords}"
            emptyMessage="No users found."
            onRowClick={(e) => navigate(`/users/${e.data.userId}`)}
            rowClassName={() => "cursor-pointer"}
            tableStyle={{ minWidth: "40rem", tableLayout: "auto" }}
          >
            <Column field="userId" header="ID" style={{ width: "70px" }} />
            <Column field="fullName" header="Name" />
            <Column
              field="email"
              header="Email"
              body={(r) => (
                <span style={{ wordBreak: "break-all" }}>{r.email}</span>
              )}
            />
            <Column
              field="isActive"
              header="Status"
              style={{ width: "110px" }}
              body={(r) => <StatusTag isActive={r.isActive} />}
            />
          </DataTable>
        </div>
      </Card>
    </AppLayout>
  );
}
