import React from 'react';
import { View, StyleSheet, TouchableOpacity, StyleProp, ViewStyle } from 'react-native';
import { COLORS, SIZES, SHADOWS } from '../../constants/theme';
import { CardProps } from '../../types/components';

const Card: React.FC<CardProps> = ({ children, onPress, style, shadow = 'medium' }) => {
  const getShadow = (): ViewStyle => {
    switch (shadow) {
      case 'small':
        return SHADOWS.small;
      case 'large':
        return SHADOWS.large;
      case 'none':
        return {};
      default:
        return SHADOWS.medium;
    }
  };

  if (onPress) {
    return (
      <TouchableOpacity
        onPress={onPress}
        activeOpacity={0.8}
        style={[styles.card, getShadow(), style]}
      >
        {children}
      </TouchableOpacity>
    );
  }

  return <View style={[styles.card, getShadow(), style]}>{children}</View>;
};

const styles = StyleSheet.create({
  card: {
    backgroundColor: COLORS.cardBg,
    borderRadius: SIZES.radiusLg,
    padding: SIZES.md,
  },
});

export default Card;
