import { useAuthContext } from '../context/AuthContext';

// Por ahora todos los usuarios del portal son rol "portal"
// Cuando el backend agregue roles al JWT, actualizar aquí
export type UserRole = 'portal' | 'admin' | 'core' | 'unknown';

export function useRole(): UserRole {
  const { user } = useAuthContext();
  if (!user) return 'unknown';
  // Extender aquí cuando el JWT incluya claim de rol
  return 'portal';
}
