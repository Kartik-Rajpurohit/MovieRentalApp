import AppLayout from "../../components/layout/AppLayout";
import LanguageTable from "../../components/languages/LanguageTable";

// Displays the languages management page with language list, creation, and editing
export default function LanguagesPage() {
  return (
    <AppLayout>
      {/* Table displaying supported movie languages with actions to add, edit, and delete */}
      <LanguageTable />
    </AppLayout>
  );
}

