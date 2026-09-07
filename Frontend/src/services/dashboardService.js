import api from "./api";

// GET /api/Dashboard — backend role se decide karta hai kya return karna hai
export const getDashboard = () => api.get("/Dashboard").then((r) => r.data);
