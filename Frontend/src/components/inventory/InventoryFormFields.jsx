import { Dropdown } from "primereact/dropdown";

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

// Form input fields for creating a new inventory copy or changing store assignment of an existing copy
export default function InventoryFormFields({
  form,
  setForm,
  errors,
  isEdit,
  inventory,
  movies,
  stores,
  loadingMovies,
  loadingStores,
}) {
  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "16px", paddingTop: "8px" }}>
      {/* Read-only Movie Display in Edit Mode */}
      {isEdit && (
        <div>
          <label style={labelStyle}>Movie</label>
          <div
            style={{
              padding: "9px 12px",
              background: "#f9fafb",
              border: "1px solid #e5e7eb",
              borderRadius: "6px",
              fontSize: "14px",
              color: "#111827",
              fontWeight: 500,
            }}
          >
            {inventory?.movieTitle
              ? `#${inventory.movieId} — ${inventory.movieTitle}`
              : `Movie #${form.movieId}`}
          </div>
        </div>
      )}

      {/* Searchable Movie Dropdown — Add Mode */}
      {!isEdit && (
        <div>
          <label style={labelStyle}>Movie</label>
          <Dropdown
            value={form.movieId}
            options={movies}
            onChange={(e) => setForm((prev) => ({ ...prev, movieId: e.value }))}
            placeholder={loadingMovies ? "Loading movies..." : "Select a movie"}
            filter
            filterBy="label"
            showClear
            virtualScrollerOptions={{ itemSize: 38 }}
            style={{ width: "100%" }}
            className={errors.movieId ? "p-invalid" : ""}
            disabled={loadingMovies}
          />
          {errors.movieId && <small className="p-error">{errors.movieId}</small>}
        </div>
      )}

      {/* Store Dropdown */}
      <div>
        <label style={labelStyle}>Store</label>
        <Dropdown
          value={form.storeId}
          options={stores}
          onChange={(e) => setForm((prev) => ({ ...prev, storeId: e.value }))}
          placeholder={loadingStores ? "Loading stores..." : "Select a store"}
          style={{ width: "100%" }}
          className={errors.storeId ? "p-invalid" : ""}
          disabled={loadingStores}
        />
        {errors.storeId && <small className="p-error">{errors.storeId}</small>}
      </div>

      {/* Submit Error */}
      {errors.submit && (
        <small className="p-error" style={{ textAlign: "center" }}>
          {errors.submit}
        </small>
      )}
    </div>
  );
}
