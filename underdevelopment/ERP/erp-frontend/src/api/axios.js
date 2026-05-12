import axios from 'axios';

// Ez a sor a kulcs: 
// 1. Megpróbálja beolvasni a Vercelen beállított VITE_API_URL-t.
// 2. Ha nem találja (vagyis a saját gépeden futtatod), akkor marad a localhost.
const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL || 'https://localhost:7159/api',
});

api.interceptors.request.use((config) => {
    const token = sessionStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export default api;