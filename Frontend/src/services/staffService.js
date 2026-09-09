// Handles API requests related to store staff members
import api from "./api";
const API = "/Staff";

// GET request to fetch a paginated list of staff members with optional search, status, and store filters
export const getStaff = async (
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

// GET request to fetch details of a single staff member by ID
export const getStaffById = async (id) => {
  const res = await api.get(`${API}/${id}`);
  return res.data;
};
