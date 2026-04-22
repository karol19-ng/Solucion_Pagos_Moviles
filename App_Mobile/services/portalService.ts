// services/portalService.ts
import AsyncStorage from '@react-native-async-storage/async-storage';
import apiClient from './apiClient';

export default {
  async desinscribir(data: { numeroCuenta: string; identificacion: string; numeroTelefono: string }) {
    try {
      const token = await AsyncStorage.getItem('access_token');
      const response = await apiClient.post('/auth/cancel-subscription', data, {
        headers: { Authorization: `Bearer ${token}` },
      });
      return response.data;
    } catch (error: any) {
      return {
        codigo: -1,
        descripcion: error.response?.data?.descripcion || 'Error en la desinscripción',
      };
    }
  },
  
  async consultarSaldo(data: { telefono: string; identificacion: string }) {
    try {
      const token = await AsyncStorage.getItem('access_token');
      const response = await apiClient.post('/accounts/balance', data, {
        headers: { Authorization: `Bearer ${token}` },
      });
      return response.data;
    } catch (error: any) {
      return {
        codigo: -1,
        descripcion: error.response?.data?.descripcion || 'Error al consultar saldo',
      };
    }
  },
};