import { useState, useEffect } from "react";
import { Dialog } from "primereact/dialog";
import { InputText } from "primereact/inputtext";
import { InputTextarea } from "primereact/inputtextarea";
import { InputNumber } from "primereact/inputnumber";
import { Dropdown } from "primereact/dropdown";
import { MultiSelect } from "primereact/multiselect";
import { Button } from "primereact/button";
import {
  createMovie,
  updateMovie,
  getLanguages,
  getCategories,
  getActors,
} from "../../services/movieService";

// MPAA rating options
const RATING_OPTIONS = [
  { label: "G", value: "G" },
  { label: "PG", value: "PG" },
  { label: "PG-13", value: "PG-13" },
  { label: "R", value: "R" },
  { label: "NC-17", value: "NC-17" },
];

const emptyForm = {
  title: "",
  description: "",
  releaseYear: null,
  languageId: null,
  originalLanguageId: null,
  rentalDuration: 3,
  rentalRate: 4.99,
  length: null,
  replacementCost: 19.99,
  rating: null,
  categoryIds: [],
  actorIds: [],
};

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

export default function MovieDialog({
  visible,
  onHide,
  onSuccess,
  mode = "add",
  movie = null,
}) {
  const isEdit = mode === "edit";

  const [form, setForm] = useState(emptyForm);
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [languages, setLanguages] = useState([]);
  const [categories, setCategories] = useState([]);
  const [actors, setActors] = useState([]);

  useEffect(() => {
    if (!visible) return;

    fetchDropdowns();

    if (isEdit && movie) {
      setForm({
        filmId: movie.filmId,
        title: movie.title ?? "",
        description: movie.description ?? "",
        releaseYear: movie.releaseYear ?? null,
        languageId: movie.languageId ?? null,
        originalLanguageId: movie.originalLanguageId ?? null,
        rentalDuration: movie.rentalDuration ?? 3,
        rentalRate: movie.rentalRate ?? 4.99,
        length: movie.length ?? null,
        replacementCost: movie.replacementCost ?? 19.99,
        rating: movie.rating ?? null,
        categoryIds: movie.categories?.map((c) => c.categoryId) ?? [],
        actorIds: movie.actors?.map((a) => a.actorId) ?? [],
      });
    } else {
      setForm(emptyForm);
    }

    setErrors({});
  }, [visible]);

  const fetchDropdowns = async () => {
    const [langs, cats, acts] = await Promise.all([
      getLanguages(),
      getCategories(),
      getActors(),
    ]);
    setLanguages(langs.map((l) => ({ label: l.name, value: l.id })));
    setCategories(cats.map((c) => ({ label: c.name, value: c.id })));
    setActors(acts.map((a) => ({ label: a.name, value: a.id })));
  };

  const handleChange = (field) => (e) => {
    const value = e.target !== undefined ? e.target.value : e.value;
    setForm((prev) => ({ ...prev, [field]: value }));
    setErrors((prev) => ({ ...prev, [field]: undefined }));
  };

  const validate = () => {
    const e = {};
    if (!form.title?.trim()) e.title = "Title is required";
    if (!form.languageId) e.languageId = "Language is required";
    if (!form.rentalDuration) e.rentalDuration = "Rental duration is required";
    if (!form.rentalRate) e.rentalRate = "Rental rate is required";
    if (!form.replacementCost)
      e.replacementCost = "Replacement cost is required";
    return e;
  };

  const handleSubmit = async () => {
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    setLoading(true);
    try {
      if (isEdit) {
        await updateMovie({
          filmId: form.filmId,
          title: form.title,
          description: form.description,
          releaseYear: form.releaseYear,
          languageId: form.languageId,
          originalLanguageId: form.originalLanguageId,
          rentalDuration: form.rentalDuration,
          rentalRate: form.rentalRate,
          length: form.length,
          replacementCost: form.replacementCost,
          rating: form.rating,
          categoryIds: form.categoryIds,
          actorIds: form.actorIds,
        });
      } else {
        await createMovie({
          title: form.title,
          description: form.description,
          releaseYear: form.releaseYear,
          languageId: form.languageId,
          originalLanguageId: form.originalLanguageId,
          rentalDuration: form.rentalDuration,
          rentalRate: form.rentalRate,
          length: form.length,
          replacementCost: form.replacementCost,
          rating: form.rating,
          categoryIds: form.categoryIds,
          actorIds: form.actorIds,
        });
      }

      setForm(emptyForm);
      setErrors({});
      onSuccess();
      onHide();
    } catch (error) {
      setErrors({
        submit: error?.response?.data ?? "Something went wrong.",
      });
    } finally {
      setLoading(false);
    }
  };

  const footer = (
    <div style={{ display: "flex", gap: "8px", justifyContent: "flex-end" }}>
      <Button
        label="Cancel"
        icon="pi pi-times"
        severity="secondary"
        outlined
        onClick={onHide}
        disabled={loading}
      />
      <Button
        label={isEdit ? "Save Changes" : "Create Movie"}
        icon="pi pi-check"
        onClick={handleSubmit}
        loading={loading}
      />
    </div>
  );

  return (
    <Dialog
      header={isEdit ? "Edit Movie" : "Add New Movie"}
      visible={visible}
      onHide={onHide}
      footer={footer}
      style={{ width: "560px" }}
      modal
    >
      <div
        style={{
          display: "flex",
          flexDirection: "column",
          gap: "16px",
          paddingTop: "8px",
        }}
      >
        {/* Title */}
        <div>
          <label style={labelStyle}>Title</label>
          <InputText
            value={form.title}
            onChange={handleChange("title")}
            placeholder="Enter movie title"
            style={{ width: "100%" }}
            className={errors.title ? "p-invalid" : ""}
          />
          {errors.title && <small className="p-error">{errors.title}</small>}
        </div>

        {/* Description */}
        <div>
          <label style={labelStyle}>Description</label>
          <InputTextarea
            value={form.description}
            onChange={handleChange("description")}
            placeholder="Enter description"
            rows={3}
            style={{ width: "100%" }}
          />
        </div>

        {/* Release Year + Length */}
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "1fr 1fr",
            gap: "16px",
          }}
        >
          <div>
            <label style={labelStyle}>Release Year</label>
            <InputNumber
              value={form.releaseYear}
              onValueChange={(e) =>
                setForm((p) => ({ ...p, releaseYear: e.value }))
              }
              placeholder="e.g. 2006"
              useGrouping={false}
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
            />
          </div>
          <div>
            <label style={labelStyle}>Length (min)</label>
            <InputNumber
              value={form.length}
              onValueChange={(e) => setForm((p) => ({ ...p, length: e.value }))}
              placeholder="e.g. 120"
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
            />
          </div>
        </div>

        {/* Language + Original Language */}
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "1fr 1fr",
            gap: "16px",
          }}
        >
          <div>
            <label style={labelStyle}>Language</label>
            <Dropdown
              value={form.languageId}
              options={languages}
              onChange={(e) => setForm((p) => ({ ...p, languageId: e.value }))}
              placeholder="Select language"
              style={{ width: "100%" }}
              appendTo="self"
              className={errors.languageId ? "p-invalid" : ""}
            />
            {errors.languageId && (
              <small className="p-error">{errors.languageId}</small>
            )}
          </div>
          <div>
            <label style={labelStyle}>Original Language</label>
            <Dropdown
              value={form.originalLanguageId}
              options={languages}
              onChange={(e) =>
                setForm((p) => ({ ...p, originalLanguageId: e.value }))
              }
              placeholder="Optional"
              style={{ width: "100%" }}
              appendTo="self"
              showClear
            />
          </div>
        </div>

        {/* Rental Duration + Rental Rate + Replacement Cost */}
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "1fr 1fr 1fr",
            gap: "16px",
          }}
        >
          <div>
            <label style={labelStyle}>Rental Days</label>
            <InputNumber
              value={form.rentalDuration}
              onValueChange={(e) =>
                setForm((p) => ({ ...p, rentalDuration: e.value }))
              }
              min={1}
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
              className={errors.rentalDuration ? "p-invalid" : ""}
            />
            {errors.rentalDuration && (
              <small className="p-error">{errors.rentalDuration}</small>
            )}
          </div>
          <div>
            <label style={labelStyle}>Rental Rate ($)</label>
            <InputNumber
              value={form.rentalRate}
              onValueChange={(e) =>
                setForm((p) => ({ ...p, rentalRate: e.value }))
              }
              mode="decimal"
              minFractionDigits={2}
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
              className={errors.rentalRate ? "p-invalid" : ""}
            />
            {errors.rentalRate && (
              <small className="p-error">{errors.rentalRate}</small>
            )}
          </div>
          <div>
            <label style={labelStyle}>Replace Cost ($)</label>
            <InputNumber
              value={form.replacementCost}
              onValueChange={(e) =>
                setForm((p) => ({ ...p, replacementCost: e.value }))
              }
              mode="decimal"
              minFractionDigits={2}
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
              className={errors.replacementCost ? "p-invalid" : ""}
            />
            {errors.replacementCost && (
              <small className="p-error">{errors.replacementCost}</small>
            )}
          </div>
        </div>

        {/* Rating */}
        <div>
          <label style={labelStyle}>MPAA Rating</label>
          <Dropdown
            value={form.rating}
            options={RATING_OPTIONS}
            onChange={(e) => setForm((p) => ({ ...p, rating: e.value }))}
            placeholder="Select rating"
            style={{ width: "100%" }}
            appendTo="self"
            showClear
          />
        </div>

        {/* Categories */}
        <div>
          <label style={labelStyle}>Categories</label>
          <MultiSelect
            value={form.categoryIds}
            options={categories}
            onChange={(e) => setForm((p) => ({ ...p, categoryIds: e.value }))}
            placeholder="Select categories"
            style={{ width: "100%" }}
            display="chip"
          />
        </div>

        {/* Actors */}
        <div>
          <label style={labelStyle}>Actors</label>
          <MultiSelect
            value={form.actorIds}
            options={actors}
            onChange={(e) => setForm((p) => ({ ...p, actorIds: e.value }))}
            placeholder="Select actors"
            style={{ width: "100%" }}
            display="chip"
            filter
          />
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
