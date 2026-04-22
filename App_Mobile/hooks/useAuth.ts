// hooks/useAuth.ts
import { useState, useEffect } from 'react';
import storageService from '../utils/storage';

export const useAuth = () => {
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const checkAuth = async () => {
      const token = await storageService.getAccessToken();
      setIsLoading(false);
    };
    checkAuth();
  }, []);

  return { user, isLoading, isAuthenticated: !!user };
};

export default useAuth;