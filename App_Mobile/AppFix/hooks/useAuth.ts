import { useState } from 'react';
import { useAuthContext } from '../context/AuthContext';
import { router } from 'expo-router';

export function useAuth() {
  const { login: contextLogin } = useAuthContext();
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [attempts, setAttempts] = useState(0);
  const [isBlocked, setIsBlocked] = useState(false);

  const login = async (usuario: string, password: string, rememberMe = false) => {
    if (isBlocked) return;
    setIsLoading(true);
    setError(null);
    try {
      await contextLogin(usuario, password, rememberMe);
      router.replace('/portal/home');
    } catch (e: any) {
      const newAttempts = attempts + 1;
      setAttempts(newAttempts);
      if (newAttempts >= 3) {
        setIsBlocked(true);
        setError('Demasiados intentos fallidos. Intente más tarde.');
      } else {
        setError('Usuario o contraseña incorrectos.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return { login, error, isLoading, attempts, isBlocked };
}