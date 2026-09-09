import AppLayout from "../../components/layout/AppLayout";
import AddressTable from "../../components/locations/addresses/AddressTable";

// Displays the addresses management page with address list, city filtering, and address creation
export default function AddressesPage() {
  return (
    <AppLayout>
      {/* Table displaying address records with district, postal code, phone, and actions */}
      <AddressTable />
    </AppLayout>
  );
}

