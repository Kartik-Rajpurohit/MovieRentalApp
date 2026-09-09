// Handles API requests related to physical store locations
import api from "./api";

// Base route for Store endpoints
const STORE = "/Store";

// GET request to fetch a paginated, sorted, and filtered list of stores (by city or country)
export const getStores = (
  page = 1,
  pageSize = 10,
  sortField = "",
  sortOrder = "",
  search = "",
  city = null,
  country = null
) =>
  api
    .get(STORE, {
      params: { page, pageSize, sortField, sortOrder, search, city, country },
    })
    .then((r) => r.data);

// GET request to fetch detailed store information by ID
export const getStoreById = (id) =>
  api.get(`${STORE}/${id}`).then((r) => r.data);

// POST request to create a new store location
export const createStore = (dto) =>
  api.post(STORE, dto).then((r) => r.data);
