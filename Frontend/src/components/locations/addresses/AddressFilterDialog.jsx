import { useEffect } from "react";
import FilterDialog from "../../common/FilterDialog";
import AddressFilters from "./AddressFilters";
import useFilters from "../../../hooks/useFilters";

const INIT_FILTERS = { city: null, district: null, postalCode: null };

export default function AddressFilterDialog({
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
      title="Filter Addresses"
      onApply={() => {
        onApply(local);
        onHide();
      }}
      onClear={() => {
        onApply(INIT_FILTERS);
        onHide();
      }}
    >
      <AddressFilters filters={local} setFilter={setFilter} />
    </FilterDialog>
  );
}
