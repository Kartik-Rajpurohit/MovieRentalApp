import api from "./api";

const PAYMENT = "/Payment";

// Get paginated, filtered, sorted list of payments
export const getPayments = (
  page = 1,
  pageSize = 10,
  sortField = "",
  sortOrder = "",
  search = "",
  customerId = null,
  staffId = null,
  rentalId = null
) =>
  api
    .get(PAYMENT, {
      params: { page, pageSize, sortField, sortOrder, search, customerId, staffId, rentalId },
    })
    .then((r) => r.data);

// Get single payment detail by ID
export const getPaymentById = (id) =>
  api.get(`${PAYMENT}/${id}`).then((r) => r.data);

// Create a new payment
export const createPayment = (dto) =>
  api.post(PAYMENT, dto).then((r) => r.data);
