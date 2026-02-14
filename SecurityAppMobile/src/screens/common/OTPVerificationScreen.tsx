import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { LinearGradient } from 'expo-linear-gradient';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Button from '../../components/common/Button';
import Input from '../../components/common/Input';
import { OTPVerificationScreenProps } from '../../types/navigation';

const OTPVerificationScreen: React.FC<OTPVerificationScreenProps> = ({ navigation, route }) => {
  const { phone, userType } = route.params;
  
  const [otp, setOtp] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [resendTimer, setResendTimer] = useState<number>(30);

  useEffect(() => {
    let timer: NodeJS.Timeout;
    if (resendTimer > 0) {
      timer = setTimeout(() => setResendTimer(resendTimer - 1), 1000);
    }
    return () => clearTimeout(timer);
  }, [resendTimer]);

  const handleVerifyOTP = (): void => {
    if (otp.length !== 6) {
      Alert.alert('Error', 'Please enter a valid 6-digit OTP');
      return;
    }

    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      // Navigate based on user type
      if (userType === 'guard') {
        navigation.reset({
          index: 0,
          routes: [{ name: 'GuardMain' }],
        });
      } else {
        navigation.reset({
          index: 0,
          routes: [{ name: 'SupervisorMain' }],
        });
      }
    }, 2000);
  };

  const handleResendOTP = (): void => {
    if (resendTimer === 0) {
      setResendTimer(30);
      Alert.alert('Success', 'OTP sent successfully');
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <LinearGradient
        colors={[COLORS.primary, COLORS.primaryLight]}
        style={styles.header}
      >
        <View style={styles.headerContent}>
          <MaterialCommunityIcons name="shield-check" size={60} color={COLORS.white} />
          <Text style={styles.headerTitle}>Verify Your Number</Text>
          <Text style={styles.headerSubtitle}>
            We've sent a 6-digit verification code to {phone}
          </Text>
        </View>
      </LinearGradient>

      <View style={styles.content}>
        <Text style={styles.instructionText}>
          Enter the verification code below
        </Text>

        <Input
          label="Verification Code"
          placeholder="Enter 6-digit OTP"
          value={otp}
          onChangeText={setOtp}
          keyboardType="numeric"
          style={styles.otpInput}
        />

        <Button
          title="Verify & Continue"
          onPress={handleVerifyOTP}
          loading={loading}
          style={styles.verifyButton}
        />

        <View style={styles.resendContainer}>
          <Text style={styles.resendText}>Didn't receive the code?</Text>
          <TouchableOpacity
            onPress={handleResendOTP}
            disabled={resendTimer > 0}
          >
            <Text style={[
              styles.resendButton,
              resendTimer > 0 && styles.resendButtonDisabled
            ]}>
              {resendTimer > 0 ? `Resend in ${resendTimer}s` : 'Resend OTP'}
            </Text>
          </TouchableOpacity>
        </View>

        <TouchableOpacity
          style={styles.changeNumberButton}
          onPress={() => navigation.goBack()}
        >
          <Text style={styles.changeNumberText}>Change Phone Number</Text>
        </TouchableOpacity>
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
    paddingTop: SIZES.xxl,
    paddingBottom: SIZES.xl,
    alignItems: 'center',
    borderBottomLeftRadius: SIZES.radiusXl,
    borderBottomRightRadius: SIZES.radiusXl,
  },
  headerContent: {
    alignItems: 'center',
    paddingHorizontal: SIZES.lg,
  },
  headerTitle: {
    fontSize: FONTS.h2,
    fontWeight: 'bold',
    color: COLORS.white,
    marginTop: SIZES.md,
    textAlign: 'center',
  },
  headerSubtitle: {
    fontSize: FONTS.body,
    color: 'rgba(255,255,255,0.8)',
    marginTop: SIZES.sm,
    textAlign: 'center',
  },
  content: {
    flex: 1,
    padding: SIZES.lg,
    paddingTop: SIZES.xl,
  },
  instructionText: {
    fontSize: FONTS.body,
    color: COLORS.textSecondary,
    textAlign: 'center',
    marginBottom: SIZES.xl,
  },
  otpInput: {
    marginBottom: SIZES.xl,
  },
  verifyButton: {
    marginBottom: SIZES.xl,
  },
  resendContainer: {
    alignItems: 'center',
    marginBottom: SIZES.lg,
  },
  resendText: {
    fontSize: FONTS.bodySmall,
    color: COLORS.textSecondary,
    marginBottom: SIZES.sm,
  },
  resendButton: {
    fontSize: FONTS.body,
    color: COLORS.primary,
    fontWeight: '600',
  },
  resendButtonDisabled: {
    color: COLORS.gray400,
  },
  changeNumberButton: {
    alignItems: 'center',
    paddingVertical: SIZES.md,
  },
  changeNumberText: {
    fontSize: FONTS.bodySmall,
    color: COLORS.primary,
    fontWeight: '500',
  },
});

export default OTPVerificationScreen;
