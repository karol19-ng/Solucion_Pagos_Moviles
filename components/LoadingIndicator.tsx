import React from 'react';
import { View, ActivityIndicator, StyleSheet } from 'react-native';
import { colors } from '../styles/theme';

export const LoadingIndicator = () => (
  <View style={styles.container}>
    <ActivityIndicator size="large" color={colors.primaryAccent} />
  </View>
);

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.primaryBackground }
});