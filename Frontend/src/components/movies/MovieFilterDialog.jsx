import { useState, useEffect } from "react";
import FilterDialog from "../common/FilterDialog";
import { Dropdown } from "primereact/dropdown";
import { InputNumber } from "primereact/inputnumber";
import useFilters from "../../hooks/useFilters";
import { getLanguages, getCategories } from "../../services/movieService";

// MPAA rating filter options
const RATING_OPTIONS = [
  { label: "All", value: null },
  { label: "G", value: "G" },
  { label: "PG", value: "PG" },
  { label: "PG-13", value: "PG-13" },
  { label: "R", value: "R" },
  { label: "NC-17", value: "NC-17" },
];

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

const INIT_FILTERS = {
  languageId: null,
  categoryId: null,
  rating: null,
  releaseYear: null,
  minRentalRate: null,
  maxRentalRate: null,
  minLength: null,
  maxLength: null,
};

export default function MovieFilterDialog({
  visible,
  onHide,
  filters,
  onApply,
}) {
  const {
    filters: local,
    setFilter: set,
    setFilters: setLocal,
    reset: resetLocal,
  } = useFilters(filters);
  const [languages, setLanguages] = useState([]);
  const [categories, setCategories] = useState([]);

  useEffect(() => {
    if (!visible) return;
    setLocal(filters);
    fetchDropdowns();
  }, [visible]);

  const fetchDropdowns = async () => {
    const [langs, cats] = await Promise.all([getLanguages(), getCategories()]);
    setLanguages([
      { label: "All", value: null },
      ...langs.map((l) => ({ label: l.name, value: l.id })),
    ]);
    setCategories([
      { label: "All", value: null },
      ...cats.map((c) => ({ label: c.name, value: c.id })),
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
      title="Filter Movies"
      onApply={handleApply}
      onClear={handleClear}
    >
      <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
        {/* Language */}
        <div>
          <label style={labelStyle}>Language</label>
          <Dropdown
            value={local.languageId}
            options={languages}
            onChange={(e) => set("languageId")(e.value)}
            placeholder="All languages"
            style={{ width: "100%" }}
            appendTo="self"
          />
        </div>

        {/* Category */}
        <div>
          <label style={labelStyle}>Category</label>
          <Dropdown
            value={local.categoryId}
            options={categories}
            onChange={(e) => set("categoryId")(e.value)}
            placeholder="All categories"
            style={{ width: "100%" }}
            appendTo="self"
          />
        </div>

        {/* Rating */}
        <div>
          <label style={labelStyle}>Rating</label>
          <Dropdown
            value={local.rating}
            options={RATING_OPTIONS}
            onChange={(e) => set("rating")(e.value)}
            placeholder="All ratings"
            style={{ width: "100%" }}
            appendTo="self"
          />
        </div>

        {/* Release Year */}
        <div>
          <label style={labelStyle}>Release Year</label>
          <InputNumber
            value={local.releaseYear}
            onValueChange={(e) => set("releaseYear")(e.value)}
            placeholder="e.g. 2006"
            useGrouping={false}
            style={{ width: "100%" }}
            inputStyle={{ width: "100%" }}
          />
        </div>
        {/* Rental Rate Range */}
        <div>
          <label style={labelStyle}>Rental Rate ($)</label>
          <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
            <InputNumber
              value={local.minRentalRate}
              onValueChange={(e) => set("minRentalRate")(e.value)}
              placeholder="Min"
              mode="decimal"
              minFractionDigits={2}
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
            />
            <span style={{ color: "#6b7280" }}>—</span>
            <InputNumber
              value={local.maxRentalRate}
              onValueChange={(e) => set("maxRentalRate")(e.value)}
              placeholder="Max"
              mode="decimal"
              minFractionDigits={2}
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
            />
          </div>
        </div>

        {/* Length Range */}
        <div>
          <label style={labelStyle}>Length (min)</label>
          <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
            <InputNumber
              value={local.minLength}
              onValueChange={(e) => set("minLength")(e.value)}
              placeholder="Min"
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
            />
            <span style={{ color: "#6b7280" }}>—</span>
            <InputNumber
              value={local.maxLength}
              onValueChange={(e) => set("maxLength")(e.value)}
              placeholder="Max"
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
            />
          </div>
        </div>
      </div>
    </FilterDialog>
  );
}
