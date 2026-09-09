// Handles API requests related to rental payments and financial transactions
import api from "./api";

// Base route for Payment endpoints
const PAYMENT = "/Payment";

// GET request to fetch a paginated list of payments with sorting and filters (customer, staff, dates, amount)
export const getPayments = ({
  page = 1,
  pageSize = 10,
  sortField = "",
  sortOrder = "",
  search = "",
  customerId = null,
  staffId = null,
  rentalId = null,
  minAmount = null,
  maxAmount = null,
  fromDate = null,
  toDate = null,
} = {}) =>
  api
    .get(PAYMENT, {
      params: {
        page,
        pageSize,
        sortField,
        sortOrder,
        search,
        customerId,
        staffId,
        rentalId,
        minAmount,
        maxAmount,
        fromDate,
        toDate,
      },
    })
    .then((r) => r.data);

// GET request to fetch payment details by ID
export const getPaymentById = (id) =>
  api.get(`${PAYMENT}/${id}`).then((r) => r.data);

// POST request to record a new payment for a rental
export const createPayment = (dto) =>
  api.post(PAYMENT, dto).then((r) => r.data);
