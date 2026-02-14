import React from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';

function PrivacyScreen({ navigation }: any) {
  const sections = [
    {
      title: 'Information We Collect',
      content: `We collect information you provide directly to us, including:

• Personal information (name, email, phone number)
• Employment details (employee ID, designation)
• Location data (for attendance and patrol tracking)
• Photos (for check-in verification)
• Device information (device ID, OS version)

This information is essential for providing our security management services.`
    },
    {
      title: 'How We Use Your Information',
      content: `Your information is used to:

• Verify your identity and attendance
• Track patrol routes and checkpoints
• Process incident reports
• Manage leave requests and schedules
• Send important notifications
• Improve our services
• Comply with legal requirements`
    },
    {
      title: 'Data Storage & Security',
      content: `We implement industry-standard security measures to protect your data:

• Encrypted data transmission (SSL/TLS)
• Secure cloud storage with access controls
• Regular security audits
• Limited access to authorized personnel only
• Automatic data backup systems

Your data is stored in secure servers located in India.`
    },
    {
      title: 'Location Tracking',
      content: `Location tracking is used for:

• Verifying check-in/check-out locations
• Tracking patrol routes in real-time
• Emergency SOS location sharing
• Geofencing alerts

Location data is only collected during active duty hours and when the app is in use. You can disable location tracking in settings, but some features may not work.`
    },
    {
      title: 'Data Sharing',
      content: `We do not sell your personal information. We may share data with:

• Your employer/security agency
• Supervisors and authorized personnel
• Emergency services (when SOS is activated)
• Service providers who assist our operations
• Legal authorities when required by law

All third parties are bound by confidentiality agreements.`
    },
    {
      title: 'Your Rights',
      content: `You have the right to:

• Access your personal data
• Request correction of inaccurate data
• Request deletion of your data (subject to legal retention requirements)
• Opt-out of non-essential data collection
• Withdraw consent for data processing

Contact us to exercise these rights.`
    },
    {
      title: 'Data Retention',
      content: `We retain your data for:

• Active employment: Full data access
• Post-employment: Essential records for 7 years (legal requirement)
• Location data: 90 days
• Incident reports: 5 years

You can request data deletion after the mandatory retention period.`
    },
    {
      title: 'Updates to Policy',
      content: `We may update this policy periodically. We will notify you of significant changes through:

• In-app notifications
• Email communications
• Announcement section

Continued use of the app after updates constitutes acceptance of the new policy.`
    }
  ];

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Privacy Policy</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        <View style={styles.intro}>
          <MaterialCommunityIcons name="shield-account" size={48} color={COLORS.primary} />
          <Text style={styles.introTitle}>Your Privacy Matters</Text>
          <Text style={styles.introText}>
            Last updated: December 15, 2024
          </Text>
        </View>

        <Text style={styles.description}>
          Security Guard App ("we", "our", or "us") is committed to protecting your privacy. This policy explains how we collect, use, and safeguard your information.
        </Text>

        {sections.map((section, index) => (
          <View key={index} style={styles.section}>
            <Text style={styles.sectionTitle}>{section.title}</Text>
            <Text style={styles.sectionContent}>{section.content}</Text>
          </View>
        ))}

        <View style={styles.contact}>
          <Text style={styles.contactTitle}>Questions?</Text>
          <Text style={styles.contactText}>
            Contact our Data Protection Officer:{'\n'}
            Email: privacy@navyacloud.com{'\n'}
            Phone: +91 1800 123 4567
          </Text>
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
  intro: { alignItems: 'center', paddingVertical: SIZES.lg, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, marginBottom: SIZES.md, ...SHADOWS.small },
  introTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginTop: SIZES.sm },
  introText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 4 },
  description: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, lineHeight: 22, marginBottom: SIZES.lg },
  section: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.md, ...SHADOWS.small },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.primary, marginBottom: SIZES.sm },
  sectionContent: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, lineHeight: 22 },
  contact: { backgroundColor: COLORS.primary + '10', borderRadius: SIZES.radiusMd, padding: SIZES.lg, marginTop: SIZES.md, marginBottom: SIZES.xxl },
  contactTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.primary, marginBottom: SIZES.sm },
  contactText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, lineHeight: 22 },
});

export default PrivacyScreen;
