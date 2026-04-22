import { Tabs } from 'expo-router';
import  theme  from '../../styles/theme';
import { ProtectedRoute } from '../../components/ProtectedRoute';

export default function AdminLayout() {
  return (
    <ProtectedRoute allowedRoles={['admin']}>
      <Tabs screenOptions={{ tabBarActiveTintColor: theme.colors.primary, tabBarInactiveTintColor: theme.colors.secondary, tabBarStyle: { backgroundColor: theme.colors.background }, headerStyle: { backgroundColor: theme.colors.background }, headerTitleStyle: { color: theme.colors.textPrimary } }}>
        <Tabs.Screen name="users" options={{ title: 'Usuarios' }} />
        <Tabs.Screen name="commissions" options={{ title: 'Comisiones' }} />
        <Tabs.Screen name="audit-logs" options={{ title: 'Auditoría' }} />
        <Tabs.Screen name="metrics" options={{ title: 'Métricas' }} />
        <Tabs.Screen name="notifications" options={{ title: 'Notificaciones' }} />
        <Tabs.Screen name="user-form" options={{ href: null }} />
      </Tabs>
    </ProtectedRoute>
  );
}