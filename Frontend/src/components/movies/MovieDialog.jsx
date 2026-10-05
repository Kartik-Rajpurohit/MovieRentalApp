import { useState, useEffect } from "react";
import { Dialog } from "primereact/dialog";
import { Button } from "primereact/button";
import { createMovie, updateMovie } from "../../services/movieService";
import { getLanguages } from "../../services/languageService";
import { getCategories } from "../../services/categoryService";
import { getActors } from "../../services/actorService";
import MovieFormFields from "./MovieFormFields";

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
  specialFeatures: [],
  categoryIds: [],
  actorIds: [],
};

// Dialog modal for creating a new movie or editing an existing movie's details.
export default function MovieDialog({
  visible,
  onHide,
  onSuccess,
  mode = "add",
  movie = null,
}) {
  const isEdit = mode === "edit";

  // Form input values
  const [form, setForm] = useState(emptyForm);
  // Field-level validation error messages
  const [errors, setErrors] = useState({});
  // Submission loading state
  const [loading, setLoading] = useState(false);
  // Dropdown options loaded from backend services
  const [languages, setLanguages] = useState([]);
  const [categories, setCategories] = useState([]);
  const [actors, setActors] = useState([]);

  // Populate form fields if editing, or reset if adding new movie
  useEffect(() => {
    if (!visible) return;

    fetchDropdowns();

    if (isEdit && movie) {
      setForm({
        movieId: movie.movieId,
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
        specialFeatures: movie.specialFeatures ?? [],
        categoryIds: movie.categories?.map((c) => c.categoryId) ?? [],
        actorIds: movie.actors?.map((a) => a.actorId) ?? [],
      });
    } else {
      setForm(emptyForm);
    }

    setErrors({});
  }, [visible]);

  // Load languages, categories, and actors for the form dropdowns/selects
  const fetchDropdowns = async () => {
    const [langsRes, catsRes, actsRes] = await Promise.all([
      getLanguages(),
      getCategories(1, 100),
      getActors(1, 100),
    ]);
    const langList = langsRes?.data ?? (Array.isArray(langsRes) ? langsRes : []);
    const catList = catsRes?.data ?? (Array.isArray(catsRes) ? catsRes : []);
    const actList = actsRes?.data ?? (Array.isArray(actsRes) ? actsRes : []);

    setLanguages(langList.map((l) => ({ label: l.name, value: l.languageId ?? l.id })));
    setCategories(catList.map((c) => ({ label: c.name, value: c.categoryId ?? c.id })));
    setActors(actList.map((a) => ({ label: a.fullName ?? a.name, value: a.actorId ?? a.id })));
  };

  // Validate required form fields before submitting
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

  // Create or update the movie record via API
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
          movieId: form.movieId,
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
          specialFeatures: form.specialFeatures,
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
          specialFeatures: form.specialFeatures,
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
      <MovieFormFields
        form={form}
        setForm={setForm}
        errors={errors}
        setErrors={setErrors}
        languages={languages}
        categories={categories}
        actors={actors}
      />
    </Dialog>
  );
}
