// app/auth/login.tsx
import React, { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, StyleSheet, Alert } from 'react-native';
import { useRouter } from 'expo-router';
import apiClient from '../../services/apiClient';
import storageService from '../../utils/storage';



export default function LoginScreen() {
  const router = useRouter();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = async () => {
    if (!email || !password) {
      Alert.alert('Error', 'Complete todos los campos');
      return;
    }

    setLoading(true);
    try {
      const response = await apiClient.post('/auth/login', { email, password });
      
      if (response.data.codigo === 0 && response.data.access_token) {
        await storageService.saveToken(response.data.access_token);
        Alert.alert('Éxito', 'Login exitoso');
        router.replace('../portal/balance');
      } else {
        Alert.alert('Error', response.data.descripcion || 'Credenciales incorrectas');
      }
    } catch (error) {
      Alert.alert('Error', 'Error de conexión. Verifique el servidor');
    } finally {
      setLoading(false);
    }
  };

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Iniciar Sesión</Text>
      <TextInput
        style={styles.input}
        placeholder="Email"
        value={email}
        onChangeText={setEmail}
        autoCapitalize="none"
      />
      <TextInput
        style={styles.input}
        placeholder="Contraseña"
        value={password}
        onChangeText={setPassword}
        secureTextEntry
      />
      <TouchableOpacity style={styles.button} onPress={handleLogin} disabled={loading}>
        <Text style={styles.buttonText}>{loading ? 'Cargando...' : 'Ingresar'}</Text>
      </TouchableOpacity>
      
      <TouchableOpacity onPress={() => router.push('../portal/balance')}>
        <Text style={styles.link}>Probar AM6 (Consulta Saldo)</Text>
      </TouchableOpacity>
      
      <TouchableOpacity onPress={() => router.push('../portal/unsubscribe')}>
        <Text style={styles.link}>Probar AM5 (Desinscripción)</Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 20, justifyContent: 'center', backgroundColor: '#f5f5f5' },
  title: { fontSize: 28, fontWeight: 'bold', textAlign: 'center', marginBottom: 40 },
  input: { borderWidth: 1, borderColor: '#ddd', borderRadius: 8, padding: 12, marginBottom: 15, fontSize: 16, backgroundColor: '#fff' },
  button: { backgroundColor: '#007aff', padding: 15, borderRadius: 8, alignItems: 'center' },
  buttonText: { color: '#fff', fontWeight: 'bold', fontSize: 16 },
  link: { textAlign: 'center', marginTop: 15, color: '#007aff', fontSize: 14 },
});