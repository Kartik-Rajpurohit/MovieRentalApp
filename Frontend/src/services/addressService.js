import api from "./api";

const API = "/Address";

export const getAddresses = async (params = {}) => {
  const res = await api.get(API, { params });
  return res.data;
};

export const getAddressById = async (id) => {
  const res = await api.get(`${API}/${id}`);
  return res.data;
};

export const createAddress = async (dto) => {
  const res = await api.post(API, dto);
  return res.data;
};

export const updateAddress = async (dto) => {
  const res = await api.patch(API, dto);
  return res.data;
};

export const deleteAddress = async (id) => {
  const res = await api.delete(`${API}/${id}`);
  return res.data;
};
