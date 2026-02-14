import React, { useState } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Button from '../../components/common/Button';
import Input from '../../components/common/Input';
import { ChangePasswordScreenProps } from '../../types/navigation';

const ChangePasswordScreen: React.FC<ChangePasswordScreenProps> = ({ navigation }) => {
  const [currentPassword, setCurrentPassword] = useState<string>('');
  const [newPassword, setNewPassword] = useState<string>('');
  const [confirmPassword, setConfirmPassword] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);

  const validatePasswords = (): boolean => {
    if (newPassword !== confirmPassword) {
      Alert.alert('Error', 'New passwords do not match');
      return false;
    }
    if (newPassword.length < 6) {
      Alert.alert('Error', 'Password must be at least 6 characters long');
      return false;
    }
    return true;
  };

  const handleChangePassword = (): void => {
    if (!currentPassword || !newPassword || !confirmPassword) {
      Alert.alert('Error', 'Please fill in all fields');
      return;
    }

    if (!validatePasswords()) {
      return;
    }

    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      Alert.alert('Success', 'Password changed successfully', [
        { text: 'OK', onPress: () => navigation.goBack() }
      ]);
    }, 2000);
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Change Password</Text>
        <View style={styles.placeholder} />
      </View>

      <View style={styles.content}>
        <Text style={styles.subtitle}>
          Enter your current password and choose a new one
        </Text>

        <Input
          label="Current Password"
          placeholder="Enter current password"
          value={currentPassword}
          onChangeText={setCurrentPassword}
          secureTextEntry
          icon="lock-outline"
        />

        <Input
          label="New Password"
          placeholder="Enter new password"
          value={newPassword}
          onChangeText={setNewPassword}
          secureTextEntry
          icon="key-outline"
        />

        <Input
          label="Confirm New Password"
          placeholder="Confirm new password"
          value={confirmPassword}
          onChangeText={setConfirmPassword}
          secureTextEntry
          icon="shield-check-outline"
        />

        <View style={styles.passwordRequirements}>
          <Text style={styles.requirementsTitle}>Password Requirements:</Text>
          <View style={styles.requirementItem}>
            <MaterialCommunityIcons name="check-circle" size={16} color={newPassword.length >= 6 ? COLORS.success : COLORS.gray400} />
            <Text style={[
              styles.requirementText,
              { color: newPassword.length >= 6 ? COLORS.success : COLORS.gray400 }
            ]}>
              At least 6 characters long
            </Text>
          </View>
          <View style={styles.requirementItem}>
            <MaterialCommunityIcons name="check-circle" size={16} color={newPassword === confirmPassword && newPassword.length > 0 ? COLORS.success : COLORS.gray400} />
            <Text style={[
              styles.requirementText,
              { color: newPassword === confirmPassword && newPassword.length > 0 ? COLORS.success : COLORS.gray400 }
            ]}>
              Passwords match
            </Text>
          </View>
        </View>

        <Button
          title="Update Password"
          onPress={handleChangePassword}
          loading={loading}
          style={styles.changeButton}
        />
      </View>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: SIZES.md,
    paddingVertical: SIZES.md,
    backgroundColor: COLORS.white,
    ...SHADOWS.small,
  },
  backButton: {
    width: 40,
    height: 40,
    borderRadius: 20,
    justifyContent: 'center',
    alignItems: 'center',
  },
  headerTitle: {
    fontSize: FONTS.h4,
    fontWeight: '600',
    color: COLORS.textPrimary,
  },
  placeholder: {
    width: 40,
  },
  content: {
    flex: 1,
    padding: SIZES.lg,
    paddingTop: SIZES.xl,
  },
  subtitle: {
    fontSize: FONTS.body,
    color: COLORS.textSecondary,
    textAlign: 'center',
    marginBottom: SIZES.xl,
  },
  passwordRequirements: {
    backgroundColor: COLORS.gray100,
    borderRadius: SIZES.radiusMd,
    padding: SIZES.md,
    marginVertical: SIZES.lg,
  },
  requirementsTitle: {
    fontSize: FONTS.bodySmall,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: SIZES.sm,
  },
  requirementItem: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: SIZES.xs,
  },
  requirementText: {
    fontSize: FONTS.bodySmall,
    marginLeft: SIZES.sm,
  },
  changeButton: {
    marginTop: SIZES.lg,
  },
});

export default ChangePasswordScreen;
