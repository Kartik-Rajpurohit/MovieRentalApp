import api from "./api";

const LANGUAGE = "/Language";

// Get all languages — no pagination (backend returns full list)
export const getLanguages = () =>
  api.get(LANGUAGE).then(r => r.data);

// Get language detail by id
export const getLanguageById = (id) =>
  api.get(`${LANGUAGE}/${id}/detail`).then(r => r.data);

// Get paginated films for a language
export const getFilmsByLanguage = (id, page = 1, pageSize = 10, search = "") =>
  api.get(`${LANGUAGE}/${id}/films`, { params: { page, pageSize, search } }).then(r => r.data);

// Create new language
export const createLanguage = (dto) =>
  api.post(LANGUAGE, dto).then(r => r.data);

// Update language
export const updateLanguage = (dto) =>
  api.patch(LANGUAGE, dto).then(r => r.data);

// Delete language
export const deleteLanguage = (id) =>
  api.delete(`${LANGUAGE}/${id}`).then(r => r.data);
