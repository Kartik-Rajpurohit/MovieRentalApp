// Handles API requests related to movie languages
import api from "./api";

// Base route for Language endpoints
const LANGUAGE = "/Language";

// GET request to fetch languages with pagination, search, and sorting
export const getLanguages = (
  page = 1,
  pageSize = 10,
  search = "",
  sortField = "",
  sortOrder = "",
) =>
  api
    .get(LANGUAGE, {
      params: {
        page,
        pageSize,
        search: search || undefined,
        sortBy: sortField || undefined,
        sortOrder: sortOrder || undefined,
      },
    })
    .then((r) => r.data);

// GET request to fetch detailed language info by ID
export const getLanguageById = (id) =>
  api.get(`${LANGUAGE}/${id}`).then(r => r.data);

// GET request to fetch movies in a specific language with pagination and search
export const getMoviesByLanguage = (id, page = 1, pageSize = 10, search = "") =>
  api.get(`${LANGUAGE}/${id}/movies`, { params: { page, pageSize, search } }).then(r => r.data);

// POST request to create a new language record
export const createLanguage = (dto) =>
  api.post(LANGUAGE, dto).then(r => r.data);

// PATCH request to update an existing language record
export const updateLanguage = (dto) =>
  api.patch(LANGUAGE, dto).then(r => r.data);

// DELETE request to remove a language by ID
export const deleteLanguage = (id) =>
  api.delete(`${LANGUAGE}/${id}`).then(r => r.data);
