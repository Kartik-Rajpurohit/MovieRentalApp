import { useEffect } from "react";
import FilterDialog from "../common/FilterDialog";
import PaymentFilters from "./PaymentFilters";
import useFilters from "../../hooks/useFilters";

// Initial/default filter state for payment queries
const INIT_FILTERS = {
  minAmount: null,
  maxAmount: null,
  fromDate: null,
  toDate: null,
};

// Filter dialog for payments allowing filtering by min/max amount and date range.
export default function PaymentFilterDialog({
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

  // Sync dialog local filters when dialog opens
  useEffect(() => {
    if (visible) setLocal(filters);
  }, [visible]);


  return (
    <FilterDialog
      visible={visible}
      onHide={onHide}
      title="Filter Payments"
      onApply={() => {
        onApply(local);
        onHide();
      }}
      onClear={() => {
        onApply(INIT_FILTERS);
        onHide();
      }}
    >
      <PaymentFilters filters={local} setFilter={setFilter} />
    </FilterDialog>
  );
}
