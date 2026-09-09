// Custom hook providing reusable filter state and updater functions for data tables
import { useState } from "react";

export default function useFilters(initial = {}) {
  // Current key-value pairs of applied filters
  const [filters, setFilters] = useState(initial);

  // Returns a function to update a specific filter property
  const setFilter = (key) => (value) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  // Resets all filters back to their initial default values
  const reset = () => setFilters(initial);

  // Return filter values and updater functions
  return { filters, setFilter, reset, setFilters };
}
