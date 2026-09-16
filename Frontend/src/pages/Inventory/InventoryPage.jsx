import AppLayout from "../../components/layout/AppLayout";
import InventoryTable from "../../components/inventory/InventoryTable";

// Displays the inventory management page where staff and admins manage movie copies across stores
export default function InventoryPage() {
  return (
    <AppLayout>
      {/* Table displaying inventory items with movie title, store, availability status, and actions */}
      <InventoryTable />
    </AppLayout>
  );
}

