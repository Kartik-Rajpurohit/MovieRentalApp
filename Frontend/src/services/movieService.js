import api from "./api";

const MOVIE = "/Movie";

// Get paginated, filtered, sorted list of movies
export const getMovies = (
  page = 1,
  pageSize = 10,
  sortField = "",
  sortOrder = "",
  search = "",
  languageId = null,
  categoryId = null,
  rating = null,
  releaseYear = null,
  minRentalRate = null,
  maxRentalRate = null,
  minLength = null,
  maxLength = null,
) =>
  api
    .get(MOVIE, {
      params: {
        page,
        pageSize,
        sortField,
        sortOrder,
        search,
        languageId,
        categoryId,
        rating,
        releaseYear,
        minRentalRate,
        maxRentalRate,
        minLength,
        maxLength,
      },
    })
    .then((r) => r.data);

// Get single movie detail by ID
export const getMovieById = (id) =>
  api.get(`${MOVIE}/${id}`).then((r) => r.data);

// Create a new movie
export const createMovie = (dto) => api.post(MOVIE, dto).then((r) => r.data);

// Partial update a movie
export const updateMovie = (dto) => api.patch(MOVIE, dto).then((r) => r.data);

// Delete a movie
export const deleteMovie = (id) =>
  api.delete(`${MOVIE}/${id}`).then((r) => r.data);

// Dropdowns for add/edit form
export const getLanguages = (page = 1, pageSize = 100) =>
  api
    .get(`${MOVIE}/languages`, { params: { page, pageSize } })
    .then((r) => r.data);

export const getCategories = (page = 1, pageSize = 100) =>
  api
    .get(`${MOVIE}/categories`, { params: { page, pageSize } })
    .then((r) => r.data);

export const getActors = (page = 1, pageSize = 100) =>
  api
    .get(`${MOVIE}/actors`, { params: { page, pageSize } })
    .then((r) => r.data);
