// app/portal/unsubscribe.tsx - AM5
import React, { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ScrollView,
  Alert,
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
} from 'react-native';
import { useRouter } from 'expo-router';
import { useAuth } from '../../hooks/useAuth';
import portalService from '../../services/portalService';

export default function UnsubscribeScreen() {
  const router = useRouter();
  const { user } = useAuth();
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    numeroCuenta: '',
    identificacion: '',
    numeroTelefono: '',
  });
  const [mostrarConfirmacion, setMostrarConfirmacion] = useState(false);

  const handleChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const validateForm = (): boolean => {
    if (!formData.numeroCuenta.trim()) {
      Alert.alert('Error', 'Debe ingresar el número de cuenta');
      return false;
    }
    if (!formData.identificacion.trim()) {
      Alert.alert('Error', 'Debe ingresar la identificación');
      return false;
    }
    if (!formData.numeroTelefono.trim()) {
      Alert.alert('Error', 'Debe ingresar el número de teléfono');
      return false;
    }
    if (!/^\d{8}$/.test(formData.numeroTelefono)) {
      Alert.alert('Error', 'El teléfono debe tener 8 dígitos');
      return false;
    }
    return true;
  };

  const handleUnsubscribe = async () => {
    if (!validateForm()) return;
    setMostrarConfirmacion(true);
  };

  const confirmUnsubscribe = async () => {
    setMostrarConfirmacion(false);
    setLoading(true);
    try {
      const response = await portalService.desinscribir(formData);
      if (response.codigo === 0) {
        Alert.alert(
          'Éxito',
          response.descripcion || 'Desinscripción realizada correctamente',
          [{ text: 'OK', onPress: () => router.back() }]
        );
      } else {
        Alert.alert('Error', response.descripcion);
      }
    } catch (error) {
      Alert.alert('Error', 'Ocurrió un error al procesar la desinscripción');
    } finally {
      setLoading(false);
    }
  };

  React.useEffect(() => {
    if (user) {
      setFormData(prev => ({
        ...prev,
        identificacion: user.identificacion || '',
        numeroTelefono: user.telefono || '',
      }));
    }
  }, [user]);

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView contentContainerStyle={styles.scrollContainer}>
        {/* Header */}
        <View style={styles.header}>
          <TouchableOpacity onPress={() => router.back()} style={styles.backButton}>
            <Text style={styles.backButtonText}>←</Text>
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Desinscripción</Text>
        </View>

        {/* Alerta */}
        <View style={styles.alertCard}>
          <Text style={styles.alertIcon}>⚠️</Text>
          <Text style={styles.alertText}>
            Al desinscribirse, no podrá realizar transferencias hasta que se inscriba nuevamente.
          </Text>
        </View>

        {/* Formulario */}
        <View style={styles.formCard}>
          <View style={styles.inputGroup}>
            <Text style={styles.label}>Número de Cuenta</Text>
            <TextInput
              style={styles.input}
              placeholder="CR012345678901234568"
              placeholderTextColor="#a08040"
              value={formData.numeroCuenta}
              onChangeText={(v) => handleChange('numeroCuenta', v)}
            />
          </View>

          <View style={styles.inputGroup}>
            <Text style={styles.label}>Identificación</Text>
            <TextInput
              style={[styles.input, user?.identificacion && styles.disabledInput]}
              placeholder="123456789"
              placeholderTextColor="#a08040"
              value={formData.identificacion}
              onChangeText={(v) => handleChange('identificacion', v)}
              keyboardType="numeric"
              editable={!user?.identificacion}
            />
          </View>

          <View style={styles.inputGroup}>
            <Text style={styles.label}>Número de Teléfono</Text>
            <TextInput
              style={[styles.input, user?.telefono && styles.disabledInput]}
              placeholder="88888888"
              placeholderTextColor="#a08040"
              value={formData.numeroTelefono}
              onChangeText={(v) => handleChange('numeroTelefono', v)}
              keyboardType="phone-pad"
              maxLength={8}
              editable={!user?.telefono}
            />
            <Text style={styles.hint}>8 dígitos, solo números</Text>
          </View>

          <TouchableOpacity
            style={[styles.submitButton, loading && styles.disabledButton]}
            onPress={handleUnsubscribe}
            disabled={loading}
          >
            {loading ? (
              <ActivityIndicator color="#f2cd54" />
            ) : (
              <Text style={styles.submitButtonText}>DESINSCRIBIRME</Text>
            )}
          </TouchableOpacity>
        </View>

        {/* Modal de confirmación */}
        {mostrarConfirmacion && (
          <View style={styles.modalOverlay}>
            <View style={styles.modalCard}>
              <Text style={styles.modalIcon}>⚠️</Text>
              <Text style={styles.modalTitle}>Confirmar Desinscripción</Text>
              <Text style={styles.modalText}>
                ¿Está seguro que desea desinscribirse del servicio de Pagos Móviles?
              </Text>
              <View style={styles.modalButtons}>
                <TouchableOpacity
                  style={[styles.modalButton, styles.modalCancel]}
                  onPress={() => setMostrarConfirmacion(false)}
                >
                  <Text style={styles.modalCancelText}>Cancelar</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  style={[styles.modalButton, styles.modalConfirm]}
                  onPress={confirmUnsubscribe}
                >
                  <Text style={styles.modalConfirmText}>Confirmar</Text>
                </TouchableOpacity>
              </View>
            </View>
          </View>
        )}
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#370000',
  },
  scrollContainer: {
    flexGrow: 1,
    padding: 20,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 20,
    marginTop: 20,
  },
  backButton: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: '#4a0000',
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: '#f2cd54',
    marginRight: 15,
  },
  backButtonText: {
    fontSize: 24,
    color: '#f2cd54',
  },
  headerTitle: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#f2cd54',
    flex: 1,
  },
  alertCard: {
    backgroundColor: '#4a0000',
    borderRadius: 12,
    padding: 16,
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 24,
    borderWidth: 1,
    borderColor: '#f2cd54',
  },
  alertIcon: {
    fontSize: 24,
    marginRight: 12,
  },
  alertText: {
    flex: 1,
    fontSize: 13,
    color: '#f2cd54',
    lineHeight: 18,
  },
  formCard: {
    backgroundColor: '#4a0000',
    borderRadius: 20,
    padding: 24,
    borderWidth: 1,
    borderColor: '#f2cd54',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 5,
  },
  inputGroup: {
    marginBottom: 20,
  },
  label: {
    fontSize: 14,
    fontWeight: '600',
    color: '#f2cd54',
    marginBottom: 8,
  },
  input: {
    borderWidth: 2,
    borderColor: '#f2cd54',
    borderRadius: 12,
    paddingHorizontal: 16,
    paddingVertical: 14,
    fontSize: 16,
    backgroundColor: '#2a0000',
    color: '#f2cd54',
  },
  disabledInput: {
    opacity: 0.7,
  },
  hint: {
    fontSize: 12,
    color: '#c9a03d',
    marginTop: 6,
  },
  submitButton: {
    backgroundColor: '#370000',
    paddingVertical: 16,
    borderRadius: 12,
    alignItems: 'center',
    borderWidth: 2,
    borderColor: '#f2cd54',
    marginTop: 10,
  },
  submitButtonText: {
    color: '#f2cd54',
    fontWeight: 'bold',
    fontSize: 16,
  },
  disabledButton: {
    opacity: 0.6,
  },
  modalOverlay: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(0,0,0,0.7)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  modalCard: {
    backgroundColor: '#4a0000',
    borderRadius: 20,
    padding: 24,
    width: '80%',
    alignItems: 'center',
    borderWidth: 2,
    borderColor: '#f2cd54',
  },
  modalIcon: {
    fontSize: 48,
    marginBottom: 16,
  },
  modalTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#f2cd54',
    marginBottom: 12,
  },
  modalText: {
    fontSize: 14,
    color: '#f2cd54',
    textAlign: 'center',
    marginBottom: 24,
  },
  modalButtons: {
    flexDirection: 'row',
    gap: 12,
    width: '100%',
  },
  modalButton: {
    flex: 1,
    paddingVertical: 12,
    borderRadius: 10,
    alignItems: 'center',
    borderWidth: 1,
  },
  modalCancel: {
    backgroundColor: '#370000',
    borderColor: '#f2cd54',
  },
  modalCancelText: {
    color: '#f2cd54',
  },
  modalConfirm: {
    backgroundColor: '#f2cd54',
    borderColor: '#f2cd54',
  },
  modalConfirmText: {
    color: '#370000',
    fontWeight: 'bold',
  },
});