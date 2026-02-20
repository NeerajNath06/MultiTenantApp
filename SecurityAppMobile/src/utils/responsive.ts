import { Dimensions, ScaledSize } from 'react-native';

const { width: WINDOW_WIDTH, height: WINDOW_HEIGHT } = Dimensions.get('window');

// Base dimensions (e.g. design reference: iPhone 14 / common Android)
const BASE_WIDTH = 375;
const BASE_HEIGHT = 812;

/**
 * Scale value by screen width (use for horizontal spacing, widths, font sizes)
 */
export function scaleWidth(value: number): number {
  const scale = WINDOW_WIDTH / BASE_WIDTH;
  return Math.round(value * scale);
}

/**
 * Scale value by screen height (use for vertical spacing, heights)
 */
export function scaleHeight(value: number): number {
  const scale = WINDOW_HEIGHT / BASE_HEIGHT;
  return Math.round(value * scale);
}

/**
 * Moderate scale - less aggressive scaling for fonts/sizes (avoids too big on tablets)
 */
export function moderateScale(value: number, factor: number = 0.5): number {
  const scale = WINDOW_WIDTH / BASE_WIDTH;
  const moderated = 1 + (scale - 1) * factor;
  return Math.round(value * moderated);
}

/**
 * Get current window dimensions (updates on rotation/resize)
 */
export function getWindowDimensions(): ScaledSize {
  return Dimensions.get('window');
}

export const wp = (percent: number): number => (WINDOW_WIDTH * percent) / 100;
export const hp = (percent: number): number => (WINDOW_HEIGHT * percent) / 100;

export const isSmallScreen = WINDOW_WIDTH < 360 || WINDOW_HEIGHT < 640;
export const isLargeScreen = WINDOW_WIDTH >= 414 || WINDOW_HEIGHT >= 896;
export const windowWidth = WINDOW_WIDTH;
export const windowHeight = WINDOW_HEIGHT;

/**
 * Responsive horizontal padding (same on all screens but scales on very small)
 */
export function horizontalPadding(): number {
  if (WINDOW_WIDTH < 360) return 12;
  return 16;
}

/**
 * Minimum touch target size (44pt recommended by Apple)
 */
export const MIN_TOUCH_SIZE = 44;

export default {
  scaleWidth,
  scaleHeight,
  moderateScale,
  getWindowDimensions,
  wp,
  hp,
  isSmallScreen,
  isLargeScreen,
  windowWidth,
  windowHeight,
  horizontalPadding,
  MIN_TOUCH_SIZE,
  BASE_WIDTH,
  BASE_HEIGHT,
};
