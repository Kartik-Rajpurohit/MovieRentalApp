import AppLayout from "../../components/layout/AppLayout";
import CategoryTable from "../../components/categories/CategoryTable";

// Displays the movie categories management page with data table, creation, and editing
export default function CategoriesPage() {
  return (
    <AppLayout>
      {/* Table displaying movie categories with actions to add, edit, and delete */}
      <CategoryTable />
    </AppLayout>
  );
}

