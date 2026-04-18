import axios from 'axios';

const api = axios.create({
    baseURL: 'https://localhost:7159/api' 
});

// Ez a rész automatikusan hozzáadja a tokent a fejléchez, ha be vagy jelentkezve
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export default api;