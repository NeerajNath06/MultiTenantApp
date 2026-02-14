import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  TouchableOpacity,
  Alert,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Button from '../../components/common/Button';
import Input from '../../components/common/Input';
import { SafeAreaView } from 'react-native-safe-area-context';
import { GuardRegistrationScreenProps } from '../../types/navigation';

const GuardRegistrationScreen: React.FC<GuardRegistrationScreenProps> = ({ navigation }) => {
  const [username, setUsername] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [confirmPassword, setConfirmPassword] = useState<string>('');
  const [email, setEmail] = useState<string>('');
  const [mobile, setMobile] = useState<string>('');
  const [agencyId, setAgencyId] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!username.trim()) {
      newErrors.username = 'Username is required';
    } else if (username.length < 3) {
      newErrors.username = 'Username must be at least 3 characters';
    }

    if (!password) {
      newErrors.password = 'Password is required';
    } else if (password.length < 6) {
      newErrors.password = 'Password must be at least 6 characters';
    }

    if (!confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your password';
    } else if (password !== confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }

    if (!mobile.trim()) {
      newErrors.mobile = 'Mobile number is required';
    } else if (!/^[0-9]{10}$/.test(mobile)) {
      newErrors.mobile = 'Please enter a valid 10-digit mobile number';
    }

    if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      newErrors.email = 'Please enter a valid email address';
    }

    if (!agencyId.trim()) {
      newErrors.agencyId = 'Agency ID is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleRegister = async (): Promise<void> => {
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    try {
      // TODO: Replace with actual API call
      const registerData = {
        username: username.trim(),
        password: password,
        email: email.trim() || null,
        mobile: mobile.trim(),
        roleId: 6, // Guard role
        agencyId: parseInt(agencyId, 10),
      };

      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 1500));

      Alert.alert(
        'Registration Successful',
        'Your account has been created successfully. Please login to continue.',
        [
          {
            text: 'OK',
            onPress: () => navigation.navigate('Login'),
          },
        ]
      );
    } catch (error) {
      Alert.alert(
        'Registration Failed',
        'An error occurred during registration. Please try again.',
        [{ text: 'OK' }]
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        style={styles.keyboardView}
      >
        <ScrollView
          contentContainerStyle={styles.scrollContent}
          showsVerticalScrollIndicator={false}
        >
          {/* Header with Logo */}
          <LinearGradient
            colors={[COLORS.primary, COLORS.primaryLight]}
            style={styles.header}
          >
            <TouchableOpacity
              style={styles.backButton}
              onPress={() => navigation.goBack()}
            >
              <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
            </TouchableOpacity>
            <View style={styles.logoContainer}>
              <MaterialCommunityIcons name="account-plus" size={60} color={COLORS.white} />
            </View>
            <Text style={styles.appName}>Guard Registration</Text>
            <Text style={styles.tagline}>Create your security guard account</Text>
          </LinearGradient>

          {/* Registration Form */}
          <View style={styles.formContainer}>
            <Text style={styles.welcomeText}>Join Us!</Text>
            <Text style={styles.subtitleText}>Fill in your details to get started</Text>

            {/* Input Fields */}
            <Input
              label="Username"
              placeholder="Enter your username"
              value={username}
              onChangeText={(text) => {
                setUsername(text);
                if (errors.username) setErrors({ ...errors, username: '' });
              }}
              icon="account-outline"
              error={errors.username}
            />

            <Input
              label="Mobile Number"
              placeholder="Enter your 10-digit mobile number"
              value={mobile}
              onChangeText={(text) => {
                setMobile(text.replace(/[^0-9]/g, '').slice(0, 10));
                if (errors.mobile) setErrors({ ...errors, mobile: '' });
              }}
              keyboardType="phone-pad"
              icon="phone-outline"
              error={errors.mobile}
            />

            <Input
              label="Email (Optional)"
              placeholder="Enter your email address"
              value={email}
              onChangeText={(text) => {
                setEmail(text);
                if (errors.email) setErrors({ ...errors, email: '' });
              }}
              keyboardType="email-address"
              icon="email-outline"
              error={errors.email}
            />

            <Input
              label="Agency ID"
              placeholder="Enter your agency ID"
              value={agencyId}
              onChangeText={(text) => {
                setAgencyId(text.replace(/[^0-9]/g, ''));
                if (errors.agencyId) setErrors({ ...errors, agencyId: '' });
              }}
              keyboardType="numeric"
              icon="office-building-outline"
              error={errors.agencyId}
            />

            <Input
              label="Password"
              placeholder="Enter your password (min 6 characters)"
              value={password}
              onChangeText={(text) => {
                setPassword(text);
                if (errors.password) setErrors({ ...errors, password: '' });
              }}
              secureTextEntry
              icon="lock-outline"
              error={errors.password}
            />

            <Input
              label="Confirm Password"
              placeholder="Re-enter your password"
              value={confirmPassword}
              onChangeText={(text) => {
                setConfirmPassword(text);
                if (errors.confirmPassword) setErrors({ ...errors, confirmPassword: '' });
              }}
              secureTextEntry
              icon="lock-check-outline"
              error={errors.confirmPassword}
            />

            {/* Register Button */}
            <Button
              title="Register"
              onPress={handleRegister}
              loading={loading}
              style={styles.registerButton}
            />

            {/* Login Link */}
            <View style={styles.loginLinkContainer}>
              <Text style={styles.loginLinkText}>Already have an account? </Text>
              <TouchableOpacity onPress={() => navigation.navigate('Login')}>
                <Text style={styles.loginLink}>Login</Text>
              </TouchableOpacity>
            </View>
          </View>

          {/* Footer */}
          <View style={styles.footer}>
            <Text style={styles.footerText}>Powered by Security App</Text>
            <Text style={styles.versionText}>Version 1.0.0</Text>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  keyboardView: {
    flex: 1,
  },
  scrollContent: {
    flexGrow: 1,
  },
  header: {
    paddingTop: SIZES.xl,
    paddingBottom: SIZES.xl,
    alignItems: 'center',
    borderBottomLeftRadius: SIZES.radiusXl,
    borderBottomRightRadius: SIZES.radiusXl,
    position: 'relative',
  },
  backButton: {
    position: 'absolute',
    top: SIZES.xl,
    left: SIZES.lg,
    zIndex: 1,
    padding: SIZES.xs,
  },
  logoContainer: {
    width: 100,
    height: 100,
    borderRadius: 50,
    backgroundColor: 'rgba(255,255,255,0.2)',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: SIZES.md,
    marginTop: SIZES.lg,
  },
  appName: {
    fontSize: FONTS.h2,
    fontWeight: 'bold',
    color: COLORS.white,
  },
  tagline: {
    fontSize: FONTS.bodySmall,
    color: 'rgba(255,255,255,0.8)',
    marginTop: SIZES.xs,
  },
  formContainer: {
    flex: 1,
    paddingHorizontal: SIZES.lg,
    paddingTop: SIZES.xl,
  },
  welcomeText: {
    fontSize: FONTS.h2,
    fontWeight: 'bold',
    color: COLORS.textPrimary,
  },
  subtitleText: {
    fontSize: FONTS.body,
    color: COLORS.textSecondary,
    marginTop: SIZES.xs,
    marginBottom: SIZES.lg,
  },
  registerButton: {
    marginTop: SIZES.md,
  },
  loginLinkContainer: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: SIZES.lg,
  },
  loginLinkText: {
    fontSize: FONTS.body,
    color: COLORS.textSecondary,
  },
  loginLink: {
    fontSize: FONTS.body,
    color: COLORS.primary,
    fontWeight: '600',
  },
  footer: {
    alignItems: 'center',
    paddingVertical: SIZES.lg,
  },
  footerText: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
  },
  versionText: {
    fontSize: FONTS.tiny,
    color: COLORS.textLight,
    marginTop: SIZES.xs,
  },
});

export default GuardRegistrationScreen;
