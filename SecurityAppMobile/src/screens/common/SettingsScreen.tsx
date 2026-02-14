import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Switch, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';

interface SettingItem {
  id: string;
  title: string;
  subtitle?: string;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
  type: 'toggle' | 'navigate' | 'action';
  value?: boolean;
  route?: string;
  color?: string;
}

function SettingsScreen({ navigation }: any) {
  const [settings, setSettings] = useState({
    notifications: true,
    biometric: true,
    darkMode: false,
    locationTracking: true,
    autoSync: true,
    soundEffects: true,
    vibration: true,
    dataUsage: false,
  });

  const toggleSetting = (key: string) => {
    setSettings(prev => ({ ...prev, [key]: !prev[key as keyof typeof prev] }));
  };

  const generalSettings: SettingItem[] = [
    { id: 'language', title: 'Language', subtitle: 'English', icon: 'translate', type: 'navigate', route: 'LanguageSelection', color: COLORS.primaryBlue },
    { id: 'notifications', title: 'Push Notifications', subtitle: 'Manage notification preferences', icon: 'bell-outline', type: 'navigate', route: 'NotificationSettings', color: COLORS.warning },
    { id: 'darkMode', title: 'Dark Mode', subtitle: settings.darkMode ? 'On' : 'Off', icon: 'theme-light-dark', type: 'toggle', value: settings.darkMode, color: COLORS.gray700 },
  ];

  const securitySettings: SettingItem[] = [
    { id: 'changePassword', title: 'Change Password', subtitle: 'Update your password', icon: 'lock-outline', type: 'navigate', route: 'ChangePassword', color: COLORS.secondary },
    { id: 'biometric', title: 'Biometric Login', subtitle: 'Face ID / Fingerprint', icon: 'fingerprint', type: 'toggle', value: settings.biometric, color: COLORS.success },
    { id: 'locationTracking', title: 'Location Tracking', subtitle: 'Required for check-in', icon: 'map-marker-outline', type: 'toggle', value: settings.locationTracking, color: COLORS.error },
  ];

  const appSettings: SettingItem[] = [
    { id: 'autoSync', title: 'Auto Sync', subtitle: 'Sync data automatically', icon: 'sync', type: 'toggle', value: settings.autoSync, color: COLORS.info },
    { id: 'soundEffects', title: 'Sound Effects', subtitle: 'Button sounds', icon: 'volume-high', type: 'toggle', value: settings.soundEffects, color: COLORS.primaryBlue },
    { id: 'vibration', title: 'Vibration', subtitle: 'Haptic feedback', icon: 'vibrate', type: 'toggle', value: settings.vibration, color: COLORS.warning },
    { id: 'dataUsage', title: 'Data Saver', subtitle: 'Reduce data usage', icon: 'cellphone-arrow-down', type: 'toggle', value: settings.dataUsage, color: COLORS.success },
  ];

  const supportSettings: SettingItem[] = [
    { id: 'help', title: 'Help & Support', subtitle: 'FAQs and contact', icon: 'help-circle-outline', type: 'navigate', route: 'Support', color: COLORS.info },
    { id: 'privacy', title: 'Privacy Policy', icon: 'shield-account-outline', type: 'navigate', route: 'Privacy', color: COLORS.secondary },
    { id: 'terms', title: 'Terms of Service', icon: 'file-document-outline', type: 'navigate', route: 'Terms', color: COLORS.gray600 },
    { id: 'about', title: 'About App', subtitle: 'Version 1.0.0', icon: 'information-outline', type: 'navigate', route: 'About', color: COLORS.primaryBlue },
  ];

  const handleClearCache = () => {
    Alert.alert(
      'Clear Cache',
      'This will clear all cached data. Are you sure?',
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Clear', style: 'destructive', onPress: () => Alert.alert('Success', 'Cache cleared successfully') }
      ]
    );
  };

  const handleDeleteAccount = () => {
    Alert.alert(
      'Delete Account',
      'This action is permanent and cannot be undone. All your data will be deleted.',
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Delete', style: 'destructive', onPress: () => Alert.alert('Contact Support', 'Please contact support to delete your account.') }
      ]
    );
  };

  const renderSettingItem = (item: SettingItem) => {
    const handlePress = () => {
      if (item.type === 'navigate' && item.route) {
        navigation.navigate(item.route);
      } else if (item.type === 'toggle') {
        toggleSetting(item.id);
      }
    };

    return (
      <TouchableOpacity
        key={item.id}
        style={styles.settingItem}
        onPress={handlePress}
        activeOpacity={item.type === 'toggle' ? 1 : 0.7}
      >
        <View style={[styles.settingIcon, { backgroundColor: (item.color || COLORS.primary) + '15' }]}>
          <MaterialCommunityIcons name={item.icon} size={22} color={item.color || COLORS.primary} />
        </View>
        <View style={styles.settingContent}>
          <Text style={styles.settingTitle}>{item.title}</Text>
          {item.subtitle && <Text style={styles.settingSubtitle}>{item.subtitle}</Text>}
        </View>
        {item.type === 'toggle' ? (
          <Switch
            value={item.value}
            onValueChange={() => toggleSetting(item.id)}
            trackColor={{ false: COLORS.gray300, true: COLORS.primary + '50' }}
            thumbColor={item.value ? COLORS.primary : COLORS.gray400}
          />
        ) : (
          <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.gray400} />
        )}
      </TouchableOpacity>
    );
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Settings</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* General */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>General</Text>
          <View style={styles.settingsCard}>
            {generalSettings.map(renderSettingItem)}
          </View>
        </View>

        {/* Security */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Security & Privacy</Text>
          <View style={styles.settingsCard}>
            {securitySettings.map(renderSettingItem)}
          </View>
        </View>

        {/* App Settings */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>App Preferences</Text>
          <View style={styles.settingsCard}>
            {appSettings.map(renderSettingItem)}
          </View>
        </View>

        {/* Support */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Support & Info</Text>
          <View style={styles.settingsCard}>
            {supportSettings.map(renderSettingItem)}
          </View>
        </View>

        {/* Danger Zone */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Data Management</Text>
          <View style={styles.settingsCard}>
            <TouchableOpacity style={styles.settingItem} onPress={handleClearCache}>
              <View style={[styles.settingIcon, { backgroundColor: COLORS.warning + '15' }]}>
                <MaterialCommunityIcons name="broom" size={22} color={COLORS.warning} />
              </View>
              <View style={styles.settingContent}>
                <Text style={styles.settingTitle}>Clear Cache</Text>
                <Text style={styles.settingSubtitle}>Free up storage space</Text>
              </View>
              <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.gray400} />
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.settingItem} onPress={handleDeleteAccount}>
              <View style={[styles.settingIcon, { backgroundColor: COLORS.error + '15' }]}>
                <MaterialCommunityIcons name="account-remove" size={22} color={COLORS.error} />
              </View>
              <View style={styles.settingContent}>
                <Text style={[styles.settingTitle, { color: COLORS.error }]}>Delete Account</Text>
                <Text style={styles.settingSubtitle}>Permanently delete your account</Text>
              </View>
              <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.gray400} />
            </TouchableOpacity>
          </View>
        </View>

        {/* App Info */}
        <View style={styles.appInfo}>
          <Text style={styles.appName}>Security Guard App</Text>
          <Text style={styles.appVersion}>Version 1.0.0 (Build 100)</Text>
          <Text style={styles.copyright}>© 2024 Navya Cloud Solutions</Text>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  content: { padding: SIZES.md },
  section: { marginBottom: SIZES.lg },
  sectionTitle: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.textSecondary, textTransform: 'uppercase', letterSpacing: 0.5, marginBottom: SIZES.sm, marginLeft: SIZES.xs },
  settingsCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, ...SHADOWS.small, overflow: 'hidden' },
  settingItem: { flexDirection: 'row', alignItems: 'center', padding: SIZES.md, borderBottomWidth: 1, borderBottomColor: COLORS.gray100 },
  settingIcon: { width: 40, height: 40, borderRadius: 10, justifyContent: 'center', alignItems: 'center' },
  settingContent: { flex: 1, marginLeft: SIZES.sm },
  settingTitle: { fontSize: FONTS.body, fontWeight: '500', color: COLORS.textPrimary },
  settingSubtitle: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  appInfo: { alignItems: 'center', paddingVertical: SIZES.xl },
  appName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  appVersion: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 4 },
  copyright: { fontSize: FONTS.tiny, color: COLORS.gray400, marginTop: 8 },
});

export default SettingsScreen;
