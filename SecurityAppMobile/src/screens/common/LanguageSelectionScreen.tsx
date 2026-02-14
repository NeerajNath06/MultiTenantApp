import React, { useState } from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { LanguageSelectionScreenProps } from '../../types/navigation';

interface Language {
  code: string;
  name: string;
  nativeName: string;
  flag: string;
}

const languages: Language[] = [
  { code: 'en', name: 'English', nativeName: 'English', flag: '🇺🇸' },
  { code: 'hi', name: 'Hindi', nativeName: 'हिन्दी', flag: '🇮🇳' },
  { code: 'es', name: 'Spanish', nativeName: 'Español', flag: '🇪🇸' },
  { code: 'fr', name: 'French', nativeName: 'Français', flag: '🇫🇷' },
  { code: 'de', name: 'German', nativeName: 'Deutsch', flag: '🇩🇪' },
  { code: 'zh', name: 'Chinese', nativeName: '中文', flag: '🇨🇳' },
];

const LanguageSelectionScreen: React.FC<LanguageSelectionScreenProps> = ({ navigation }) => {
  const [selectedLanguage, setSelectedLanguage] = useState<string>('en');

  const handleLanguageSelect = (languageCode: string): void => {
    setSelectedLanguage(languageCode);
  };

  const handleContinue = (): void => {
    // Save language preference and navigate
    navigation.goBack();
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Select Language</Text>
        <View style={styles.placeholder} />
      </View>

      <View style={styles.content}>
        <Text style={styles.subtitle}>Choose your preferred language</Text>
        
        <View style={styles.languageList}>
          {languages.map((language) => (
            <TouchableOpacity
              key={language.code}
              style={[
                styles.languageCard,
                selectedLanguage === language.code && styles.selectedLanguageCard
              ]}
              onPress={() => handleLanguageSelect(language.code)}
            >
              <View style={styles.languageInfo}>
                <Text style={styles.flag}>{language.flag}</Text>
                <View style={styles.languageNames}>
                  <Text style={[
                    styles.languageName,
                    selectedLanguage === language.code && styles.selectedLanguageName
                  ]}>
                    {language.name}
                  </Text>
                  <Text style={[
                    styles.nativeName,
                    selectedLanguage === language.code && styles.selectedNativeName
                  ]}>
                    {language.nativeName}
                  </Text>
                </View>
              </View>
              {selectedLanguage === language.code && (
                <MaterialCommunityIcons name="check-circle" size={24} color={COLORS.primary} />
              )}
            </TouchableOpacity>
          ))}
        </View>

        <TouchableOpacity style={styles.continueButton} onPress={handleContinue}>
          <Text style={styles.continueButtonText}>Continue</Text>
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
  },
  subtitle: {
    fontSize: FONTS.body,
    color: COLORS.textSecondary,
    textAlign: 'center',
    marginBottom: SIZES.xl,
  },
  languageList: {
    marginBottom: SIZES.xl,
  },
  languageCard: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    backgroundColor: COLORS.white,
    padding: SIZES.md,
    marginBottom: SIZES.sm,
    borderRadius: SIZES.radiusMd,
    borderWidth: 2,
    borderColor: 'transparent',
    ...SHADOWS.small,
  },
  selectedLanguageCard: {
    borderColor: COLORS.primary,
    backgroundColor: COLORS.primary + '05',
  },
  languageInfo: {
    flexDirection: 'row',
    alignItems: 'center',
    flex: 1,
  },
  flag: {
    fontSize: 32,
    marginRight: SIZES.md,
  },
  languageNames: {
    flex: 1,
  },
  languageName: {
    fontSize: FONTS.body,
    fontWeight: '500',
    color: COLORS.textPrimary,
  },
  nativeName: {
    fontSize: FONTS.bodySmall,
    color: COLORS.textSecondary,
    marginTop: 2,
  },
  selectedLanguageName: {
    color: COLORS.primary,
  },
  selectedNativeName: {
    color: COLORS.primary,
  },
  continueButton: {
    backgroundColor: COLORS.primary,
    paddingVertical: SIZES.md,
    borderRadius: SIZES.radiusMd,
    alignItems: 'center',
  },
  continueButtonText: {
    fontSize: FONTS.body,
    fontWeight: '600',
    color: COLORS.white,
  },
});

export default LanguageSelectionScreen;
