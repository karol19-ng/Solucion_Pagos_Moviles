import React, { createContext, useContext, useEffect, useState } from 'react';
import { storage } from '../app/utils/storage';
import { authService, decodeToken } from '../app/services/authService';

interface AuthUser {
  usuarioId: string;
  usuarioName: string;
  email: string;
  token: string;
}

interface AuthContextType {
  user: AuthUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  failedAttempts: number;
  login: (usuario: string, password: string, rememberMe?: boolean) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [failedAttempts, setFailedAttempts] = useState(0);

  useEffect(() => {
    // Restaurar sesión al abrir la app
    (async () => {
      try {
        const token = await storage.getToken();
        if (token) {
          const claims = decodeToken(token);
          const name = await storage.getUsuarioName();
          setUser({
            usuarioId: claims.nameid ?? '',
            usuarioName: claims.unique_name ?? name ?? '',
            email: claims.email ?? '',
            token,
          });
        }
      } finally {
        setIsLoading(false);
      }
    })();
  }, []);

  const login = async (usuario: string, password: string, rememberMe = false) => {
    const data = await authService.login(usuario, password);
    const claims = decodeToken(data.access_token);

    const authUser: AuthUser = {
      usuarioId: claims.nameid ?? '',
      usuarioName: claims.unique_name ?? usuario,
      email: claims.email ?? '',
      token: data.access_token,
    };

    await storage.saveSession({
      access_token: data.access_token,
      refresh_token: data.refresh_token,
      expires_in: data.expires_in,
      usuarioId: authUser.usuarioId,
      usuarioName: authUser.usuarioName,
    });

    if (rememberMe) await storage.setRememberMe(true);

    setFailedAttempts(0);
    setUser(authUser);
  };

  const logout = async () => {
    await storage.clearSession();
    setUser(null);
    setFailedAttempts(0);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        isAuthenticated: !!user,
        failedAttempts,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuthContext() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuthContext must be used within AuthProvider');
  return ctx;
}
