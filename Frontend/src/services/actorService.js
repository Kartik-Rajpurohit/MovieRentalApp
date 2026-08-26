import api from "./api";

const API = "/Actor";

export const getActors = async (page = 1, pageSize = 10, search = "", sortField = "", sortOrder = "") => {
    const params = { page, pageSize };
    if (search) params.search = search;
    if (sortField) params.sortField = sortField;
    if (sortOrder) params.sortOrder = sortOrder;
    const res = await api.get(API, { params });
    return res.data;
};

export const getActorById = async (id) => {
    const res = await api.get(`${API}/${id}`);
    return res.data;
};

export const getActorDetail = async (id) => {
    const res = await api.get(`${API}/${id}/detail`);
    return res.data;
};

export const getFilmsByActor = async (id, page = 1, pageSize = 10, search = "") => {
    const params = { page, pageSize };
    if (search) params.search = search;
    const res = await api.get(`${API}/${id}/films`, { params });
    return res.data;
};

export const createActor = async (data) => {
    const res = await api.post(API, data);
    return res.data;
};

export const updateActor = async (data) => {
    const res = await api.patch(API, data);
    return res.data;
};

export const deleteActor = async (id) => {
    const res = await api.delete(`${API}/${id}`);
    return res.data;
};