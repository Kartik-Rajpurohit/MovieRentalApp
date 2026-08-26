import api from "./api";
const API = "/Role";

export const getRoles = async (page = 1, pageSize = 10, search = "") => {
  const params = { page, pageSize };
  if (search) params.search = search;
  const response = await api.get(API, { params });
  return response.data;
};
