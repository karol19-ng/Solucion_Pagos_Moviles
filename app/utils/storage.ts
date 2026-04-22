import AsyncStorage from '@react-native-async-storage/async-storage';

const TOKEN_KEY = '@NexusPay:token';
const USER_KEY = '@NexusPay:user';
const REMEMBER_KEY = '@NexusPay:remember';
const USERNAME_KEY = '@NexusPay:username';

export const storage = {
  getToken: async () => await AsyncStorage.getItem(TOKEN_KEY),
  getUsuarioName: async () => await AsyncStorage.getItem(USERNAME_KEY),
  setRememberMe: async (val: boolean) => await AsyncStorage.setItem(REMEMBER_KEY, JSON.stringify(val)),
  saveSession: async (data: { access_token: string; refresh_token: string; expires_in?: number; usuarioId: string; usuarioName: string }) => {
    await AsyncStorage.setItem(TOKEN_KEY, data.access_token);
    await AsyncStorage.setItem(USERNAME_KEY, data.usuarioName);
  },
  clearSession: async () => {
    await AsyncStorage.multiRemove([TOKEN_KEY, USER_KEY, REMEMBER_KEY, USERNAME_KEY]);
  },
};