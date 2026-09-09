import AppLayout from "../../components/layout/AppLayout";
import RentalTable from "../../components/rentals/RentalTable";

// Displays movie rental transactions, rental status tracking, and return actions
export default function RentalsPage() {
  return (
    <AppLayout>
      {/* Table displaying movie rentals with customer, staff, rental date, return status, and actions */}
      <RentalTable />
    </AppLayout>
  );
}

