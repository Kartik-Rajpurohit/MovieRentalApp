import { useState, useEffect } from "react";
import FilterDialog from "../../common/FilterDialog";
import CityFilters from "./CityFilters";
import useFilters from "../../../hooks/useFilters";
import { getCountries } from "../../../services/countryService";

const INIT_FILTERS = { countryId: null };

export default function CityFilterDialog({
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
  const [countries, setCountries] = useState([
    { label: "All Countries", value: null },
  ]);

  useEffect(() => {
    if (visible) {
      setLocal(filters);
      fetchCountries();
    }
  }, [visible]);

  const fetchCountries = async () => {
    const data = await getCountries(1, 300);
    setCountries([
      { label: "All Countries", value: null },
      ...(data.data ?? []).map((c) => ({ label: c.name, value: c.countryId })),
    ]);
  };

  return (
    <FilterDialog
      visible={visible}
      onHide={onHide}
      title="Filter Cities"
      onApply={() => {
        onApply(local);
        onHide();
      }}
      onClear={() => {
        onApply(INIT_FILTERS);
        onHide();
      }}
    >
      <CityFilters
        filters={local}
        setFilter={setFilter}
        countries={countries}
      />
    </FilterDialog>
  );
}
