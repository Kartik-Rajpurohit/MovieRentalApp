import AppLayout from "../../components/layout/AppLayout";
import CustomerTable from "../../components/customers/CustomerTable";

// Displays the customers list page with search, filters, and customer management actions
export default function CustomersPage() {
  return (
    <AppLayout>
      {/* Table displaying customer records with pagination, search, and detail links */}
      <CustomerTable />
    </AppLayout>
  );
}

