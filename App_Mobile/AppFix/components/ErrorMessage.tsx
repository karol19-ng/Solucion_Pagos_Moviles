import React from 'react';
import { Text, StyleSheet } from 'react-native';
import { colors } from '../styles/theme';

export const ErrorMessage = ({ message }: { message?: string }) => 
  message ? <Text style={styles.text}>{message}</Text> : null;

const styles = StyleSheet.create({
  text: { color: colors.error, fontSize: 12, marginTop: 4, marginBottom: 8 }
});