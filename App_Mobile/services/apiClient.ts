import axios from 'axios';
import { API_BASE_URL } from '../constants/api';
import { getToken, clearAll } from '../utils/storage';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' }
});

apiClient.interceptors.request.use(async (config) => {
  const token = await getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

apiClient.interceptors.response.use(
  (res) => res,
  async (error) => {
    if (error.response?.status === 401) await clearAll();
    return Promise.reject(error);
  }
);

export default apiClient;