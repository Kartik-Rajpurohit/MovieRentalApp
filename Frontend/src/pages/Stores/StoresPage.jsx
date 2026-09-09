import AppLayout from "../../components/layout/AppLayout";
import StoreTable from "../../components/stores/StoreTable";

// Displays the rental stores page where administrators manage retail store locations
export default function StoresPage() {
  return (
    <AppLayout>
      {/* Table displaying store locations with manager name, city, staff count, and actions */}
      <StoreTable />
    </AppLayout>
  );
}

