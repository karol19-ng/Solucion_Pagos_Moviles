import { API_URL } from '../../constants/api';
export const authService = {
  login: async (usuario: string, password: string) => {
    const response = await fetch(`${API_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ usuario, password }),
    });

    if (!response.ok) {
      throw new Error('Credenciales incorrectas');
    }

    return await response.json();
  },
};

export const decodeToken = (token: string): any => {
  try {
    const base64 = token.split('.')[1];
    const decoded = atob(base64.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(decoded);
  } catch {
    return {};
  }
};