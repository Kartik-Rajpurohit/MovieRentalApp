// Handles API requests related to countries
import api from "./api";

// Base route for Country endpoints
const API = "/Country";

// GET request to fetch a paginated list of countries with search and sorting options
export const getCountries = async (page = 1, pageSize = 10, search = "", sortField = "", sortOrder = "") => {
  const params = { page, pageSize };
  if (search) params.search = search;
  if (sortField) params.sortField = sortField;
  if (sortOrder) params.sortOrder = sortOrder;
  const res = await api.get(API, { params });
  return res.data;
};

// GET request to fetch details of a country by its ID
export const getCountryById = async (id) => {
  const res = await api.get(`${API}/${id}`);
  return res.data;
};

// POST request to create a new country record
export const createCountry = async (dto) => {
  const res = await api.post(API, dto);
  return res.data;
};

// PUT request to update an existing country record
export const updateCountry = async (dto) => {
  const res = await api.put(API, dto);
  return res.data;
};

// DELETE request to delete a country by its ID
export const deleteCountry = async (id) => {
  const res = await api.delete(`${API}/${id}`);
  return res.data;
};
