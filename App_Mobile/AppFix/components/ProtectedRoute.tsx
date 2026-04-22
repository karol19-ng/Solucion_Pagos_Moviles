import { ReactNode, useEffect } from 'react';
import { router } from 'expo-router';
import { useAuthContext } from '../context/AuthContext';
import { View, ActivityIndicator } from 'react-native';
import  theme  from '../styles/theme';


type ProtectedRouteProps = 
{
  children:ReactNode;
  allowedRoles?: string[];
}// fin de protectedRouteProps

export function ProtectedRoute({ children, allowedRoles }: ProtectedRouteProps) {
  //logica validacion
  const { isAuthenticated, isLoading } = useAuthContext();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace('/(auth)/login' as any );
    }
  }, [isAuthenticated, isLoading]);

  if (isLoading) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: theme.colors.background }}>
        <ActivityIndicator color={theme.colors.primary} size="large" />
      </View>
    );
  }

  if (!isAuthenticated) return null;

  return <>{children}</>;
}
