import AppLayout from "../../components/layout/AppLayout";
import MovieTable from "../../components/movies/MovieTable";

// Displays the catalog of movies with rich filtering, search, sorting, and management actions
export default function MoviesPage() {
  return (
    <AppLayout>
      {/* Table displaying movie records, ratings, rental rates, categories, and actions */}
      <MovieTable />
    </AppLayout>
  );
}

