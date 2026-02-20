# Mobile layout guide – consistent screens on all devices

Use these so every screen looks correct on any screen size (small phones, large phones, tablets) and safe area (notch, home indicator).

## 1. ScrollView content

For every main vertical `ScrollView`:

- Set `contentContainerStyle` with **`flexGrow: 1`** so content can extend and scroll.
- Add **`paddingBottom: SIZES.xxl`** (or 24+) so the last items aren’t hidden behind tab bar or cut off.
- Use **`keyboardShouldPersistTaps="handled"`** so taps work when the keyboard is open.

Example:

```ts
contentContainerStyle={[styles.scrollContent]}
// In StyleSheet:
scrollContent: { flexGrow: 1, paddingBottom: SIZES.xxl },
```

## 2. Responsive sizes

Import from theme:

```ts
import { COLORS, SIZES, scaleWidth, scaleHeight, moderateScale, horizontalPadding } from '../../constants/theme';
```

- **Fixed widths (cards, icons):** use `scaleWidth(value)` so they scale with screen width.
- **Heights / vertical spacing:** use `scaleHeight(value)` if needed.
- **Fonts / icons (avoid too big on tablets):** use `moderateScale(value)`.
- **Horizontal padding:** use `horizontalPadding()` for side padding (12 on very small, 16 otherwise).

Examples:

- Card width: `width: scaleWidth(160)` instead of `160`.
- Logo: `width: moderateScale(100), height: moderateScale(100)`.
- Screen padding: `paddingHorizontal: horizontalPadding()`.

## 3. Safe area

- Keep using **`SafeAreaView`** from `react-native-safe-area-context` for the screen root so content stays below notch and above home indicator.
- `App.tsx` already wraps the app in `SafeAreaProvider`.

## 4. Optional: ScreenWrapper

For new screens you can use:

```ts
import { ScreenWrapper } from '../../components/common';

<ScreenWrapper scroll keyboardAvoiding={false} edges={['top']}>
  {/* content */}
</ScreenWrapper>
```

- `scroll={true}`: wraps content in `ScrollView` with `flexGrow` and bottom padding.
- `keyboardAvoiding`: wrap in `KeyboardAvoidingView` for forms.
- `edges`: which safe area edges to apply (e.g. `['top']` when using a bottom tab bar).

## 5. Avoid

- **Fixed widths** for cards/list items on small screens: use `scaleWidth()` or percentage.
- **ScrollView without `flexGrow`**: content can be cut off on short screens.
- **No bottom padding**: last content can sit under the tab bar or be unscrollable.

## 6. Base reference

Responsive scaling uses base width **375** and height **812**. Very small devices get slightly reduced horizontal padding via `horizontalPadding()`.
