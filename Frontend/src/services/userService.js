// Handles API requests related to user accounts, roles, and address dropdown lookups
import api from "./api";
const API = "/User";

// GET request to fetch a paginated list of users with sorting, search, and status/role filters
export const getUsers = async (
  page,
  pageSize,
  sortField,
  sortOrder,
  name,
  email,
  role,
  search,
  isActive,
) => {
  const params = { page, pageSize };
  if (sortField && sortOrder) {
    params.sortField = sortField;
    params.sortOrder = sortOrder;
  }
  if (name) params.name = name;
  if (email) params.email = email;
  if (role) params.roleId = role;
  if (search) params.search = search;
  if (isActive !== null && isActive !== undefined) params.isActive = isActive;

  const response = await api.get(API, { params });
  return response.data;
};

// PATCH request to update existing user information
export const updateUser = async (userData) => {
  const response = await api.patch(API, userData);
  return response.data;
};

// GET request to fetch a single user's details by ID
export const getUserById = async (id) => {
  const response = await api.get(`${API}/${id}`);
  return response.data;
};

// PATCH request to toggle a user's active/inactive status
export const toggleUserStatus = async (id) => {
  const response = await api.patch(`${API}/${id}/toggle-status`);
  return response.data;
};
