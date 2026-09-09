import AppLayout from "../../components/layout/AppLayout";
import RoleTable from "../../components/roles/RoleTable";

// Displays the security roles page and list of system roles with user counts
export default function RolesPage() {
  return (
    <AppLayout>
      {/* Table displaying role names, assigned user count, creation dates, and detail links */}
      <RoleTable />
    </AppLayout>
  );
}

