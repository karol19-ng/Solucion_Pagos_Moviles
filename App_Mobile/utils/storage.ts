// utils/storage.ts
import AsyncStorage from '@react-native-async-storage/async-storage';

class StorageService {
  async getAccessToken(): Promise<string | null> {
    return await AsyncStorage.getItem('access_token');
  }
  
  async saveToken(token: string): Promise<void> {
    await AsyncStorage.setItem('access_token', token);
  }
  
  async clearToken(): Promise<void> {
    await AsyncStorage.removeItem('access_token');
  }
}

export default new StorageService();