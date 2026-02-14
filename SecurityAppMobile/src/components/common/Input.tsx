import React, { useState } from 'react';
import { View, TextInput, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES } from '../../constants/theme';
import { InputProps } from '../../types/components';

const Input: React.FC<InputProps> = ({
  label,
  placeholder,
  value,
  onChangeText,
  secureTextEntry = false,
  keyboardType = 'default',
  autoCapitalize = 'none',
  error,
  icon,
  leftIcon,
  rightIcon,
  onRightIconPress,
  multiline = false,
  numberOfLines = 1,
  editable = true,
  style,
  inputStyle,
}) => {
  // Support both icon and leftIcon props
  const iconToUse = icon || leftIcon;
  const [isFocused, setIsFocused] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  return (
    <View style={[styles.container, style]}>
      {label && <Text style={styles.label}>{label}</Text>}
      <View
        style={[
          styles.inputContainer,
          isFocused && styles.focusedInput,
          error && styles.errorInput,
          !editable && styles.disabledInput,
          multiline && styles.multilineContainer,
        ]}
      >
        {iconToUse && (
          <MaterialCommunityIcons
            name={iconToUse as keyof typeof MaterialCommunityIcons.glyphMap}
            size={SIZES.iconMd}
            color={isFocused ? COLORS.primary : COLORS.gray400}
            style={[styles.leftIcon, multiline && styles.multilineIcon]}
          />
        )}
        <TextInput
          placeholder={placeholder}
          placeholderTextColor={COLORS.gray400}
          value={value}
          onChangeText={onChangeText}
          secureTextEntry={secureTextEntry && !showPassword}
          keyboardType={keyboardType}
          autoCapitalize={autoCapitalize}
          onFocus={() => setIsFocused(true)}
          onBlur={() => setIsFocused(false)}
          multiline={multiline}
          numberOfLines={numberOfLines}
          editable={editable}
          style={[
            styles.input,
            multiline && styles.multilineInput,
            inputStyle,
          ]}
        />
        {secureTextEntry && (
          <TouchableOpacity
            onPress={() => setShowPassword(!showPassword)}
            style={styles.rightIcon}
          >
            <MaterialCommunityIcons
              name={showPassword ? 'eye-off' : 'eye'}
              size={SIZES.iconMd}
              color={COLORS.gray400}
            />
          </TouchableOpacity>
        )}
        {rightIcon && onRightIconPress && (
          <TouchableOpacity onPress={onRightIconPress} style={styles.rightIcon}>
            <MaterialCommunityIcons 
              name={rightIcon as keyof typeof MaterialCommunityIcons.glyphMap} 
              size={SIZES.iconMd} 
              color={COLORS.gray400} 
            />
          </TouchableOpacity>
        )}
      </View>
      {error && <Text style={styles.errorText}>{error}</Text>}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    marginBottom: SIZES.md,
  },
  label: {
    fontSize: FONTS.bodySmall,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: SIZES.sm,
  },
  inputContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.gray100,
    borderRadius: SIZES.radiusMd,
    borderWidth: 1.5,
    borderColor: 'transparent',
    height: SIZES.inputHeight,
    paddingHorizontal: SIZES.md,
  },
  focusedInput: {
    borderColor: COLORS.primary,
    backgroundColor: COLORS.white,
  },
  errorInput: {
    borderColor: COLORS.error,
    backgroundColor: '#FEF2F2',
  },
  disabledInput: {
    backgroundColor: COLORS.gray200,
  },
  leftIcon: {
    marginRight: SIZES.sm,
  },
  input: {
    flex: 1,
    fontSize: FONTS.body,
    color: COLORS.textPrimary,
  },
  multilineContainer: {
    height: 'auto',
    minHeight: 100,
    alignItems: 'flex-start',
    paddingVertical: SIZES.sm,
  },
  multilineInput: {
    minHeight: 80,
    textAlignVertical: 'top',
    paddingTop: 0,
  },
  multilineIcon: {
    marginTop: SIZES.xs,
  },
  rightIcon: {
    marginLeft: SIZES.sm,
    padding: SIZES.xs,
  },
  errorText: {
    fontSize: FONTS.caption,
    color: COLORS.error,
    marginTop: SIZES.xs,
  },
});

export default Input;
