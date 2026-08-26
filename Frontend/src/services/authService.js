import api from "./api";

// Auth endpoints use the shared api instance — base URL from .env
const AUTH = "/Auth";

export const loginUser = async (email, password) => {
  const res = await api.post(`${AUTH}/login`, { email, password });
  return res.data;
};

export const signUpUser = async (firstName, lastName, email, password) => {
  const res = await api.post(`${AUTH}/signup`, {
    firstName,
    lastName,
    email,
    password,
  });
  return res.data;
};

export const refreshToken = async (refreshToken) => {
  const res = await api.post(`${AUTH}/refresh`, { refreshToken });
  return res.data;
};

export const logoutUser = async () => {
  const refreshToken = localStorage.getItem("refreshToken");
  if (refreshToken) {
    await axios.post(`${API}/logout`, { refreshToken });
  }
};
