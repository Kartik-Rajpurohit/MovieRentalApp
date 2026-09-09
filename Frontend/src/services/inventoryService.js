// Handles API requests related to physical movie copies in store inventory
import api from "./api";

// Base route for Inventory endpoints
const INVENTORY = "/Inventory";

// GET request to fetch a paginated list of inventory copies with optional filters (storeId, filmId, availability)
export const getInventory = (params = {}) =>
  api.get(INVENTORY, { params }).then(r => r.data);

// GET request to fetch detailed information for an inventory copy by ID
export const getInventoryById = (id) =>
  api.get(`${INVENTORY}/${id}`).then(r => r.data);

// POST request to add a new physical film copy to a store's inventory
export const createInventory = (dto) =>
  api.post(INVENTORY, dto).then(r => r.data);

// PATCH request to update the store assignment of an inventory copy
export const updateInventory = (dto) =>
  api.patch(INVENTORY, dto).then(r => r.data);

// DELETE request to remove an inventory copy by ID
export const deleteInventory = (id) =>
  api.delete(`${INVENTORY}/${id}`).then(r => r.data);
