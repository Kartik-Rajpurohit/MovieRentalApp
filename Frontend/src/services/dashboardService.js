// Handles API requests related to dashboard summaries and statistics
import api from "./api";

// GET request to fetch role-based dashboard statistics (Admin, Staff, or Customer metrics)
export const getDashboard = () => api.get("/Dashboard").then((r) => r.data);
