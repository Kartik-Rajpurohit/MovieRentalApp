// Handles rental-related API calls (fetching rentals, creating new rentals, marking returns).
import api from "./api";

const API = "/Rental";

// Fetch paginated rentals with optional search and filters
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

export const getReturnedUnpaidRentals = async (page = 1, pageSize = 500) => {
  const res = await api.get(API, {
    params: { page, pageSize, isReturned: true, hasPayment: false },
  });
  return res.data;
};
