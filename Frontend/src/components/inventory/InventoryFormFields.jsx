import { useState, useEffect } from "react";
import { AutoComplete } from "primereact/autocomplete";
import { Dropdown } from "primereact/dropdown";
import { getMovies } from "../../services/movieService";

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
  stores,
  loadingStores,
}) {
  const [selectedMovie, setSelectedMovie] = useState(null);
  const [movieSuggestions, setMovieSuggestions] = useState([]);

  // Reset selected movie whenever form.movieId is cleared
  useEffect(() => {
    if (!form.movieId) {
      setSelectedMovie(null);
    }
  }, [form.movieId]);

  // Debounced server-side movie search using GET /api/Movie?page=1&pageSize=10&search={typedText}
  const searchMovies = async (event) => {
    const query = event.query?.trim();
    if (!query) {
      setMovieSuggestions([]);
      return;
    }
    try {
      const res = await getMovies(1, 10, "title", "asc", query);
      setMovieSuggestions(res.data ?? []);
    } catch (err) {
      console.error("Failed to search movies:", err);
      setMovieSuggestions([]);
    }
  };

  // Suggestion item template displaying:
  // Title
  // 2006 · #1
  const movieItemTemplate = (item) => (
    <div style={{ display: "flex", flexDirection: "column", gap: "2px" }}>
      <span style={{ fontWeight: 600, color: "#1f2937" }}>{item.title}</span>
      <span style={{ fontSize: "12px", color: "#6b7280" }}>
        {item.releaseYear ? `${item.releaseYear} · ` : ""}#{item.movieId}
      </span>
    </div>
  );

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

      {/* Server-side AutoComplete Movie Selection — Add Mode */}
      {!isEdit && (
        <div>
          <label style={labelStyle}>Movie</label>
          <AutoComplete
            value={selectedMovie}
            suggestions={movieSuggestions}
            completeMethod={searchMovies}
            field="title"
            delay={300}
            placeholder="Type to search movies..."
            itemTemplate={movieItemTemplate}
            onChange={(e) => {
              setSelectedMovie(e.value);
              if (e.value && typeof e.value === "object" && e.value.movieId) {
                setForm((prev) => ({ ...prev, movieId: e.value.movieId }));
              } else {
                setForm((prev) => ({ ...prev, movieId: null }));
              }
            }}
            style={{ width: "100%" }}
            inputStyle={{ width: "100%" }}
            className={errors.movieId ? "p-invalid" : ""}
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
