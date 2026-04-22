// services/apiClient.ts
import axios from 'axios';
import { API_CONFIG } from '../app/constants/api';
import storageService from '../utils/storage';

const apiClient = axios.create({
  baseURL: API_CONFIG.GATEWAY_URL,
  timeout: API_CONFIG.TIMEOUT,
  headers: { 'Content-Type': 'application/json' },
});

apiClient.interceptors.request.use(async (config) => {
  const token = await storageService.getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default apiClient;