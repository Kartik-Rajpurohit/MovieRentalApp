// Handles API requests related to rental customers
import api from "./api";
const API = "/Customer";

// GET request to fetch a paginated list of customers with optional search, active status, and store filters
export const getCustomers = async (
  page = 1,
  pageSize = 10,
  search = "",
  isActive = null,
  storeId = null,
) => {
  const params = { page, pageSize };
  if (search) params.search = search;
  if (isActive !== null && isActive !== undefined) params.isActive = isActive;
  if (storeId !== null) params.storeId = storeId;
  const res = await api.get(API, { params });
  return res.data;
};

// GET request to fetch detailed customer information by their ID
export const getCustomerById = async (id) => {
  const res = await api.get(`${API}/${id}`);
  return res.data;
};
