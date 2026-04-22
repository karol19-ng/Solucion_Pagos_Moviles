// services/authService.ts
import apiClient from './apiClient';
import { ENDPOINTS } from '../app/constants/api';

interface LoginResponse {
  codigo: number;
  descripcion: string;
  access_token?: string;
  refresh_token?: string;
  expires_in?: number;
}

class AuthService {
  async login(credentials: { email: string; password: string }): Promise<LoginResponse> {
    try {
      const response = await apiClient.post(ENDPOINTS.LOGIN, credentials);
      return response.data;
    } catch (error: any) {
      return {
        codigo: -1,
        descripcion: error.response?.data?.descripcion || 'Error al iniciar sesión',
      };
    }
  }
}

export default new AuthService();