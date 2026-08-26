import api from "./api";

const INVENTORY = "/Inventory";

// Get paginated inventory list with filters
export const getInventory = (params = {}) =>
  api.get(INVENTORY, { params }).then(r => r.data);

// Get inventory detail by id
export const getInventoryById = (id) =>
  api.get(`${INVENTORY}/${id}`).then(r => r.data);

// Add a new copy of a film to a store
export const createInventory = (dto) =>
  api.post(INVENTORY, dto).then(r => r.data);

// Update store assignment
export const updateInventory = (dto) =>
  api.patch(INVENTORY, dto).then(r => r.data);

// Delete inventory copy
export const deleteInventory = (id) =>
  api.delete(`${INVENTORY}/${id}`).then(r => r.data);
