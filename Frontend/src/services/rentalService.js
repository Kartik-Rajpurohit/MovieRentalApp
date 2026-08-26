import api from "./api";

const API = "/Rental";

export const getRentals = async (params = {}) => {
  const res = await api.get(API, { params });
  return res.data;
};

export const getRentalById = async (id) => {
  const res = await api.get(`${API}/${id}`);
  return res.data;
};

export const createRental = async (dto) => {
  const res = await api.post(API, dto);
  return res.data;
};

export const returnRental = async (id) => {
  const res = await api.patch(`${API}/${id}/return`);
  return res.data;
};
