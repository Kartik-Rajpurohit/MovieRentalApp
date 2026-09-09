import AppLayout from "../../components/layout/AppLayout";
import UserTable from "../../components/users/UserTable";

// Displays the user management page where administrators can create, edit, filter, and view users
export default function UsersPage() {
  return (
    <AppLayout>
      {/* Table displaying system users with role filtering, status toggling, and actions */}
      <UserTable />
    </AppLayout>
  );
}

