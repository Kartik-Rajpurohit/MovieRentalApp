import { useEffect } from "react";
import FilterDialog from "../common/FilterDialog";
import PaymentFilters from "./PaymentFilters";
import useFilters from "../../hooks/useFilters";

const INIT_FILTERS = {
  minAmount: null,
  maxAmount: null,
  fromDate: null,
  toDate: null,
};

export default function PaymentFilterDialog({
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
