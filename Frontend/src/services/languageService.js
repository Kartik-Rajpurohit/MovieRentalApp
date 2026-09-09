// Handles API requests related to movie languages
import api from "./api";

// Base route for Language endpoints
const LANGUAGE = "/Language";

// GET request to fetch all supported languages
export const getLanguages = () =>
  api.get(LANGUAGE).then(r => r.data);

// GET request to fetch detailed language info by ID
export const getLanguageById = (id) =>
  api.get(`${LANGUAGE}/${id}/detail`).then(r => r.data);

// GET request to fetch films in a specific language with pagination and search
export const getFilmsByLanguage = (id, page = 1, pageSize = 10, search = "") =>
  api.get(`${LANGUAGE}/${id}/films`, { params: { page, pageSize, search } }).then(r => r.data);

// POST request to create a new language record
export const createLanguage = (dto) =>
  api.post(LANGUAGE, dto).then(r => r.data);

// PATCH request to update an existing language record
export const updateLanguage = (dto) =>
  api.patch(LANGUAGE, dto).then(r => r.data);

// DELETE request to remove a language by ID
export const deleteLanguage = (id) =>
  api.delete(`${LANGUAGE}/${id}`).then(r => r.data);
