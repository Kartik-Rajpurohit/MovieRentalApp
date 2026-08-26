import { useState, useEffect } from "react";
import FilterDialog from "../common/FilterDialog";
import UserFilters from "./UserFilters";
import useFilters from "../../hooks/useFilters";
import { getRoles } from "../../services/userService";

const INIT_FILTERS = { name: "", email: "", role: null, isActive: null };

export default function UserFilterDialog({
  visible,
  onHide,
  filters,
  onApply,
}) {
  const { filters: local, setFilter, setFilters: setLocal, reset: resetLocal } = useFilters(filters);
  const [roles, setRoles] = useState([]);

  useEffect(() => {
    if (visible) {
      setLocal(filters);
      fetchRoles();
    }
  }, [visible]);

  const fetchRoles = async () => {
    const data = await getRoles(1, 100);
    setRoles([
      { label: "All", value: null },
      ...data.map((r) => ({ label: r.name, value: r.id })),
    ]);
  };

  const handleApply = () => {
    onApply(local);
    onHide();
  };
  const handleClear = () => {
    onApply(INIT_FILTERS);
    onHide();
  };

  return (
    <FilterDialog
      visible={visible}
      onHide={onHide}
      title="Filter Users"
      onApply={handleApply}
      onClear={handleClear}
    >
      <UserFilters filters={local} setFilter={setFilter} roles={roles} />
    </FilterDialog>
  );
}
