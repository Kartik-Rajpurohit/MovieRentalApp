// Handles API requests related to actors
import api from "./api";

// Base route for Actor endpoints
const API = "/Actor";

// GET request to fetch a paginated list of actors with optional search and sorting
export const getActors = async (page = 1, pageSize = 10, search = "", sortField = "", sortOrder = "") => {
    const params = { page, pageSize };
    if (search) params.search = search;
    if (sortField) params.sortField = sortField;
    if (sortOrder) params.sortOrder = sortOrder;
    const res = await api.get(API, { params });
    return res.data;
};

// GET request to fetch a single actor by their ID
export const getActorById = async (id) => {
    const res = await api.get(`${API}/${id}`);
    return res.data;
};

// GET request to fetch detailed information for an actor (including film count)
export const getActorDetail = async (id) => {
    const res = await api.get(`${API}/${id}/detail`);
    return res.data;
};

// GET request to fetch films starring a specific actor with pagination and search
export const getFilmsByActor = async (id, page = 1, pageSize = 10, search = "") => {
    const params = { page, pageSize };
    if (search) params.search = search;
    const res = await api.get(`${API}/${id}/films`, { params });
    return res.data;
};

// POST request to create a new actor record
export const createActor = async (data) => {
    const res = await api.post(API, data);
    return res.data;
};

// PATCH request to update an existing actor's details
export const updateActor = async (data) => {
    const res = await api.patch(API, data);
    return res.data;
};

// DELETE request to remove an actor by their ID
export const deleteActor = async (id) => {
    const res = await api.delete(`${API}/${id}`);
    return res.data;
};