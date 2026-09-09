import AppLayout from "../../components/layout/AppLayout";
import PaymentTable from "../../components/payments/PaymentTable";

// Displays the payments transaction log with customer, rental, and staff payment information
export default function PaymentsPage() {
  return (
    <AppLayout>
      {/* Table displaying payments with amount, payment date, rental details, and filters */}
      <PaymentTable />
    </AppLayout>
  );
}

