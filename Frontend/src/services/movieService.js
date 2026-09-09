// Handles API requests related to movies/films catalogue, filtering, and dropdown options
import api from "./api";

// Base route for Movie endpoints
const MOVIE = "/Movie";

// GET request to fetch a paginated, sorted, and filtered list of movies
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

// GET request to fetch complete movie details by ID
export const getMovieById = (id) =>
  api.get(`${MOVIE}/${id}`).then((r) => r.data);

// POST request to create a new movie record
export const createMovie = (dto) => api.post(MOVIE, dto).then((r) => r.data);

// PATCH request to partially update an existing movie record
export const updateMovie = (dto) => api.patch(MOVIE, dto).then((r) => r.data);

// DELETE request to remove a movie by ID
export const deleteMovie = (id) =>
  api.delete(`${MOVIE}/${id}`).then((r) => r.data);

// GET request to fetch languages for dropdown selection
export const getLanguages = (page = 1, pageSize = 100) =>
  api
    .get(`${MOVIE}/languages`, { params: { page, pageSize } })
    .then((r) => r.data);

// GET request to fetch categories for dropdown selection
export const getCategories = (page = 1, pageSize = 100) =>
  api
    .get(`${MOVIE}/categories`, { params: { page, pageSize } })
    .then((r) => r.data);

// GET request to fetch actors for dropdown selection
export const getActors = (page = 1, pageSize = 100) =>
  api
    .get(`${MOVIE}/actors`, { params: { page, pageSize } })
    .then((r) => r.data);
