import api from "./api";

const CATEGORY = "/Category";

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

export const getCategoryById = (id) =>
  api.get(`${CATEGORY}/${id}`).then((r) => r.data);

export const createCategory = (dto) =>
  api.post(CATEGORY, dto).then((r) => r.data);

export const updateCategory = (dto) =>
  api.patch(CATEGORY, dto).then((r) => r.data);

export const deleteCategory = (id) =>
  api.delete(`${CATEGORY}/${id}`).then((r) => r.data);

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
