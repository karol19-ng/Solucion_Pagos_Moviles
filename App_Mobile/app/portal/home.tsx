// app/portal/home.tsx - Con colores corporativos
import React from 'react';
import {
  View,
  Text,
  TouchableOpacity,
  StyleSheet,
  ScrollView,
} from 'react-native';
import { useRouter } from 'expo-router';
import { useAuth } from '../../hooks/useAuth';

export default function HomeScreen() {
  const router = useRouter();
  const { user } = useAuth();

  const menuOptions = [
    {
      id: 'balance',
      title: 'Consultar Saldo',
      description: 'Ver saldo disponible de tu cuenta',
      route: '/portal/balance',
    },
    {
      id: 'unsubscribe',
      title: 'Desinscribirse',
      description: 'Cancelar servicio de pagos móviles',
      route: '/portal/unsubscribe',
    },
  ];

  const getCurrentDate = () => {
    const date = new Date();
    return date.toLocaleDateString('es-CR', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    });
  };

  const getCurrentTime = () => {
    const date = new Date();
    return date.toLocaleTimeString('es-CR', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <ScrollView style={styles.container}>
      {/* Header de Bienvenida */}
      <View style={styles.welcomeCard}>
        <View style={styles.logoContainer}>
          <Text style={styles.logoIcon}>🏦</Text>
        </View>
        <Text style={styles.welcomeTitle}>¡BIENVENIDO!</Text>
        <Text style={styles.welcomeName}>{user?.nombreCompleto || 'Cliente'}</Text>
        <View style={styles.datetimeContainer}>
          <Text style={styles.dateText}>📅 {getCurrentDate()}</Text>
          <Text style={styles.timeText}>⏳ {getCurrentTime()}</Text>
        </View>
      </View>

      {/* Menú de opciones */}
      <View style={styles.menuContainer}>
        <Text style={styles.menuTitle}>Servicios Disponibles</Text>
        
        {menuOptions.map((option) => (
          <TouchableOpacity
            key={option.id}
            style={styles.menuCard}
            onPress={() => router.push(option.route as any)}
          >
            <View style={styles.menuIcon}>
              <Text style={styles.iconText}>{option.icon}</Text>
            </View>
            <View style={styles.menuContent}>
              <Text style={styles.menuCardTitle}>{option.title}</Text>
              <Text style={styles.menuCardDesc}>{option.description}</Text>
            </View>
            <Text style={styles.arrowIcon}>›</Text>
          </TouchableOpacity>
        ))}
      </View>

      {/* Footer */}
      <View style={styles.footer}>
        <Text style={styles.footerText}>Banco Pegasos - Pagos Móviles</Text>
        <Text style={styles.footerSubtext}>Seguridad y confianza</Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#370000',
  },
  welcomeCard: {
    backgroundColor: '#4a0000',
    padding: 24,
    paddingTop: 48,
    paddingBottom: 32,
    borderBottomLeftRadius: 24,
    borderBottomRightRadius: 24,
    borderBottomWidth: 2,
    borderBottomColor: '#f2cd54',
    alignItems: 'center',
  },
  logoContainer: {
    width: 70,
    height: 70,
    borderRadius: 35,
    backgroundColor: '#370000',
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 16,
    borderWidth: 2,
    borderColor: '#f2cd54',
  },
  logoIcon: {
    fontSize: 36,
  },
  welcomeTitle: {
    fontSize: 14,
    color: '#f2cd54',
    letterSpacing: 2,
    marginBottom: 8,
  },
  welcomeName: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#f2cd54',
    marginBottom: 16,
    textAlign: 'center',
  },
  datetimeContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 16,
    backgroundColor: '#370000',
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 20,
    borderWidth: 1,
    borderColor: '#f2cd54',
  },
  dateText: {
    fontSize: 12,
    color: '#f2cd54',
  },
  timeText: {
    fontSize: 12,
    color: '#f2cd54',
  },
  menuContainer: {
    padding: 20,
  },
  menuTitle: {
    fontSize: 18,
    fontWeight: '600',
    color: '#f2cd54',
    marginBottom: 16,
    letterSpacing: 1,
  },
  menuCard: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#4a0000',
    borderRadius: 16,
    padding: 16,
    marginBottom: 12,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.3,
    shadowRadius: 4,
    elevation: 3,
    borderWidth: 1,
    borderColor: '#f2cd54',
  },
  menuIcon: {
    width: 52,
    height: 52,
    borderRadius: 26,
    backgroundColor: '#2a0000',
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: 16,
    borderWidth: 1,
    borderColor: '#f2cd54',
  },
  iconText: {
    fontSize: 24,
  },
  menuContent: {
    flex: 1,
  },
  menuCardTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#f2cd54',
    marginBottom: 4,
  },
  menuCardDesc: {
    fontSize: 12,
    color: '#c9a03d',
  },
  arrowIcon: {
    fontSize: 24,
    color: '#f2cd54',
  },
  footer: {
    padding: 20,
    alignItems: 'center',
    marginTop: 20,
    marginBottom: 30,
  },
  footerText: {
    fontSize: 12,
    color: '#c9a03d',
    textAlign: 'center',
  },
  footerSubtext: {
    fontSize: 10,
    color: '#a08040',
    marginTop: 4,
    textAlign: 'center',
  },
});