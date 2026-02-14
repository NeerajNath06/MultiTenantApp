// Security App Theme - Premium Enterprise Color Scheme

export const COLORS = {
  // Primary Colors - Deep Navy Blue (Trust & Security)
  primary: '#0F172A',
  primaryLight: '#1E40AF',
  primaryDark: '#020617',
  primaryBlue: '#3B82F6',
  
  // Secondary Colors - Teal (Professional & Modern)
  secondary: '#0D9488',
  secondaryLight: '#14B8A6',
  secondaryDark: '#0F766E',
  
  // Accent Colors - Gold (Premium Feel)
  accent: '#F59E0B',
  accentLight: '#FBBF24',
  accentDark: '#D97706',
  
  // Status Colors
  success: '#10B981',
  successLight: '#34D399',
  successDark: '#059669',
  
  warning: '#F59E0B',
  warningLight: '#FCD34D',
  warningDark: '#D97706',
  
  error: '#EF4444',
  errorLight: '#F87171',
  errorDark: '#DC2626',
  
  info: '#0EA5E9',
  infoLight: '#38BDF8',
  infoDark: '#0284C7',
  
  // Neutral Colors - Modern Gray Scale
  white: '#FFFFFF',
  black: '#000000',
  gray50: '#F8FAFC',
  gray100: '#F1F5F9',
  gray200: '#E2E8F0',
  gray300: '#CBD5E1',
  gray400: '#94A3B8',
  gray500: '#64748B',
  gray600: '#475569',
  gray700: '#334155',
  gray800: '#1E293B',
  gray900: '#0F172A',
  
  // Background Colors
  background: '#F1F5F9',
  backgroundDark: '#0F172A',
  surface: '#FFFFFF',
  surfaceElevated: '#FFFFFF',
  cardBg: '#FFFFFF',
  
  // Text Colors
  textPrimary: '#0F172A',
  textSecondary: '#64748B',
  textTertiary: '#94A3B8',
  textLight: '#CBD5E1',
  textWhite: '#FFFFFF',
  textDark: '#020617',
  
  // Border Colors
  border: '#E2E8F0',
  borderLight: '#F1F5F9',
  borderDark: '#CBD5E1',
  
  // Gradient Colors
  gradientStart: '#0F172A',
  gradientMiddle: '#1E40AF',
  gradientEnd: '#3B82F6',
  
  // Special Colors
  overlay: 'rgba(15, 23, 42, 0.5)',
  overlayLight: 'rgba(15, 23, 42, 0.3)',
  shimmer: '#E2E8F0',
  
  // Chart Colors
  chart1: '#3B82F6',
  chart2: '#10B981',
  chart3: '#F59E0B',
  chart4: '#EF4444',
  chart5: '#8B5CF6',
  chart6: '#EC4899',
};

export const FONTS = {
  // Font Family
  regular: 'System',
  medium: 'System',
  semiBold: 'System',
  bold: 'System',
  
  // Font Sizes
  h1: 32,
  h2: 28,
  h3: 24,
  h4: 20,
  h5: 18,
  body: 16,
  bodySmall: 14,
  caption: 12,
  tiny: 10,
  micro: 8,
  
  // Line Heights
  lineHeightLg: 1.6,
  lineHeightMd: 1.4,
  lineHeightSm: 1.2,
};

export const SIZES = {
  // Base spacing unit
  base: 8,
  
  // Padding & Margin
  xs: 4,
  sm: 8,
  md: 16,
  lg: 24,
  xl: 32,
  xxl: 48,
  xxxl: 64,
  
  // Border Radius
  radiusXs: 4,
  radiusSm: 8,
  radiusMd: 12,
  radiusLg: 16,
  radiusXl: 20,
  radiusXxl: 24,
  radiusFull: 9999,
  
  // Icon Sizes
  iconXs: 14,
  iconSm: 18,
  iconMd: 24,
  iconLg: 32,
  iconXl: 48,
  iconXxl: 64,
  
  // Component Heights
  buttonHeight: 52,
  buttonHeightSm: 40,
  buttonHeightLg: 56,
  inputHeight: 52,
  headerHeight: 60,
  tabBarHeight: 80,
  cardMinHeight: 80,
  
  // Screen
  screenPadding: 16,
  contentMaxWidth: 600,
};

export const SHADOWS = {
  none: {
    shadowColor: 'transparent',
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0,
    shadowRadius: 0,
    elevation: 0,
  },
  small: {
    shadowColor: '#0F172A',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 3,
    elevation: 2,
  },
  medium: {
    shadowColor: '#0F172A',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.08,
    shadowRadius: 8,
    elevation: 4,
  },
  large: {
    shadowColor: '#0F172A',
    shadowOffset: { width: 0, height: 8 },
    shadowOpacity: 0.12,
    shadowRadius: 16,
    elevation: 8,
  },
  xlarge: {
    shadowColor: '#0F172A',
    shadowOffset: { width: 0, height: 12 },
    shadowOpacity: 0.15,
    shadowRadius: 24,
    elevation: 12,
  },
  colored: (color: string) => ({
    shadowColor: color,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 8,
    elevation: 4,
  }),
};

// Animation Durations
export const ANIMATIONS = {
  fast: 150,
  normal: 300,
  slow: 500,
};

// Spacing helper
export const spacing = (multiplier: number) => SIZES.base * multiplier;

export default { COLORS, FONTS, SIZES, SHADOWS, ANIMATIONS, spacing };
