import { useState } from "react";

export default function usePagination(defaultRows = 10) {
  const [lazyState, setLazyState] = useState({
    first: 0,
    rows: defaultRows,
    page: 0,
  });

  const onPage = (e) =>
    setLazyState({ first: e.first, rows: e.rows, page: e.page });

  const reset = () => setLazyState((prev) => ({ ...prev, first: 0, page: 0 }));

  return { lazyState, onPage, reset };
}
