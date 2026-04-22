import { useState } from 'react';
import { useAuthContext } from '../context/AuthContext';
import { router } from 'expo-router';

const MAX_ATTEMPTS = 3;

export function useAuth() {
  const ctx = useAuthContext();
  const [error, setError] = useState<string | null>(null);
  const [isBlocked, setIsBlocked] = useState(false);
  const [attempts, setAttempts] = useState(0);
  const [isLoading, setIsLoading] = useState(false);

  const login = async (usuario: string, password: string, rememberMe = false) => {
    if (isBlocked) {
      setError('Usuario bloqueado por demasiados intentos fallidos.');
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      await ctx.login(usuario, password, rememberMe);
      setAttempts(0);
      router.replace('/(portal)/home');
    } catch (err: any) {
      const newAttempts = attempts + 1;
      setAttempts(newAttempts);

      if (newAttempts >= MAX_ATTEMPTS) {
        setIsBlocked(true);
        setError('Usuario bloqueado por 3 intentos fallidos.');
      } else {
        setError(err?.message ?? 'Usuario y/o contraseña incorrectos.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    await ctx.logout();
    router.replace('/(auth)/login');
  };

  return {
    user: ctx.user,
    isAuthenticated: ctx.isAuthenticated,
    isLoading,
    error,
    isBlocked,
    attempts,
    login,
    logout,
  };
}
