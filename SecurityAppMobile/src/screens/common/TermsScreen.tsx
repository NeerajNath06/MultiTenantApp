import React from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';

function TermsScreen({ navigation }: any) {
  const sections = [
    {
      title: '1. Acceptance of Terms',
      content: `By downloading, installing, or using the Security Guard App, you agree to be bound by these Terms of Service. If you do not agree to these terms, do not use the app.

These terms constitute a legally binding agreement between you and Navya Cloud and Software Solutions regarding your use of the application.`
    },
    {
      title: '2. User Eligibility',
      content: `To use this app, you must:

• Be employed as a security guard or supervisor
• Be authorized by your employer to use the app
• Be at least 18 years old
• Have a valid mobile device and internet connection
• Agree to provide accurate information`
    },
    {
      title: '3. Account Responsibilities',
      content: `You are responsible for:

• Maintaining the confidentiality of your login credentials
• All activities that occur under your account
• Reporting unauthorized access immediately
• Ensuring accurate check-in/check-out data
• Using the app only for authorized purposes

Do not share your credentials with anyone.`
    },
    {
      title: '4. Permitted Use',
      content: `You may use the app for:

• Recording attendance (check-in/check-out)
• Patrol tracking and checkpoint scanning
• Incident reporting
• Viewing schedules and duty assignments
• Communication with supervisors
• Accessing training materials

Any other use requires prior written consent.`
    },
    {
      title: '5. Prohibited Activities',
      content: `You must NOT:

• Falsify attendance or location data
• Share login credentials
• Use the app for non-work purposes
• Attempt to hack or exploit vulnerabilities
• Distribute malware through the app
• Use the app to harass or threaten others
• Bypass security measures
• Resell or redistribute the app

Violations may result in account termination and legal action.`
    },
    {
      title: '6. Data Accuracy',
      content: `You agree to:

• Provide accurate personal information
• Report incidents truthfully and promptly
• Not manipulate location or time data
• Update your information when changes occur

Falsification of data is a serious offense and may result in disciplinary action by your employer.`
    },
    {
      title: '7. Intellectual Property',
      content: `All content in the app, including but not limited to:

• Software code
• User interface designs
• Graphics and icons
• Documentation

is the property of Navya Cloud Solutions and is protected by intellectual property laws. You may not copy, modify, or distribute any part of the app.`
    },
    {
      title: '8. Limitation of Liability',
      content: `We are not liable for:

• Service interruptions or downtime
• Data loss due to technical issues
• Actions taken by your employer based on app data
• Device damage or malfunction
• Third-party service failures

Use the app at your own risk. Our maximum liability is limited to the fees paid (if any).`
    },
    {
      title: '9. Termination',
      content: `Your access may be terminated:

• By your employer at any time
• If you violate these terms
• If you engage in fraudulent activity
• Upon employment termination

We may also discontinue the service with reasonable notice.`
    },
    {
      title: '10. Updates to Terms',
      content: `We may update these terms at any time. Changes will be effective when posted. Continued use after changes constitutes acceptance.

Material changes will be communicated via email or in-app notification.`
    },
    {
      title: '11. Governing Law',
      content: `These terms are governed by the laws of India. Any disputes shall be resolved in the courts of Noida, Uttar Pradesh.`
    }
  ];

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Terms of Service</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        <View style={styles.intro}>
          <MaterialCommunityIcons name="file-document-outline" size={48} color={COLORS.primary} />
          <Text style={styles.introTitle}>Terms of Service</Text>
          <Text style={styles.introText}>
            Effective: December 1, 2024
          </Text>
        </View>

        <Text style={styles.description}>
          Please read these terms carefully before using the Security Guard App. By using our service, you agree to be bound by these terms.
        </Text>

        {sections.map((section, index) => (
          <View key={index} style={styles.section}>
            <Text style={styles.sectionTitle}>{section.title}</Text>
            <Text style={styles.sectionContent}>{section.content}</Text>
          </View>
        ))}

        <View style={styles.contact}>
          <Text style={styles.contactTitle}>Questions about these terms?</Text>
          <Text style={styles.contactText}>
            Contact our Legal Team:{'\n'}
            Email: legal@navyacloud.com{'\n'}
            Address: Sector 62, Noida, UP 201301
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
  contact: { backgroundColor: COLORS.secondary + '10', borderRadius: SIZES.radiusMd, padding: SIZES.lg, marginTop: SIZES.md, marginBottom: SIZES.xxl },
  contactTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.secondary, marginBottom: SIZES.sm },
  contactText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, lineHeight: 22 },
});

export default TermsScreen;
