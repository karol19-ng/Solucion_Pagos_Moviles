import AsyncStorage from '@react-native-async-storage/async-storage';

const TOKEN_KEY = '@NexusPay:token';
const USER_KEY = '@NexusPay:user';

export const storeToken = async (token: string) => await AsyncStorage.setItem(TOKEN_KEY, token);
export const getToken = async () => await AsyncStorage.getItem(TOKEN_KEY);
export const removeToken = async () => await AsyncStorage.removeItem(TOKEN_KEY);
export const storeUser = async (user: any) => await AsyncStorage.setItem(USER_KEY, JSON.stringify(user));
export const getUser = async () => {
  const data = await AsyncStorage.getItem(USER_KEY);
  return data ? JSON.parse(data) : null;
};
export const removeUser = async () => await AsyncStorage.removeItem(USER_KEY);
export const clearAll = async () => await AsyncStorage.multiRemove([TOKEN_KEY, USER_KEY]);