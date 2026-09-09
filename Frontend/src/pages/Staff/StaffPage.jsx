import AppLayout from "../../components/layout/AppLayout";
import StaffTable from "../../components/staff/StaffTable";

// Displays the staff management page where administrators manage staff members and store assignments
export default function StaffPage() {
  return (
    <AppLayout>
      {/* Table displaying staff members with email, store, active status, and actions */}
      <StaffTable />
    </AppLayout>
  );
}

