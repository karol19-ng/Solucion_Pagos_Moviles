// app/index.tsx
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { useRouter } from 'expo-router';

export default function Index() {
  const router = useRouter();

  return (
    <View style={styles.container}>
      {/* Logo y título */}
      <View style={styles.logoContainer}>
        <Text style={styles.logoIcon}>🏦</Text>
        <Text style={styles.title}>Banco Pegasos</Text>
        <Text style={styles.subtitle}>Pagos Móviles</Text>
      </View>

      {/* Tarjeta de operaciones */}
      <View style={styles.card}>
        <Text style={styles.cardTitle}>Operaciones</Text>
        
        {/* AM6 - Consultar Saldo */}
        <TouchableOpacity 
          style={styles.optionButton}
          onPress={() => router.push('/portal/balance')}
        >
          <View style={styles.optionTextContainer}>
            <Text style={styles.optionTitle}>Consultar Saldo</Text>
            <Text style={styles.optionDescription}>Ver saldo de cuenta asociada</Text>
          </View>
          <Text style={styles.optionArrow}>›</Text>
        </TouchableOpacity>

        {/* AM5 - Desinscribirse */}
        <TouchableOpacity 
          style={[styles.optionButton, styles.dangerButton]}
          onPress={() => router.push('/portal/unsubscribe')}
        >
          
          <View style={styles.optionTextContainer}>
            <Text style={styles.optionTitle}>Desinscribirse</Text>
            <Text style={styles.optionDescription}>Cancelar servicio de pagos móviles</Text>
          </View>
          <Text style={styles.optionArrow}>›</Text>
        </TouchableOpacity>
      </View>

      {/* Footer */}
      <View style={styles.footer}>
        <Text style={styles.footerText}>© 2024 Banco Pegasos</Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#370000',
    padding: 20,
  },
  logoContainer: {
    alignItems: 'center',
    marginTop: 60,
    marginBottom: 40,
  },
  logoIcon: {
    fontSize: 60,
    marginBottom: 10,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#f2cd54',
    textAlign: 'center',
  },
  subtitle: {
    fontSize: 16,
    color: '#f2cd54',
    textAlign: 'center',
    opacity: 0.8,
    marginTop: 5,
  },
  card: {
    backgroundColor: '#4a0000',
    borderRadius: 20,
    padding: 20,
    borderWidth: 1,
    borderColor: '#f2cd54',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 5,
  },
  cardTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#f2cd54',
    marginBottom: 20,
    textAlign: 'center',
    letterSpacing: 1,
  },
  optionButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#2a0000',
    borderRadius: 12,
    padding: 15,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: '#f2cd54',
  },
  dangerButton: {
    borderColor: '#f2cd54',
  },
  optionIconContainer: {
    width: 50,
    height: 50,
    borderRadius: 25,
    backgroundColor: '#370000',
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: 15,
    borderWidth: 1,
    borderColor: '#f2cd54',
  },
  optionIcon: {
    fontSize: 24,
  },
  optionTextContainer: {
    flex: 1,
  },
  optionTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#f2cd54',
    marginBottom: 4,
  },
  optionDescription: {
    fontSize: 12,
    color: '#c9a03d',
  },
  optionArrow: {
    fontSize: 24,
    color: '#f2cd54',
  },
  footer: {
    marginTop: 40,
    alignItems: 'center',
  },
  footerText: {
    fontSize: 12,
    color: '#c9a03d',
  },
});