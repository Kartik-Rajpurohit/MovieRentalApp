import AppLayout from "../../components/layout/AppLayout";
import CityTable from "../../components/locations/cities/CityTable";

// Displays the cities management page with country filtering and city creation
export default function CitiesPage() {
  return (
    <AppLayout>
      {/* Table displaying cities with country names, address counts, and actions */}
      <CityTable />
    </AppLayout>
  );
}

