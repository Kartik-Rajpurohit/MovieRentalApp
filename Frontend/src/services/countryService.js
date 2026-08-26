import api from "./api";

const API = "/Country";

export const getCountries = async (page = 1, pageSize = 10, search = "", sortField = "", sortOrder = "") => {
  const params = { page, pageSize };
  if (search) params.search = search;
  if (sortField) params.sortField = sortField;
  if (sortOrder) params.sortOrder = sortOrder;
  const res = await api.get(API, { params });
  return res.data;
};

export const getCountryById = async (id) => {
  const res = await api.get(`${API}/${id}`);
  return res.data;
};

export const createCountry = async (dto) => {
  const res = await api.post(API, dto);
  return res.data;
};

export const updateCountry = async (dto) => {
  const res = await api.put(API, dto);
  return res.data;
};

export const deleteCountry = async (id) => {
  const res = await api.delete(`${API}/${id}`);
  return res.data;
};
