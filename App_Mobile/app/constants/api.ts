// app/constants/api.ts
export const API_CONFIG = {
  // CAMBIA 192.168.1.100 por tu IP real
  GATEWAY_URL: 'http://192.168.50.156',  // ← Usa tu IP local
  BASE_URL: 'http://192.168.50.156:5188',
  TIMEOUT: 30000,
};

export const ENDPOINTS = {
  LOGIN: '/auth/login',
  CANCEL_SUBSCRIPTION: '/auth/cancel-subscription',
  ACCOUNT_BALANCE: '/accounts/balance',
};

export default API_CONFIG;


