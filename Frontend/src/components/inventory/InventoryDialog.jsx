import { useState, useEffect } from "react";
import { Dialog } from "primereact/dialog";
import { Dropdown } from "primereact/dropdown";
import { Button } from "primereact/button";
import { createInventory, updateInventory } from "../../services/inventoryService";
import { getStores } from "../../services/storeService";
import { getMovies } from "../../services/movieService";

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

const emptyForm = { movieId: null, storeId: null };

// Modal dialog for adding a new movie inventory copy or editing store assignment of an existing copy
export default function InventoryDialog({
  visible,
  onHide,
  onSuccess,
  mode = "add",
  inventory = null,
}) {
  const isEdit = mode === "edit";
  // Form input state (movieId and storeId)
  const [form, setForm] = useState(emptyForm);
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  // Dropdown options and loading states
  const [stores, setStores] = useState([]);
  const [movies, setMovies] = useState([]);
  const [loadingStores, setLoadingStores] = useState(false);
  const [loadingMovies, setLoadingMovies] = useState(false);

  // Load stores and movies when dialog opens
  useEffect(() => {
    if (!visible) return;

    if (isEdit && inventory) {
      setForm({
        inventoryId: inventory.inventoryId,
        movieId: inventory.movieId,
        storeId: inventory.storeId,
      });
    } else {
      setForm(emptyForm);
    }
    setErrors({});

    // Fetch stores if not already loaded
    if (stores.length === 0) {
      setLoadingStores(true);
      getStores(1, 100)
        .then((res) => {
          const storeList = (res.data ?? []).map((s) => ({
            label: s.cityName
              ? `Store #${s.storeId} — ${s.cityName}${s.street ? ` (${s.street})` : ""}`
              : `Store #${s.storeId}`,
            value: s.storeId,
          }));

          // Fallback to default Store 1 & Store 2 if none returned
          setStores(
            storeList.length > 0
              ? storeList
              : [
                  { label: "Store #1", value: 1 },
                  { label: "Store #2", value: 2 },
                ]
          );
        })
        .catch(() => {
          setStores([
            { label: "Store #1", value: 1 },
            { label: "Store #2", value: 2 },
          ]);
        })
        .finally(() => setLoadingStores(false));
    }

    // Fetch movies catalogue for selection if not in edit mode and not already loaded
    if (!isEdit && movies.length === 0) {
      setLoadingMovies(true);
      getMovies(1, 1000, "title", "asc")
        .then((res) => {
          const movieList = (res.data ?? []).map((m) => ({
            label: `#${m.movieId} — ${m.title}${m.releaseYear ? ` (${m.releaseYear})` : ""}`,
            value: m.movieId,
          }));
          setMovies(movieList);
        })
        .catch(console.error)
        .finally(() => setLoadingMovies(false));
    }
  }, [visible]);

  // Validate movie and store selection
  const validate = () => {
    const e = {};
    if (!isEdit && !form.movieId) e.movieId = "Please select a movie";
    if (!form.storeId) e.storeId = "Please select a store";
    return e;
  };

  // Submit inventory copy data to backend create or update API
  const handleSubmit = async () => {
    const errs = validate();
    if (Object.keys(errs).length > 0) {
      setErrors(errs);
      return;
    }
    setLoading(true);
    try {
      if (isEdit) {
        await updateInventory({ inventoryId: form.inventoryId, storeId: form.storeId });
      } else {
        await createInventory({ movieId: form.movieId, storeId: form.storeId });
      }
      onSuccess();
      onHide();
    } catch (err) {
      const message =
        err?.response?.data?.detail ||
        err?.response?.data?.message ||
        err?.response?.data?.title ||
        (typeof err?.response?.data === "string" ? err?.response?.data : null) ||
        err?.message ||
        "Something went wrong.";
      setErrors({ submit: message });
    } finally {
      setLoading(false);
    }
  };

  const handleHide = () => {
    setForm(emptyForm);
    setErrors({});
    onHide();
  };

  const footer = (
    <div style={{ display: "flex", gap: "8px", justifyContent: "flex-end" }}>
      <Button
        label="Cancel"
        icon="pi pi-times"
        severity="secondary"
        outlined
        onClick={handleHide}
        disabled={loading}
      />
      <Button
        label={isEdit ? "Save Changes" : "Add Copy"}
        icon="pi pi-check"
        onClick={handleSubmit}
        loading={loading}
      />
    </div>
  );

  return (
    <Dialog
      header={isEdit ? "Edit Inventory" : "Add Inventory Copy"}
      visible={visible}
      onHide={handleHide}
      footer={footer}
      style={{ width: "460px", maxWidth: "95vw" }}
      modal
    >
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
    </Dialog>
  );
}
