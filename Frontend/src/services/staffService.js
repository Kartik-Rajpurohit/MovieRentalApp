import api from "./api";
const API = "/Staff";

export const getStaff = async (
  page = 1,
  pageSize = 10,
  search = "",
  isActive = null,
) => {
  const params = { page, pageSize };
  if (search) params.search = search;
  if (isActive !== null && isActive !== undefined) params.isActive = isActive;
  if (storeId !== null) params.storeId = storeId;
  const res = await api.get(API, { params });
  return res.data;
};

export const getStaffById = async (id) => {
  const res = await api.get(`${API}/${id}`);
  return res.data;
};
