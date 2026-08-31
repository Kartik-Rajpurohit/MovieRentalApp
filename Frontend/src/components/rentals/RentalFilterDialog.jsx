import { useEffect } from "react";
import FilterDialog from "../common/FilterDialog";
import RentalFilters from "./RentalFilters";
import useFilters from "../../hooks/useFilters";

const INIT_FILTERS = { isReturned: null, customerId: null, staffId: null };

export default function RentalFilterDialog({
  visible,
  onHide,
  filters,
  onApply,
}) {
  const {
    filters: local,
    setFilter,
    setFilters: setLocal,
  } = useFilters(INIT_FILTERS);

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
