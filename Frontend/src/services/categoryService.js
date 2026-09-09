// Handles API requests related to movie categories (genres)
import api from "./api";

// Base route for Category endpoints
const CATEGORY = "/Category";

// GET request to fetch a paginated list of categories with optional search and sorting
export const getCategories = (
  page = 1,
  pageSize = 10,
  search = "",
  sortField = "",
  sortOrder = "",
) =>
  api
    .get(CATEGORY, { params: { page, pageSize, search, sortField, sortOrder } })
    .then((r) => r.data);

// GET request to fetch category details by ID
export const getCategoryById = (id) =>
  api.get(`${CATEGORY}/${id}`).then((r) => r.data);

// POST request to create a new category
export const createCategory = (dto) =>
  api.post(CATEGORY, dto).then((r) => r.data);

// PATCH request to update an existing category
export const updateCategory = (dto) =>
  api.patch(CATEGORY, dto).then((r) => r.data);

// DELETE request to remove a category by ID
export const deleteCategory = (id) =>
  api.delete(`${CATEGORY}/${id}`).then((r) => r.data);

// GET request to fetch films linked to a specific category with pagination and search
export const getFilmsByCategory = async (
  categoryId,
  page = 1,
  pageSize = 10,
  search = "",
) => {
  const params = { page, pageSize };
  if (search) params.search = search;
  const res = await api.get(`${CATEGORY}/${categoryId}/films`, { params });
  return res.data;
};
