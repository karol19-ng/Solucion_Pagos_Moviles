// app/portal/balance.tsx - AM6 (Con colores Banco Pegasos)
import React, { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  Alert,
  ActivityIndicator,
  ScrollView,
  KeyboardAvoidingView,
  Platform,
} from 'react-native';
import { useRouter } from 'expo-router';
import portalService from '../../services/portalService';

export default function BalanceScreen() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [telefono, setTelefono] = useState('');
  const [identificacion, setIdentificacion] = useState('');
  const [saldo, setSaldo] = useState<number | null>(null);
  const [nombre, setNombre] = useState<string | null>(null);

  const handleConsultar = async () => {
    if (!telefono || !identificacion) {
      Alert.alert('Error', 'Complete todos los campos');
      return;
    }

    if (!/^\d{8}$/.test(telefono)) {
      Alert.alert('Error', 'El teléfono debe tener 8 dígitos');
      return;
    }

    setLoading(true);
    try {
      const response = await portalService.consultarSaldo({ telefono, identificacion });
      if (response.codigo === 0) {
        setSaldo(response.saldo || 0);
        setNombre(response.nombre || null);
      } else {
        Alert.alert('Error', response.descripcion);
        setSaldo(null);
        setNombre(null);
      }
    } catch (error) {
      Alert.alert('Error', 'Error de conexión. Verifique el servidor');
    } finally {
      setLoading(false);
    }
  };

  const handleReset = () => {
    setSaldo(null);
    setNombre(null);
    setTelefono('');
    setIdentificacion('');
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('es-CR', {
      style: 'currency',
      currency: 'CRC',
      minimumFractionDigits: 0,
    }).format(amount);
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView contentContainerStyle={styles.scrollContainer}>
        {/* Header */}
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Consultar Saldo</Text>
          <Text style={styles.headerSubtitle}>
            Complete los datos para consultar el saldo de una cuenta
          </Text>
        </View>

        {/* Formulario */}
        <View style={styles.formCard}>
          <View style={styles.inputGroup}>
            <Text style={styles.label}>Identificación</Text>
            <Text style={styles.labelHint}>Ej: 108880111</Text>
            <TextInput
              style={styles.input}
              placeholder="Ingrese su identificación"
              placeholderTextColor="#999"
              value={identificacion}
              onChangeText={setIdentificacion}
              keyboardType="numeric"
              editable={!loading}
              placeholderTextColor="#a08040"
            />
          </View>

          <View style={styles.inputGroup}>
            <Text style={styles.label}>Número de Teléfono</Text>
            <Text style={styles.labelHint}>8 dígitos, solo números</Text>
            <TextInput
              style={styles.input}
              placeholder="Ej: 88881111"
              placeholderTextColor="#a08040"
              value={telefono}
              onChangeText={setTelefono}
              keyboardType="phone-pad"
              maxLength={8}
              editable={!loading}
            />
          </View>

          <View style={styles.buttonContainer}>
            <TouchableOpacity
              style={[styles.button, styles.cancelButton]}
              onPress={() => router.back()}
              disabled={loading}
            >
              <Text style={styles.cancelButtonText}>Cancelar</Text>
            </TouchableOpacity>

            <TouchableOpacity
              style={[styles.button, styles.consultButton, loading && styles.disabledButton]}
              onPress={handleConsultar}
              disabled={loading}
            >
              {loading ? (
                <ActivityIndicator color="#f2cd54" size="small" />
              ) : (
                <Text style={styles.consultButtonText}>CONSULTAR SALDO</Text>
              )}
            </TouchableOpacity>
          </View>
        </View>

        {/* Resultado */}
        {saldo !== null && (
          <View style={styles.resultCard}>
            <View style={styles.resultHeader}>
              <Text style={styles.resultIcon}>💰</Text>
              <Text style={styles.resultTitle}>Saldo Disponible</Text>
            </View>
            {nombre && <Text style={styles.userName}>Cliente: {nombre}</Text>}
            <Text style={styles.resultAmount}>{formatCurrency(saldo)}</Text>
            <TouchableOpacity style={styles.newConsultButton} onPress={handleReset}>
              <Text style={styles.newConsultButtonText}>Nueva Consulta</Text>
            </TouchableOpacity>
          </View>
        )}
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#370000', // Fondo rojo oscuro
  },
  scrollContainer: {
    flexGrow: 1,
    padding: 20,
  },
  header: {
    marginBottom: 24,
  },
  headerTitle: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#f2cd54', // Dorado
    marginBottom: 8,
    textAlign: 'center',
  },
  headerSubtitle: {
    fontSize: 14,
    color: '#f2cd54', // Dorado
    textAlign: 'center',
    opacity: 0.8,
  },
  formCard: {
    backgroundColor: '#4a0000', // Rojo más claro para contraste
    borderRadius: 16,
    padding: 20,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 5,
    marginBottom: 20,
    borderWidth: 1,
    borderColor: '#f2cd54',
  },
  inputGroup: {
    marginBottom: 20,
  },
  label: {
    fontSize: 14,
    fontWeight: '600',
    color: '#f2cd54', // Dorado
    marginBottom: 4,
  },
  labelHint: {
    fontSize: 12,
    color: '#c9a03d',
    marginBottom: 8,
  },
  input: {
    borderWidth: 2,
    borderColor: '#f2cd54', // Borde dorado
    borderRadius: 12,
    paddingHorizontal: 16,
    paddingVertical: 14,
    fontSize: 16,
    backgroundColor: '#2a0000',
    color: '#f2cd54',
  },
  buttonContainer: {
    flexDirection: 'row',
    gap: 12,
    marginTop: 10,
  },
  button: {
    flex: 1,
    paddingVertical: 14,
    borderRadius: 12,
    alignItems: 'center',
    borderWidth: 2,
    borderColor: '#f2cd54',
  },
  cancelButton: {
    backgroundColor: '#370000',
  },
  cancelButtonText: {
    color: '#f2cd54',
    fontWeight: '600',
  },
  consultButton: {
    backgroundColor: '#370000',
  },
  consultButtonText: {
    color: '#f2cd54',
    fontWeight: '600',
    fontSize: 14,
  },
  disabledButton: {
    opacity: 0.6,
  },
  resultCard: {
    backgroundColor: '#4a0000',
    borderRadius: 16,
    padding: 24,
    alignItems: 'center',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 5,
    borderTopWidth: 4,
    borderTopColor: '#f2cd54',
    borderWidth: 1,
    borderColor: '#f2cd54',
  },
  resultHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 16,
  },
  resultIcon: {
    fontSize: 24,
    marginRight: 8,
  },
  resultTitle: {
    fontSize: 16,
    color: '#f2cd54',
  },
  userName: {
    fontSize: 14,
    color: '#c9a03d',
    marginBottom: 16,
  },
  resultAmount: {
    fontSize: 36,
    fontWeight: 'bold',
    color: '#f2cd54',
    marginVertical: 16,
  },
  newConsultButton: {
    backgroundColor: '#370000',
    paddingVertical: 12,
    paddingHorizontal: 24,
    borderRadius: 8,
    borderWidth: 2,
    borderColor: '#f2cd54',
  },
  newConsultButtonText: {
    color: '#f2cd54',
    fontWeight: '600',
  },
});