import { useEffect } from "react";
import FilterDialog from "../common/FilterDialog";
import RentalFilters from "./RentalFilters";
import useFilters from "../../hooks/useFilters";

// Default filter criteria for rentals
const INIT_FILTERS = { isReturned: null, customerId: null, staffId: null };

// Modal dialog allowing users to filter rentals by return status, customer ID, and staff ID.
export default function RentalFilterDialog({
  visible,
  onHide,
  filters,
  onApply,
}) {
  // Local state holding filter values before user applies them
  const {
    filters: local,
    setFilter,
    setFilters: setLocal,
  } = useFilters(INIT_FILTERS);

  // Sync local dialog state with applied filters when opened
  useEffect(() => {
    if (visible) setLocal(filters);
  }, [visible]);


  return (
    <FilterDialog
      visible={visible}
      onHide={onHide}
      title="Filter Rentals"
      onApply={() => {
        onApply(local);
        onHide();
      }}
      onClear={() => {
        onApply(INIT_FILTERS);
        onHide();
      }}
    >
      <RentalFilters filters={local} setFilter={setFilter} />
    </FilterDialog>
  );
}
