import api from "./api";
const API = "/User";

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

export const createUser = async (userData) => {
  const response = await api.post(API, userData);
  return response.data;
};

export const updateUser = async (userData) => {
  const response = await api.patch(API, userData);
  return response.data;
};

export const getUserById = async (id) => {
  const response = await api.get(`${API}/${id}`);
  return response.data;
};

export const toggleUserStatus = async (id) => {
  const response = await api.patch(`${API}/${id}/toggle-status`);
  return response.data;
};

export const getCountries = async (page = 1, pageSize = 10) => {
  const response = await api.get(`${API}/countries`, {
    params: { page, pageSize },
  });
  return response.data;
};

export const getCitiesByCountry = async (
  countryId,
  page = 1,
  pageSize = 10,
) => {
  const response = await api.get(`${API}/cities/${countryId}`, {
    params: { page, pageSize },
  });
  return response.data;
};

export const getRoles = async (page = 1, pageSize = 10) => {
  const response = await api.get(`${API}/roles`, {
    params: { page, pageSize },
  });
  return response.data;
};

// ✅ Fixed — was using undefined `api`
export const getStores = async (page = 1, pageSize = 100) => {
  const response = await api.get(`${API}/stores`, {
    params: { page, pageSize },
  });
  return response.data;
};

// ✅ Fixed — was using undefined `api`
export const getAddressesByCity = async (cityId, page = 1, pageSize = 100) => {
  const response = await api.get(`${API}/addresses/${cityId}`, {
    params: { page, pageSize },
  });
  return response.data;
};
