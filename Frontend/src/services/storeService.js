import api from "./api";

const STORE = "/Store";

// Get paginated, filtered, sorted list of stores
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

// Get single store detail by ID
export const getStoreById = (id) =>
  api.get(`${STORE}/${id}`).then((r) => r.data);
export const createStore = (dto) =>
  api.post(STORE, dto).then((r) => r.data);
