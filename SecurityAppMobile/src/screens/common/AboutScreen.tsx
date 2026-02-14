import React from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Linking, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';

function AboutScreen({ navigation }: any) {
  const appVersion = '1.0.0';
  const buildNumber = '100';

  const features = [
    { icon: 'check-circle', text: 'Real-time attendance tracking' },
    { icon: 'check-circle', text: 'GPS-based patrol monitoring' },
    { icon: 'check-circle', text: 'Incident reporting system' },
    { icon: 'check-circle', text: 'Document management' },
    { icon: 'check-circle', text: 'Leave management' },
    { icon: 'check-circle', text: 'Emergency SOS alerts' },
  ];

  const handleRateApp = () => {
    Alert.alert('Rate Us', 'Thank you for using Security Guard App! Would you like to rate us?', [
      { text: 'Maybe Later', style: 'cancel' },
      { text: 'Rate Now', onPress: () => Alert.alert('Thanks!', 'Redirecting to app store...') }
    ]);
  };

  const handleShareApp = () => {
    Alert.alert('Share', 'Share Security Guard App with your colleagues!');
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>About</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* App Logo & Info */}
        <View style={styles.appSection}>
          <View style={styles.logoContainer}>
            <View style={styles.logo}>
              <MaterialCommunityIcons name="shield-check" size={48} color={COLORS.white} />
            </View>
          </View>
          <Text style={styles.appName}>Security Guard App</Text>
          <Text style={styles.appTagline}>Enterprise Security Management Solution</Text>
          <View style={styles.versionContainer}>
            <Text style={styles.versionText}>Version {appVersion}</Text>
            <Text style={styles.buildText}>Build {buildNumber}</Text>
          </View>
        </View>

        {/* Features */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Key Features</Text>
          <View style={styles.featuresCard}>
            {features.map((feature, index) => (
              <View key={index} style={styles.featureItem}>
                <MaterialCommunityIcons name={feature.icon as any} size={20} color={COLORS.success} />
                <Text style={styles.featureText}>{feature.text}</Text>
              </View>
            ))}
          </View>
        </View>

        {/* Actions */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Support Us</Text>
          <View style={styles.actionsCard}>
            <TouchableOpacity style={styles.actionItem} onPress={handleRateApp}>
              <View style={[styles.actionIcon, { backgroundColor: COLORS.warning + '15' }]}>
                <MaterialCommunityIcons name="star" size={24} color={COLORS.warning} />
              </View>
              <View style={styles.actionContent}>
                <Text style={styles.actionTitle}>Rate Us</Text>
                <Text style={styles.actionSubtitle}>Share your feedback</Text>
              </View>
              <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.gray400} />
            </TouchableOpacity>

            <TouchableOpacity style={styles.actionItem} onPress={handleShareApp}>
              <View style={[styles.actionIcon, { backgroundColor: COLORS.primaryBlue + '15' }]}>
                <MaterialCommunityIcons name="share-variant" size={24} color={COLORS.primaryBlue} />
              </View>
              <View style={styles.actionContent}>
                <Text style={styles.actionTitle}>Share App</Text>
                <Text style={styles.actionSubtitle}>Recommend to colleagues</Text>
              </View>
              <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.gray400} />
            </TouchableOpacity>
          </View>
        </View>

        {/* Company Info */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Developer</Text>
          <View style={styles.companyCard}>
            <Text style={styles.companyName}>Navya Cloud and Software Solutions</Text>
            <Text style={styles.companyDesc}>Building enterprise solutions for modern businesses</Text>
            <View style={styles.contactLinks}>
              <TouchableOpacity style={styles.contactBtn} onPress={() => Linking.openURL('https://navyacloud.in')}>
                <MaterialCommunityIcons name="web" size={20} color={COLORS.primaryBlue} />
                <Text style={styles.contactBtnText}>Website</Text>
              </TouchableOpacity>
              <TouchableOpacity style={styles.contactBtn} onPress={() => Linking.openURL('mailto:support@navyacloud.com')}>
                <MaterialCommunityIcons name="email" size={20} color={COLORS.secondary} />
                <Text style={styles.contactBtnText}>Email</Text>
              </TouchableOpacity>
            </View>
          </View>
        </View>

        {/* Legal */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Legal</Text>
          <View style={styles.legalCard}>
            <TouchableOpacity style={styles.legalItem} onPress={() => navigation.navigate('Privacy')}>
              <MaterialCommunityIcons name="shield-account-outline" size={20} color={COLORS.gray600} />
              <Text style={styles.legalText}>Privacy Policy</Text>
              <MaterialCommunityIcons name="chevron-right" size={20} color={COLORS.gray400} />
            </TouchableOpacity>
            <TouchableOpacity style={styles.legalItem} onPress={() => navigation.navigate('Terms')}>
              <MaterialCommunityIcons name="file-document-outline" size={20} color={COLORS.gray600} />
              <Text style={styles.legalText}>Terms of Service</Text>
              <MaterialCommunityIcons name="chevron-right" size={20} color={COLORS.gray400} />
            </TouchableOpacity>
            <TouchableOpacity style={styles.legalItem}>
              <MaterialCommunityIcons name="license" size={20} color={COLORS.gray600} />
              <Text style={styles.legalText}>Open Source Licenses</Text>
              <MaterialCommunityIcons name="chevron-right" size={20} color={COLORS.gray400} />
            </TouchableOpacity>
          </View>
        </View>

        {/* Copyright */}
        <View style={styles.footer}>
          <Text style={styles.copyright}>© 2024 Navya Cloud & Software Solutions</Text>
          <Text style={styles.rights}>All rights reserved</Text>
          <Text style={styles.madeWith}>Made with ❤️ in India</Text>
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
  appSection: { alignItems: 'center', paddingVertical: SIZES.xl },
  logoContainer: { marginBottom: SIZES.md },
  logo: { width: 100, height: 100, borderRadius: 24, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center', ...SHADOWS.large },
  appName: { fontSize: FONTS.h3, fontWeight: 'bold', color: COLORS.textPrimary },
  appTagline: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginTop: 4 },
  versionContainer: { flexDirection: 'row', marginTop: SIZES.md, gap: SIZES.md },
  versionText: { fontSize: FONTS.caption, color: COLORS.textSecondary, backgroundColor: COLORS.gray100, paddingHorizontal: SIZES.sm, paddingVertical: 4, borderRadius: SIZES.radiusSm },
  buildText: { fontSize: FONTS.caption, color: COLORS.textSecondary, backgroundColor: COLORS.gray100, paddingHorizontal: SIZES.sm, paddingVertical: 4, borderRadius: SIZES.radiusSm },
  section: { marginBottom: SIZES.lg },
  sectionTitle: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.textSecondary, textTransform: 'uppercase', letterSpacing: 0.5, marginBottom: SIZES.sm, marginLeft: SIZES.xs },
  featuresCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, ...SHADOWS.small },
  featureItem: { flexDirection: 'row', alignItems: 'center', paddingVertical: SIZES.sm },
  featureText: { fontSize: FONTS.bodySmall, color: COLORS.textPrimary, marginLeft: SIZES.sm },
  actionsCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, ...SHADOWS.small, overflow: 'hidden' },
  actionItem: { flexDirection: 'row', alignItems: 'center', padding: SIZES.md, borderBottomWidth: 1, borderBottomColor: COLORS.gray100 },
  actionIcon: { width: 44, height: 44, borderRadius: 22, justifyContent: 'center', alignItems: 'center' },
  actionContent: { flex: 1, marginLeft: SIZES.sm },
  actionTitle: { fontSize: FONTS.body, fontWeight: '500', color: COLORS.textPrimary },
  actionSubtitle: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  companyCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.lg, alignItems: 'center', ...SHADOWS.small },
  companyName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  companyDesc: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 4, textAlign: 'center' },
  contactLinks: { flexDirection: 'row', marginTop: SIZES.md, gap: SIZES.md },
  contactBtn: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.gray100, paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusFull, gap: SIZES.xs },
  contactBtnText: { fontSize: FONTS.bodySmall, color: COLORS.textPrimary, fontWeight: '500' },
  legalCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, ...SHADOWS.small, overflow: 'hidden' },
  legalItem: { flexDirection: 'row', alignItems: 'center', padding: SIZES.md, borderBottomWidth: 1, borderBottomColor: COLORS.gray100 },
  legalText: { flex: 1, fontSize: FONTS.body, color: COLORS.textPrimary, marginLeft: SIZES.sm },
  footer: { alignItems: 'center', paddingVertical: SIZES.xl },
  copyright: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  rights: { fontSize: FONTS.tiny, color: COLORS.gray400, marginTop: 4 },
  madeWith: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.md },
});

export default AboutScreen;
