import api from "./api";
const API = "/Customer";

export const getCustomers = async (
  page = 1,
  pageSize = 10,
  search = "",
  isActive = null,
) => {
  const params = { page, pageSize };
  if (search) params.search = search;
  if (isActive !== null && isActive !== undefined) params.isActive = isActive;
  const res = await api.get(API, { params });
  return res.data;
};

export const getCustomerById = async (id) => {
  const res = await api.get(`${API}/${id}`);
  return res.data;
};
