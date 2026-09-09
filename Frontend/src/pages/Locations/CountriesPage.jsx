import AppLayout from "../../components/layout/AppLayout";
import CountryTable from "../../components/locations/countries/CountryTable";

// Displays the countries management page with country list, city counts, and country creation
export default function CountriesPage() {
  return (
    <AppLayout>
      {/* Table displaying countries with associated city counts and actions to edit or delete */}
      <CountryTable />
    </AppLayout>
  );
}

