// Handles API requests related to user roles
import api from "./api";
const API = "/Role";

// GET request to fetch a paginated list of user roles with optional search
export const getRoles = async (page = 1, pageSize = 10, search = "") => {
  const params = { page, pageSize };
  if (search) params.search = search;
  const response = await api.get(API, { params });
  return response.data;
};

// POST request to create a new user role
export const createRole = async (dto) => {
  const response = await api.post(API, dto);
  return response.data;
};
