import api from "./api";
const API = "/City";

export const getCities = async (params = {}) => {
  const res = await api.get(API, { params });
  return res.data;
};

export const getCityById = async (id) => {
  const res = await api.get(`${API}/${id}`);
  return res.data;
};

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

export const createCity = async (dto) => {
  const res = await api.post(API, dto);
  return res.data;
};

export const updateCity = async (dto) => {
  const res = await api.patch(API, dto);
  return res.data;
};

export const deleteCity = async (id) => {
  const res = await api.delete(`${API}/${id}`);
  return res.data;
};
