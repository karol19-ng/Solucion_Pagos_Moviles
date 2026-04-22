import React, { useState } from 'react';
import {
  View, Text, TextInput, TouchableOpacity, StyleSheet,
  KeyboardAvoidingView, Platform, ScrollView, ActivityIndicator,
} from 'react-native';
import  theme  from '../../styles/theme';
import { useAuth } from '../../hooks/useAuth';

export default function LoginScreen() {
  const [usuario, setUsuario] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const { login, error, isLoading, isBlocked, attempts } = useAuth();

  const handleLogin = async () => {
    if (!usuario.trim() || !password.trim()) {
      return;
    }
    await login(usuario.trim(), password, rememberMe);
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">

        {/* Logo / Marca */}
        <View style={styles.logoContainer}>
          <View style={styles.logoCircle}>
            <Text style={styles.logoText}>N</Text>
          </View>
          <Text style={styles.brandName}>NexusPay</Text>
          <Text style={styles.brandSub}>Pagos Móviles</Text>
        </View>

        {/* Card */}
        <View style={styles.card}>
          <Text style={styles.title}>Iniciar Sesión</Text>

          {/* Error */}
          {error && (
            <View style={styles.errorBox}>
              <Text style={styles.errorText}>{error}</Text>
            </View>
          )}

          {/* Intentos */}
          {attempts > 0 && attempts < 3 && (
            <Text style={styles.attemptsText}>
              Intentos fallidos: {attempts}/3
            </Text>
          )}

          {/* Usuario */}
          <Text style={styles.label}>Usuario</Text>
          <TextInput
            style={[styles.input, isBlocked && styles.inputDisabled]}
            placeholder="Ingrese su usuario"
            placeholderTextColor={theme.colors.textSecondary}
            value={usuario}
            onChangeText={setUsuario}
            autoCapitalize="none"
            editable={!isBlocked}
          />

          {/* Contraseña */}
          <Text style={styles.label}>Contraseña</Text>
          <View style={styles.passwordContainer}>
            <TextInput
              style={[styles.inputPassword, isBlocked && styles.inputDisabled]}
              placeholder="Ingrese su contraseña"
              placeholderTextColor={theme.colors.textSecondary}
              value={password}
              onChangeText={setPassword}
              secureTextEntry={!showPassword}
              editable={!isBlocked}
            />
            <TouchableOpacity
              style={styles.eyeBtn}
              onPress={() => setShowPassword(v => !v)}
            >
              <Text style={styles.eyeText}>{showPassword ? '🙈' : '👁️'}</Text>
            </TouchableOpacity>
          </View>

          {/* Recordarme */}
          <TouchableOpacity
            style={styles.rememberRow}
            onPress={() => setRememberMe(v => !v)}
          >
            <View style={[styles.checkbox, rememberMe && styles.checkboxActive]}>
              {rememberMe && <Text style={styles.checkmark}>✓</Text>}
            </View>
            <Text style={styles.rememberText}>Recordarme</Text>
          </TouchableOpacity>

          {/* Botón */}
          <TouchableOpacity
            style={[styles.btn, (isLoading || isBlocked) && styles.btnDisabled]}
            onPress={handleLogin}
            disabled={isLoading || isBlocked}
          >
            {isLoading ? (
              <ActivityIndicator color="#0A0E17" />
            ) : (
              <Text style={styles.btnText}>ACEPTAR</Text>
            )}
          </TouchableOpacity>
        </View>

      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: theme.colors.background,
  },
  scroll: {
    flexGrow: 1,
    justifyContent: 'center',
    padding: theme.spacing.lg,
  },
  logoContainer: {
    alignItems: 'center',
    marginBottom: theme.spacing.xl,
  },
  logoCircle: {
    width: 80,
    height: 80,
    borderRadius: 40,
    backgroundColor: theme.colors.primary,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: theme.spacing.sm,
    shadowColor: theme.colors.primary,
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0.6,
    shadowRadius: 20,
    elevation: 10,
  },
  logoText: {
    fontSize: 36,
    fontWeight: 'bold',
    color: theme.colors.background,
  },
  brandName: {
    fontSize: 28,
    fontWeight: 'bold',
    color: theme.colors.primary,
    letterSpacing: 2,
  },
  brandSub: {
    fontSize: 13,
    color: theme.colors.textSecondary,
    letterSpacing: 1,
  },
  card: {
    backgroundColor: theme.colors.cardBg,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.lg,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  title: {
    fontSize: 20,
    fontWeight: 'bold',
    color: theme.colors.textPrimary,
    marginBottom: theme.spacing.md,
    textAlign: 'center',
  },
  errorBox: {
    backgroundColor: 'rgba(255,76,76,0.15)',
    borderRadius: theme.radius.sm,
    padding: theme.spacing.sm,
    marginBottom: theme.spacing.sm,
    borderWidth: 1,
    borderColor: theme.colors.error,
  },
  errorText: {
    color: theme.colors.error,
    fontSize: 13,
    textAlign: 'center',
  },
  attemptsText: {
    color: theme.colors.warning,
    fontSize: 12,
    textAlign: 'center',
    marginBottom: theme.spacing.sm,
  },
  label: {
    color: theme.colors.textSecondary,
    fontSize: 12,
    marginBottom: 4,
    marginTop: theme.spacing.sm,
  },
  input: {
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.sm,
    borderWidth: 1,
    borderColor: theme.colors.border,
    color: theme.colors.textPrimary,
    padding: theme.spacing.sm + 4,
    fontSize: 15,
  },
  inputDisabled: {
    opacity: 0.5,
  },
  passwordContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: theme.colors.surface,
    borderRadius: theme.radius.sm,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  inputPassword: {
    flex: 1,
    color: theme.colors.textPrimary,
    padding: theme.spacing.sm + 4,
    fontSize: 15,
  },
  eyeBtn: {
    padding: theme.spacing.sm,
  },
  eyeText: {
    fontSize: 16,
  },
  rememberRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: theme.spacing.md,
    marginBottom: theme.spacing.sm,
  },
  checkbox: {
    width: 20,
    height: 20,
    borderRadius: 4,
    borderWidth: 2,
    borderColor: theme.colors.primary,
    marginRight: theme.spacing.sm,
    justifyContent: 'center',
    alignItems: 'center',
  },
  checkboxActive: {
    backgroundColor: theme.colors.primary,
  },
  checkmark: {
    color: theme.colors.background,
    fontSize: 12,
    fontWeight: 'bold',
  },
  rememberText: {
    color: theme.colors.textSecondary,
    fontSize: 13,
  },
  btn: {
    backgroundColor: theme.colors.primary,
    borderRadius: theme.radius.md,
    padding: theme.spacing.md,
    alignItems: 'center',
    marginTop: theme.spacing.md,
  },
  btnDisabled: {
    opacity: 0.5,
  },
  btnText: {
    color: theme.colors.background,
    fontWeight: 'bold',
    fontSize: 15,
    letterSpacing: 1,
  },
});
