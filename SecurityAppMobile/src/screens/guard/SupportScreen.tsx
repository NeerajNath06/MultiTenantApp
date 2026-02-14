import React from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Linking, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';

interface SupportOption {
  id: string;
  title: string;
  subtitle: string;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
  color: string;
  action: () => void;
}

function SupportScreen({ navigation }: any) {
  const supportOptions: SupportOption[] = [
    {
      id: '1',
      title: 'Call Support',
      subtitle: 'Talk to our support team',
      icon: 'phone',
      color: COLORS.success,
      action: () => {
        Linking.openURL('tel:+911234567890').catch(() => {
          Alert.alert('Error', 'Unable to make phone call');
        });
      }
    },
    {
      id: '2',
      title: 'Live Chat',
      subtitle: 'Chat with us in real-time',
      icon: 'chat',
      color: COLORS.primary,
      action: () => navigation.navigate('LiveChat')
    },
    {
      id: '3',
      title: 'Email Support',
      subtitle: 'Send us an email',
      icon: 'email',
      color: COLORS.info,
      action: () => {
        Linking.openURL('mailto:support@securityapp.com').catch(() => {
          Alert.alert('Error', 'Unable to open email client');
        });
      }
    },
    {
      id: '4',
      title: 'FAQ',
      subtitle: 'Find answers to common questions',
      icon: 'help-circle',
      color: COLORS.warning,
      action: () => navigation.navigate('FAQ')
    },
    {
      id: '5',
      title: 'Report Issue',
      subtitle: 'Report a bug or problem',
      icon: 'bug',
      color: COLORS.error,
      action: () => navigation.navigate('ReportIssue')
    },
    {
      id: '6',
      title: 'Send Feedback',
      subtitle: 'Share your thoughts with us',
      icon: 'message-text',
      color: COLORS.secondary,
      action: () => navigation.navigate('Feedback')
    },
  ];

  const handleEmergencyContact = () => {
    Alert.alert(
      'Emergency Contact',
      'Are you sure you want to call the emergency hotline?',
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Call', onPress: () => Linking.openURL('tel:+911234567890') }
      ]
    );
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Help & Support</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* Support Header */}
        <View style={styles.supportHeader}>
          <View style={styles.supportIcon}>
            <MaterialCommunityIcons name="headset" size={48} color={COLORS.primary} />
          </View>
          <Text style={styles.supportTitle}>How can we help you?</Text>
          <Text style={styles.supportSubtitle}>
            Choose from the options below to get assistance
          </Text>
        </View>

        {/* Support Options */}
        <View style={styles.optionsGrid}>
          {supportOptions.map((option) => (
            <TouchableOpacity
              key={option.id}
              style={styles.optionCard}
              onPress={option.action}
            >
              <View style={[styles.optionIcon, { backgroundColor: option.color + '15' }]}>
                <MaterialCommunityIcons name={option.icon} size={28} color={option.color} />
              </View>
              <Text style={styles.optionTitle}>{option.title}</Text>
              <Text style={styles.optionSubtitle}>{option.subtitle}</Text>
            </TouchableOpacity>
          ))}
        </View>

        {/* Emergency Contact */}
        <Card style={styles.emergencyCard}>
          <View style={styles.emergencyContent}>
            <View style={styles.emergencyIcon}>
              <MaterialCommunityIcons name="phone-alert" size={32} color={COLORS.error} />
            </View>
            <View style={styles.emergencyText}>
              <Text style={styles.emergencyTitle}>Emergency Hotline</Text>
              <Text style={styles.emergencySubtitle}>Available 24/7 for urgent issues</Text>
            </View>
          </View>
          <TouchableOpacity style={styles.emergencyButton} onPress={handleEmergencyContact}>
            <MaterialCommunityIcons name="phone" size={20} color={COLORS.white} />
            <Text style={styles.emergencyButtonText}>Call Now</Text>
          </TouchableOpacity>
        </Card>

        {/* Quick Links */}
        <View style={styles.quickLinks}>
          <Text style={styles.quickLinksTitle}>Quick Links</Text>
          <TouchableOpacity style={styles.quickLink} onPress={() => navigation.navigate('ChangePassword')}>
            <MaterialCommunityIcons name="lock-reset" size={20} color={COLORS.primary} />
            <Text style={styles.quickLinkText}>Change Password</Text>
            <MaterialCommunityIcons name="chevron-right" size={20} color={COLORS.gray400} />
          </TouchableOpacity>
          <TouchableOpacity style={styles.quickLink} onPress={() => navigation.navigate('NotificationSettings')}>
            <MaterialCommunityIcons name="bell-cog" size={20} color={COLORS.primary} />
            <Text style={styles.quickLinkText}>Notification Settings</Text>
            <MaterialCommunityIcons name="chevron-right" size={20} color={COLORS.gray400} />
          </TouchableOpacity>
          <TouchableOpacity style={styles.quickLink} onPress={() => navigation.navigate('EditProfile')}>
            <MaterialCommunityIcons name="account-edit" size={20} color={COLORS.primary} />
            <Text style={styles.quickLinkText}>Edit Profile</Text>
            <MaterialCommunityIcons name="chevron-right" size={20} color={COLORS.gray400} />
          </TouchableOpacity>
        </View>

        {/* App Info */}
        <View style={styles.appInfo}>
          <Text style={styles.appInfoText}>Security App v1.0.0</Text>
          <Text style={styles.appInfoSubtext}>© 2024 Navya Cloud Solutions</Text>
        </View>
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
  supportHeader: { alignItems: 'center', marginBottom: SIZES.xl },
  supportIcon: { width: 80, height: 80, borderRadius: 40, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center', marginBottom: SIZES.md },
  supportTitle: { fontSize: FONTS.h3, fontWeight: 'bold', color: COLORS.textPrimary, marginBottom: SIZES.xs },
  supportSubtitle: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, textAlign: 'center' },
  optionsGrid: { flexDirection: 'row', flexWrap: 'wrap', justifyContent: 'space-between', marginBottom: SIZES.lg },
  optionCard: { width: '48%', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, alignItems: 'center', ...SHADOWS.small },
  optionIcon: { width: 56, height: 56, borderRadius: 28, justifyContent: 'center', alignItems: 'center', marginBottom: SIZES.sm },
  optionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.xs, textAlign: 'center' },
  optionSubtitle: { fontSize: FONTS.caption, color: COLORS.textSecondary, textAlign: 'center' },
  emergencyCard: { backgroundColor: COLORS.error + '08', borderWidth: 1, borderColor: COLORS.error + '20', padding: SIZES.md, marginBottom: SIZES.lg },
  emergencyContent: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.md },
  emergencyIcon: { width: 56, height: 56, borderRadius: 28, backgroundColor: COLORS.error + '15', justifyContent: 'center', alignItems: 'center', marginRight: SIZES.sm },
  emergencyText: { flex: 1 },
  emergencyTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.error, marginBottom: SIZES.xs },
  emergencySubtitle: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  emergencyButton: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.error, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusMd, gap: SIZES.xs },
  emergencyButtonText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.white },
  quickLinks: { marginBottom: SIZES.lg },
  quickLinksTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  quickLink: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, padding: SIZES.md, borderRadius: SIZES.radiusMd, marginBottom: SIZES.xs, ...SHADOWS.small },
  quickLinkText: { flex: 1, fontSize: FONTS.body, color: COLORS.textPrimary, marginLeft: SIZES.sm },
  appInfo: { alignItems: 'center', marginBottom: SIZES.xxl },
  appInfoText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  appInfoSubtext: { fontSize: FONTS.caption, color: COLORS.gray400, marginTop: SIZES.xs },
});

export default SupportScreen;
