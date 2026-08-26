import { useState } from "react";
import { Button } from "primereact/button";
import { Badge } from "primereact/badge";
import SearchBar from "../common/SearchBar";
import UserFilterDialog from "./UserFilterDialog";

export default function UserToolbar({
  search,
  onSearchChange,
  filters,
  onFiltersChange,
}) {
  const [filterVisible, setFilterVisible] = useState(false);

  const activeCount = [
    filters.name,
    filters.email,
    filters.role,
    filters.isActive !== null && filters.isActive !== undefined ? "x" : "",
  ].filter(Boolean).length;

  return (
    <>
      <UserFilterDialog
        visible={filterVisible}
        onHide={() => setFilterVisible(false)}
        filters={filters}
        onApply={onFiltersChange}
      />

      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: "12px",
          marginBottom: "16px",
        }}
      >
        <SearchBar
          value={search}
          onChange={onSearchChange}
          placeholder="Search users..."
        />

        <div style={{ position: "relative" }}>
          <Button
            label="Filters"
            icon="pi pi-sliders-h"
            outlined
            onClick={() => setFilterVisible(true)}
          />
          {activeCount > 0 && (
            <Badge
              value={activeCount}
              severity="danger"
              style={{ position: "absolute", top: "-8px", right: "-8px" }}
            />
          )}
        </div>
      </div>
    </>
  );
}
