// app/portal/_layout.tsx
import { Stack } from 'expo-router';

export default function PortalLayout() {
  return (
    <Stack>
      <Stack.Screen 
        name="home" 
        options={{ 
          title: 'Inicio',
          headerShown: false 
        }} 
      />
      <Stack.Screen 
        name="balance" 
        options={{ 
          title: 'Consulta de Saldo',
          headerBackTitle: 'Volver'
        }} 
      />
      <Stack.Screen 
        name="unsubscribe" 
        options={{ 
          title: 'Desinscripción',
          headerBackTitle: 'Volver'
        }} 
      />
      <Stack.Screen 
        name="transfer" 
        options={{ 
          title: 'Realizar Transferencia',
          headerBackTitle: 'Volver'
        }} 
      />
      <Stack.Screen 
        name="history" 
        options={{ 
          title: 'Mis Movimientos',
          headerBackTitle: 'Volver'
        }} 
      />
      <Stack.Screen 
        name="profile" 
        options={{ 
          title: 'Mi Perfil',
          headerBackTitle: 'Volver'
        }} 
      />
      <Stack.Screen 
        name="pay-services" 
        options={{ 
          title: 'Pagar Servicios',
          headerBackTitle: 'Volver'
        }} 
      />
      <Stack.Screen 
        name="details/[id]" 
        options={{ 
          title: 'Detalle de Transacción',
          headerBackTitle: 'Volver'
        }} 
      />
    </Stack>
  );
}