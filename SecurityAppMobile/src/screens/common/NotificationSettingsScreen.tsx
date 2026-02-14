import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Switch, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Button from '../../components/common/Button';

interface NotificationSetting {
  id: string;
  title: string;
  description: string;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
  enabled: boolean;
}

function NotificationSettingsScreen({ navigation }: any) {
  const [loading, setLoading] = useState(false);
  const [settings, setSettings] = useState<NotificationSetting[]>([
    { id: '1', title: 'Shift Reminders', description: 'Get notified before your shift starts', icon: 'clock-alert', enabled: true },
    { id: '2', title: 'Check-in Alerts', description: 'Reminders to check-in on time', icon: 'login', enabled: true },
    { id: '3', title: 'Check-out Alerts', description: 'Reminders to check-out', icon: 'logout', enabled: true },
    { id: '4', title: 'Emergency Alerts', description: 'Critical security notifications', icon: 'alert', enabled: true },
    { id: '5', title: 'Duty Updates', description: 'New duty assignments and changes', icon: 'clipboard-list', enabled: true },
    { id: '6', title: 'Incident Reports', description: 'Updates on reported incidents', icon: 'file-document', enabled: false },
    { id: '7', title: 'Attendance Summary', description: 'Daily attendance summary', icon: 'chart-bar', enabled: false },
    { id: '8', title: 'System Updates', description: 'App updates and maintenance', icon: 'cog', enabled: true },
    { id: '9', title: 'Promotional', description: 'News and offers', icon: 'bullhorn', enabled: false },
  ]);

  const [quietHours, setQuietHours] = useState({
    enabled: false,
    startTime: '22:00',
    endTime: '07:00',
  });

  const toggleSetting = (id: string) => {
    setSettings(settings.map(s => s.id === id ? { ...s, enabled: !s.enabled } : s));
  };

  const handleSave = () => {
    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      Alert.alert('Success', 'Notification settings saved successfully', [
        { text: 'OK', onPress: () => navigation.goBack() }
      ]);
    }, 1000);
  };

  const enableAll = () => {
    setSettings(settings.map(s => ({ ...s, enabled: true })));
  };

  const disableAll = () => {
    setSettings(settings.map(s => ({ ...s, enabled: false })));
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Notification Settings</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* Quick Actions */}
        <View style={styles.quickActions}>
          <TouchableOpacity style={styles.quickActionBtn} onPress={enableAll}>
            <Text style={styles.quickActionText}>Enable All</Text>
          </TouchableOpacity>
          <TouchableOpacity style={[styles.quickActionBtn, styles.quickActionBtnOutline]} onPress={disableAll}>
            <Text style={[styles.quickActionText, styles.quickActionTextOutline]}>Disable All</Text>
          </TouchableOpacity>
        </View>

        {/* Notification Settings */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Notification Types</Text>
          <View style={styles.settingsCard}>
            {settings.map((setting, index) => (
              <View key={setting.id} style={[styles.settingItem, index !== settings.length - 1 && styles.settingItemBorder]}>
                <View style={[styles.settingIcon, { backgroundColor: setting.enabled ? COLORS.primary + '15' : COLORS.gray200 }]}>
                  <MaterialCommunityIcons name={setting.icon} size={22} color={setting.enabled ? COLORS.primary : COLORS.gray400} />
                </View>
                <View style={styles.settingContent}>
                  <Text style={styles.settingTitle}>{setting.title}</Text>
                  <Text style={styles.settingDescription}>{setting.description}</Text>
                </View>
                <Switch
                  value={setting.enabled}
                  onValueChange={() => toggleSetting(setting.id)}
                  trackColor={{ false: COLORS.gray300, true: COLORS.primary + '50' }}
                  thumbColor={setting.enabled ? COLORS.primary : COLORS.gray400}
                />
              </View>
            ))}
          </View>
        </View>

        {/* Quiet Hours */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Quiet Hours</Text>
          <View style={styles.settingsCard}>
            <View style={styles.settingItem}>
              <View style={[styles.settingIcon, { backgroundColor: quietHours.enabled ? COLORS.secondary + '15' : COLORS.gray200 }]}>
                <MaterialCommunityIcons name="moon-waning-crescent" size={22} color={quietHours.enabled ? COLORS.secondary : COLORS.gray400} />
              </View>
              <View style={styles.settingContent}>
                <Text style={styles.settingTitle}>Enable Quiet Hours</Text>
                <Text style={styles.settingDescription}>Mute notifications during set hours</Text>
              </View>
              <Switch
                value={quietHours.enabled}
                onValueChange={(value) => setQuietHours({ ...quietHours, enabled: value })}
                trackColor={{ false: COLORS.gray300, true: COLORS.secondary + '50' }}
                thumbColor={quietHours.enabled ? COLORS.secondary : COLORS.gray400}
              />
            </View>
            {quietHours.enabled && (
              <View style={styles.timeContainer}>
                <View style={styles.timeItem}>
                  <Text style={styles.timeLabel}>From</Text>
                  <TouchableOpacity style={styles.timeButton}>
                    <MaterialCommunityIcons name="clock-outline" size={18} color={COLORS.primary} />
                    <Text style={styles.timeText}>{quietHours.startTime}</Text>
                  </TouchableOpacity>
                </View>
                <View style={styles.timeItem}>
                  <Text style={styles.timeLabel}>To</Text>
                  <TouchableOpacity style={styles.timeButton}>
                    <MaterialCommunityIcons name="clock-outline" size={18} color={COLORS.primary} />
                    <Text style={styles.timeText}>{quietHours.endTime}</Text>
                  </TouchableOpacity>
                </View>
              </View>
            )}
          </View>
        </View>

        {/* Sound & Vibration */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Sound & Vibration</Text>
          <View style={styles.settingsCard}>
            <TouchableOpacity style={styles.settingItem}>
              <View style={[styles.settingIcon, { backgroundColor: COLORS.info + '15' }]}>
                <MaterialCommunityIcons name="volume-high" size={22} color={COLORS.info} />
              </View>
              <View style={styles.settingContent}>
                <Text style={styles.settingTitle}>Notification Sound</Text>
                <Text style={styles.settingDescription}>Default Alert</Text>
              </View>
              <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.gray400} />
            </TouchableOpacity>
            <View style={[styles.settingItem, styles.settingItemBorder]}>
              <View style={[styles.settingIcon, { backgroundColor: COLORS.warning + '15' }]}>
                <MaterialCommunityIcons name="vibrate" size={22} color={COLORS.warning} />
              </View>
              <View style={styles.settingContent}>
                <Text style={styles.settingTitle}>Vibration</Text>
                <Text style={styles.settingDescription}>Vibrate for notifications</Text>
              </View>
              <Switch
                value={true}
                trackColor={{ false: COLORS.gray300, true: COLORS.warning + '50' }}
                thumbColor={COLORS.warning}
              />
            </View>
          </View>
        </View>

        <Button
          title="Save Settings"
          onPress={handleSave}
          loading={loading}
          style={styles.saveButton}
        />
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  content: { padding: SIZES.md },
  quickActions: { flexDirection: 'row', gap: SIZES.sm, marginBottom: SIZES.md },
  quickActionBtn: { flex: 1, backgroundColor: COLORS.primary, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusMd, alignItems: 'center' },
  quickActionBtnOutline: { backgroundColor: 'transparent', borderWidth: 1, borderColor: COLORS.primary },
  quickActionText: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.white },
  quickActionTextOutline: { color: COLORS.primary },
  section: { marginBottom: SIZES.lg },
  sectionTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  settingsCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, ...SHADOWS.small },
  settingItem: { flexDirection: 'row', alignItems: 'center', padding: SIZES.md },
  settingItemBorder: { borderTopWidth: 1, borderTopColor: COLORS.gray100 },
  settingIcon: { width: 44, height: 44, borderRadius: 22, justifyContent: 'center', alignItems: 'center', marginRight: SIZES.sm },
  settingContent: { flex: 1 },
  settingTitle: { fontSize: FONTS.body, fontWeight: '500', color: COLORS.textPrimary },
  settingDescription: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  timeContainer: { flexDirection: 'row', paddingHorizontal: SIZES.md, paddingBottom: SIZES.md, gap: SIZES.md },
  timeItem: { flex: 1 },
  timeLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginBottom: SIZES.xs },
  timeButton: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.gray100, paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusSm, gap: SIZES.xs },
  timeText: { fontSize: FONTS.body, color: COLORS.textPrimary },
  saveButton: { marginBottom: SIZES.xxl },
});

export default NotificationSettingsScreen;
