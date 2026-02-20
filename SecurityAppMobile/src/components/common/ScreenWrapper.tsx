import React from 'react';
import {
  View,
  StyleSheet,
  ScrollView,
  KeyboardAvoidingView,
  Platform,
  ViewStyle,
  ScrollViewProps,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { horizontalPadding } from '../../utils/responsive';

interface ScreenWrapperProps {
  children: React.ReactNode;
  /** Use ScrollView so content scrolls on small screens; default true */
  scroll?: boolean;
  /** KeyboardAvoidingView when scroll is true and keyboardAvoiding is true */
  keyboardAvoiding?: boolean;
  /** Edges for SafeAreaView; default ['top'] so bottom can be used by tab bar */
  edges?: ('top' | 'bottom' | 'left' | 'right')[];
  /** Style for outer SafeAreaView */
  style?: ViewStyle;
  /** Content container style (for ScrollView contentContainerStyle) */
  contentContainerStyle?: ViewStyle;
  /** ScrollView props when scroll=true */
  scrollViewProps?: ScrollViewProps;
}

/**
 * Wraps screen content with SafeAreaView and optional ScrollView so layout
 * stays correct on any screen size and safe area (notch, home indicator).
 */
const ScreenWrapper: React.FC<ScreenWrapperProps> = ({
  children,
  scroll = true,
  keyboardAvoiding = false,
  edges = ['top'],
  style,
  contentContainerStyle,
  scrollViewProps = {},
}) => {
  const content = scroll ? (
    <ScrollView
      style={styles.scroll}
      contentContainerStyle={[
        styles.scrollContent,
        contentContainerStyle,
      ]}
      showsVerticalScrollIndicator={false}
      keyboardShouldPersistTaps="handled"
      {...scrollViewProps}
    >
      {children}
    </ScrollView>
  ) : (
    <View style={[styles.content, contentContainerStyle]}>{children}</View>
  );

  const wrapped = keyboardAvoiding && scroll ? (
    <KeyboardAvoidingView
      style={styles.keyboardView}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      keyboardVerticalOffset={Platform.OS === 'ios' ? 0 : 0}
    >
      {content}
    </KeyboardAvoidingView>
  ) : (
    content
  );

  return (
    <SafeAreaView style={[styles.safe, style]} edges={edges}>
      {wrapped}
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  safe: {
    flex: 1,
  },
  keyboardView: {
    flex: 1,
  },
  scroll: {
    flex: 1,
  },
  scrollContent: {
    flexGrow: 1,
    paddingHorizontal: horizontalPadding(),
    paddingBottom: 24,
  },
  content: {
    flex: 1,
    paddingHorizontal: horizontalPadding(),
  },
});

export default ScreenWrapper;
