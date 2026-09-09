// Handles API requests related to cities
import api from "./api";
const API = "/City";

// GET request to fetch a list of cities with optional query parameters (page, pageSize, countryId, search)
export const getCities = async (params = {}) => {
  const res = await api.get(API, { params });
  return res.data;
};

// GET request to fetch details of a single city by its ID
export const getCityById = async (id) => {
  const res = await api.get(`${API}/${id}`);
  return res.data;
};

// GET request to fetch all addresses within a specific city
export const getAddressesByCity = async (
  id,
  page = 1,
  pageSize = 10,
  search = "",
) => {
  const res = await api.get(`${API}/${id}/addresses`, {
    params: { page, pageSize, search },
  });
  return res.data;
};

// POST request to create a new city
export const createCity = async (dto) => {
  const res = await api.post(API, dto);
  return res.data;
};

// PATCH request to update an existing city
export const updateCity = async (dto) => {
  const res = await api.patch(API, dto);
  return res.data;
};

// DELETE request to remove a city by its ID
export const deleteCity = async (id) => {
  const res = await api.delete(`${API}/${id}`);
  return res.data;
};
