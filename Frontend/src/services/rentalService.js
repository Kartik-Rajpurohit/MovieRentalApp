// Handles API requests related to movie rentals, checkouts, and returns
import api from "./api";

// Base route for Rental endpoints
const API = "/Rental";

// GET request to fetch a paginated list of rentals with optional filters (customer, staff, returned status)
export const getRentals = async (params = {}) => {
  const res = await api.get(API, { params });
  return res.data;
};

// GET request to fetch details of a single rental transaction by its ID
export const getRentalById = async (id) => {
  const res = await api.get(`${API}/${id}`);
  return res.data;
};

// POST request to create a new movie rental checkout
export const createRental = async (dto) => {
  const res = await api.post(API, dto);
  return res.data;
};

// PATCH request to mark a rented film copy as returned
export const returnRental = async (id) => {
  const res = await api.patch(`${API}/${id}/return`);
  return res.data;
};

// GET request to fetch returned rentals that do not yet have a recorded payment
export const getReturnedUnpaidRentals = async (page = 1, pageSize = 500) => {
  const res = await api.get(API, {
    params: { page, pageSize, isReturned: true, hasPayment: false },
  });
  return res.data;
};
