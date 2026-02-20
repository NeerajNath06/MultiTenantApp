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
import { COLORS, FONTS, SIZES, SHADOWS, moderateScale, horizontalPadding } from '../../constants/theme';
import Button from '../../components/common/Button';
import Input from '../../components/common/Input';
import { SafeAreaView } from 'react-native-safe-area-context';
import { LoginScreenProps } from '../../types/navigation';
import { authService, LoginRequest } from '../../services/authService';

const LoginScreen: React.FC<LoginScreenProps> = ({ navigation }) => {
  const [email, setEmail] = useState<string>('');
  const [password, setPassword] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [userType, setUserType] = useState<string>('guard'); // 'guard' or 'supervisor'

  const validateForm = (): boolean => {
    if (!email.trim()) {
      Alert.alert('Validation Error', 'Please enter your email address');
      return false;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      Alert.alert('Validation Error', 'Please enter a valid email address');
      return false;
    }

    if (!password.trim()) {
      Alert.alert('Validation Error', 'Please enter your password');
      return false;
    }

    if (password.length < 6) {
      Alert.alert('Validation Error', 'Password must be at least 6 characters long');
      return false;
    }

    return true;
  };

  const handleLogin = async (): Promise<void> => {
    if (!validateForm()) {
      return;
    }

    setLoading(true);

    try {
      const loginRequest: LoginRequest = {
        userEmail: email.trim(),
        password: password.trim(),
      };

      const response = await authService.login(loginRequest);

      if (response.success && response.data) {
        Alert.alert(
          'Login Successful',
          response.data.message || 'Welcome back!',
          [
            {
              text: 'OK',
              onPress: () => {
                const user = response.data!.user;
                const userRole = (user.role || '').toLowerCase();
                const isSupervisor = user.isSupervisor ?? userRole.includes('supervisor');
                if (isSupervisor) {
                  navigation.reset({
                    index: 0,
                    routes: [{ name: 'SupervisorMain' }],
                  });
                } else {
                  navigation.reset({
                    index: 0,
                    routes: [{ name: 'GuardMain' }],
                  });
                }
              },
            },
          ]
        );
      } else {
        const errorMessage = response.error?.message || 'Login failed. Please try again.';
        const title = response.error?.code === 'MOBILE_ACCESS_DENIED' ? 'Access Denied' : 'Login Failed';
        Alert.alert(title, errorMessage);
      }
    } catch (error) {
      console.error('Login error:', error);
      Alert.alert(
        'Network Error',
        'Unable to connect to server. Please check your internet connection and try again.'
      );
    } finally {
      setLoading(false);
    }
  };

  const handleForgotPassword = (): void => {
    if (!email.trim()) {
      Alert.alert(
        'Email Required',
        'Please enter your email address first to reset your password.'
      );
      return;
    }
    navigation.navigate('ForgotPassword', { email: email.trim() });
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
            <View style={styles.logoContainer}>
              <MaterialCommunityIcons name="shield-check" size={60} color={COLORS.white} />
            </View>
            <Text style={styles.appName}>Security Guard</Text>
            <Text style={styles.tagline}>Secure • Monitor • Protect</Text>
          </LinearGradient>

          {/* Login Form */}
          <View style={styles.formContainer}>
            <Text style={styles.welcomeText}>Welcome Back!</Text>
            <Text style={styles.subtitleText}>Sign in to continue</Text>

            {/* User Type Toggle */}
            <View style={styles.toggleContainer}>
              <TouchableOpacity
                style={[
                  styles.toggleButton,
                  userType === 'guard' && styles.activeToggle,
                ]}
                onPress={() => setUserType('guard')}
              >
                <MaterialCommunityIcons
                  name="account"
                  size={20}
                  color={userType === 'guard' ? COLORS.white : COLORS.gray500}
                />
                <Text
                  style={[
                    styles.toggleText,
                    userType === 'guard' && styles.activeToggleText,
                  ]}
                >
                  Guard
                </Text>
              </TouchableOpacity>
              <TouchableOpacity
                style={[
                  styles.toggleButton,
                  userType === 'supervisor' && styles.activeToggle,
                ]}
                onPress={() => setUserType('supervisor')}
              >
                <MaterialCommunityIcons
                  name="account-group"
                  size={20}
                  color={userType === 'supervisor' ? COLORS.white : COLORS.gray500}
                />
                <Text
                  style={[
                    styles.toggleText,
                    userType === 'supervisor' && styles.activeToggleText,
                  ]}
                >
                  Supervisor
                </Text>
              </TouchableOpacity>
            </View>

            {/* Input Fields */}
            <Input
              label="Email Address"
              placeholder="Enter your email address"
              value={email}
              onChangeText={setEmail}
              keyboardType="email-address"
              icon="email-outline"
            />

            <Input
              label="Password"
              placeholder="Enter your password"
              value={password}
              onChangeText={setPassword}
              secureTextEntry
              icon="lock-outline"
            />

            {/* Forgot Password */}
            <TouchableOpacity
              style={styles.forgotPassword}
              onPress={handleForgotPassword}
            >
              <Text style={styles.forgotPasswordText}>Forgot Password?</Text>
            </TouchableOpacity>

            {/* Login Button */}
            <Button
              title="Login"
              onPress={handleLogin}
              loading={loading}
              style={styles.loginButton}
            />

            {/* Language Selection */}
            <TouchableOpacity
              style={styles.languageButton}
              onPress={() => navigation.navigate('LanguageSelection')}
            >
              <MaterialCommunityIcons name="translate" size={20} color={COLORS.primary} />
              <Text style={styles.languageText}>Change Language</Text>
            </TouchableOpacity>
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
    paddingBottom: SIZES.xxl,
  },
  header: {
    paddingTop: SIZES.xxl,
    paddingBottom: SIZES.xl,
    alignItems: 'center',
    borderBottomLeftRadius: SIZES.radiusXl,
    borderBottomRightRadius: SIZES.radiusXl,
  },
  logoContainer: {
    width: moderateScale(100),
    height: moderateScale(100),
    borderRadius: moderateScale(50),
    backgroundColor: 'rgba(255,255,255,0.2)',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: SIZES.md,
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
    paddingHorizontal: horizontalPadding() || SIZES.lg,
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
  toggleContainer: {
    flexDirection: 'row',
    backgroundColor: COLORS.gray100,
    borderRadius: SIZES.radiusMd,
    padding: SIZES.xs,
    marginBottom: SIZES.lg,
  },
  toggleButton: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: SIZES.sm,
    borderRadius: SIZES.radiusSm,
    gap: SIZES.xs,
  },
  activeToggle: {
    backgroundColor: COLORS.primary,
  },
  toggleText: {
    fontSize: FONTS.bodySmall,
    fontWeight: '600',
    color: COLORS.gray500,
  },
  activeToggleText: {
    color: COLORS.white,
  },
  forgotPassword: {
    alignSelf: 'flex-end',
    marginTop: -SIZES.sm,
    marginBottom: SIZES.lg,
  },
  forgotPasswordText: {
    fontSize: FONTS.bodySmall,
    color: COLORS.primary,
    fontWeight: '600',
  },
  loginButton: {
    marginTop: SIZES.sm,
  },
  languageButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    marginTop: SIZES.lg,
    gap: SIZES.sm,
  },
  languageText: {
    fontSize: FONTS.bodySmall,
    color: COLORS.primary,
    fontWeight: '500',
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

export default LoginScreen;
