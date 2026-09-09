import AppLayout from "../../components/layout/AppLayout";
import ActorTable from "../../components/actors/ActorTable";

// Displays the actors page where users can browse, search, and manage actors
export default function ActorsPage() {
  return (
    <AppLayout>
      {/* Table component that displays actor records with pagination, sorting, and actions */}
      <ActorTable />
    </AppLayout>
  );
}

