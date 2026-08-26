import { useState } from "react";

export default function useFilters(initial = {}) {
  const [filters, setFilters] = useState(initial);

  const setFilter = (key) => (value) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  const reset = () => setFilters(initial);

  return { filters, setFilter, reset, setFilters };
}
