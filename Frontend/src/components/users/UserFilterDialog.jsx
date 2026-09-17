import { useState, useEffect } from "react";
import FilterDialog from "../common/FilterDialog";
import UserFilters from "./UserFilters";
import useFilters from "../../hooks/useFilters";
import { getRoles } from "../../services/roleService";

// Default filter values for users list
const INIT_FILTERS = { name: "", email: "", role: null, isActive: null };

// Modal filter dialog for users list, filtering by name, email, role, and active status.
export default function UserFilterDialog({
  visible,
  onHide,
  filters,
  onApply,
}) {
  // Local state holding filter values before applying
  const { filters: local, setFilter, setFilters: setLocal } = useFilters(filters);
  // Available system roles for the dropdown
  const [roles, setRoles] = useState([]);

  // Fetch roles and sync local filters when dialog opens
  useEffect(() => {
    if (visible) {
      setLocal(filters);
      fetchRoles();
    }
  }, [visible]);


  const fetchRoles = async () => {
    const res = await getRoles(1, 100);
    const data = res?.data ?? (Array.isArray(res) ? res : []);
    setRoles([
      { label: "All", value: null },
      ...data.map((r) => ({ label: r.roleName ?? r.name, value: r.roleId ?? r.id })),
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
