// Custom hook providing reusable pagination state and page-change handlers for PrimeReact data tables
import { useState } from "react";

export default function usePagination(defaultRows = 10) {
  // Tracks current record offset (first), rows per page, and page index
  const [lazyState, setLazyState] = useState({
    first: 0,
    rows: defaultRows,
    page: 0,
  });

  // Handler triggered when the user navigates pages or changes the rows-per-page dropdown
  const onPage = (e) =>
    setLazyState({ first: e.first, rows: e.rows, page: e.page });

  // Resets pagination back to the first page (offset 0)
  const reset = () => setLazyState((prev) => ({ ...prev, first: 0, page: 0 }));

  // Return pagination state and helper handlers
  return { lazyState, onPage, reset };
}
