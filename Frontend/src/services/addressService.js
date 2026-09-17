// Handles API requests related to addresses
import api from "./api";

// Base route for Address endpoints
const API = "/Address";

// GET request to fetch a list of addresses with optional filter and pagination parameters
export const getAddresses = async (params = {}) => {
  const res = await api.get(API, { params });
  return res.data;
};

// GET request to fetch a single address by its ID
export const getAddressById = async (id) => {
  const res = await api.get(`${API}/${id}`);
  return res.data;
};

// POST request to create a new address record
export const createAddress = async (dto) => {
  const res = await api.post(API, dto);
  return res.data;
};
