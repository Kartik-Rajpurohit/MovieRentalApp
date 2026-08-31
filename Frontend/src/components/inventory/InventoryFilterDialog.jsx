import { useEffect } from "react";
import FilterDialog from "../common/FilterDialog";
import InventoryFilters from "./InventoryFilters";
import useFilters from "../../hooks/useFilters";

const INIT_FILTERS = { storeId: null, isAvailable: null };

export default function InventoryFilterDialog({
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
      title="Filter Inventory"
      onApply={() => {
        onApply(local);
        onHide();
      }}
      onClear={() => {
        onApply(INIT_FILTERS);
        onHide();
      }}
    >
      <InventoryFilters filters={local} setFilter={setFilter} />
    </FilterDialog>
  );
}
