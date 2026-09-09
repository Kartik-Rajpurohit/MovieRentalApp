// Handles authentication-related API requests: login, registration, token refresh, and logout
import api from "./api";

// Base route for authentication endpoints
const AUTH = "/Auth";

// Sends user credentials (email, password) to backend via POST and returns auth data with JWT token
export const loginUser = async (email, password) => {
  const res = await api.post(`${AUTH}/login`, { email, password });
  return res.data;
};

// Sends new user registration details via POST and returns registered user data and token
export const signUpUser = async (payload) => {
  // payload contains: firstName, lastName, email, password, cityId,
  // district, postalCode, phone, and either existingAddressId OR street
  const res = await api.post(`${AUTH}/signup`, payload);
  return res.data;
};

// Requests a refreshed access token via POST using the HttpOnly cookie
export const refreshToken = async () => {
  // HttpOnly cookie is sent automatically — no token in request body
  const res = await api.post(`${AUTH}/refresh`);
  return res.data;
};

// Logs the current user out via POST and clears server-side authentication cookies
export const logoutUser = async () => {
  // HttpOnly cookie sent automatically; backend revokes and clears it
  await api.post("/Auth/logout");
};
