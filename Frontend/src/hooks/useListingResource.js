// Custom hook providing unified state management for listing pages: pagination, search, sorting, filtering, modal state, and error handling
import { useState, useEffect, useCallback } from "react";
import usePagination from "./usePagination";
import useFilters from "./useFilters";
import useDialog from "./useDialog";

export default function useListingResource({
  fetchFn,
  initialFilters = {},
  defaultSortField = "id",
  defaultSortOrder = -1,
  pageSize = 10,
}) {
  const { lazyState, onPage, reset: resetPagination } = usePagination(pageSize);
  const { filters, setFilter, reset: resetFilters, setFilters } = useFilters(initialFilters);
  const addDialog = useDialog();

  const [items, setItems] = useState([]);
  const [totalRecords, setTotalRecords] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [search, setSearch] = useState("");
  const [sortField, setSortField] = useState(defaultSortField);
  const [sortOrder, setSortOrder] = useState(defaultSortOrder);

  const onSearchChange = (val) => {
    setSearch(val);
    resetPagination();
  };

  const onSort = (e) => {
    setSortField(e.sortField);
    setSortOrder(e.sortOrder);
    resetPagination();
  };

  const loadData = useCallback(async () => {
    if (!fetchFn) return;
    setLoading(true);
    setError(null);
    try {
      const res = await fetchFn({
        page: lazyState.page + 1,
        pageSize: lazyState.rows,
        search,
        sortField,
        sortOrder: sortOrder === 1 ? "asc" : "desc",
        filters,
      });
      if (res?.data !== undefined) {
        setItems(res.data ?? []);
        setTotalRecords(res.totalRecords ?? 0);
      } else if (Array.isArray(res)) {
        setItems(res);
        setTotalRecords(res.length);
      } else {
        setItems([]);
        setTotalRecords(0);
      }
    } catch (err) {
      console.error(err);
      setError(err);
    } finally {
      setLoading(false);
    }
  }, [fetchFn, lazyState, search, sortField, sortOrder, filters]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  return {
    items,
    setItems,
    totalRecords,
    setTotalRecords,
    loading,
    error,
    search,
    onSearchChange,
    setSearch,
    sortField,
    sortOrder,
    onSort,
    setSortField,
    setSortOrder,
    lazyState,
    onPage,
    resetPagination,
    filters,
    setFilter,
    resetFilters,
    setFilters,
    addDialog,
    reload: loadData,
  };
}
