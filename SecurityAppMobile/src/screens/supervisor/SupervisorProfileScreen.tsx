import React, { useState, useCallback, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { authService } from '../../services/authService';
import { userService, UserProfile } from '../../services/userService';
import { guardService } from '../../services/guardService';
import { siteService } from '../../services/siteService';

interface ProfileOption {
  id: number;
  title: string;
  subtitle?: string;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
  color: string;
  route?: string;
}

function SupervisorProfileScreen({ navigation }: any) {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [guardsCount, setGuardsCount] = useState(0);
  const [sitesCount, setSitesCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadData = useCallback(async () => {
    const user = await authService.getStoredUser();
    if (!user?.id) {
      setLoading(false);
      setRefreshing(false);
      return;
    }
    const [profileRes, guardsRes, sitesRes] = await Promise.all([
      userService.getProfile(user.id),
      guardService.getGuards({ supervisorId: user.id, pageSize: 1 }),
      siteService.getSites({ supervisorId: user.id, pageSize: 1 }),
    ]);
    if (profileRes.success && profileRes.data) setProfile(profileRes.data);
    if (guardsRes.success && guardsRes.data) {
      const raw = guardsRes.data as { totalCount?: number; TotalCount?: number; items?: any[] };
      const total = raw?.totalCount ?? raw?.TotalCount ?? (Array.isArray(raw?.items) ? raw.items.length : 0);
      setGuardsCount(typeof total === 'number' ? total : 0);
    }
    if (sitesRes.success && sitesRes.data) {
      const raw = sitesRes.data as { totalCount?: number; TotalCount?: number; items?: any[]; Items?: any[] };
      const total = raw?.totalCount ?? raw?.TotalCount ?? (Array.isArray(raw?.items) ? raw.items.length : Array.isArray(raw?.Items) ? raw.Items.length : 0);
      setSitesCount(typeof total === 'number' ? total : 0);
    }
    setLoading(false);
    setRefreshing(false);
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadData();
  }, [loadData]);

  const handleLogout = () => {
    Alert.alert(
      'Logout',
      'Are you sure you want to logout?',
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Logout',
          style: 'destructive',
          onPress: async () => {
            await authService.logout();
            navigation.reset({
              index: 0,
              routes: [{ name: 'Login' }],
            });
          }
        }
      ]
    );
  };

  const profileOptions: ProfileOption[] = [
    { id: 1, title: 'Edit Profile', subtitle: 'Update your personal information', icon: 'account-edit', color: COLORS.primary, route: 'EditProfile' },
    { id: 2, title: 'Change Password', subtitle: 'Update your password', icon: 'lock', color: COLORS.secondary, route: 'ChangePassword' },
    { id: 3, title: 'Notification Settings', subtitle: 'Manage notification preferences', icon: 'bell-cog', color: COLORS.warning, route: 'NotificationSettings' },
    { id: 4, title: 'Help & Support', subtitle: 'Get help or contact support', icon: 'help-circle', color: COLORS.info, route: 'Support' },
    { id: 5, title: 'FAQ', subtitle: 'Frequently asked questions', icon: 'frequently-asked-questions', color: COLORS.success, route: 'FAQ' },
    { id: 6, title: 'Send Feedback', subtitle: 'Share your thoughts', icon: 'message-text', color: COLORS.accent, route: 'Feedback' },
  ];

  const handleOptionPress = (option: ProfileOption) => {
    if (option.route) navigation.navigate(option.route);
  };

  const handleAbout = () => {
    Alert.alert(
      'About Security App',
      'Version: 1.0.0\n\nSecurity Guard Management System\n\n© 2024 Navya Cloud Solutions',
      [{ text: 'OK' }]
    );
  };

  const displayName = profile
    ? `${profile.firstName || ''} ${profile.lastName || ''}`.trim() || profile.userName
    : '—';
  const displayRole = profile?.roles?.[0] ?? 'Security Supervisor';
  const initials = displayName !== '—'
    ? displayName.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase()
    : 'S';
  const displayEmail = profile?.email ?? '—';
  const displayPhone = profile?.phoneNumber ?? '—';
  const displayJoined = profile?.createdDate
    ? new Date(profile.createdDate).toLocaleDateString('en-IN', { month: 'long', year: 'numeric' })
    : '—';

  if (loading && !profile) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading profile...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        <View style={styles.header}>
          <View style={styles.profileImageContainer}>
            <View style={styles.profileImage}>
              <Text style={styles.profileInitials}>{initials}</Text>
            </View>
            <TouchableOpacity style={styles.editImageButton} onPress={() => navigation.navigate('EditProfile')}>
              <MaterialCommunityIcons name="camera" size={16} color={COLORS.white} />
            </TouchableOpacity>
          </View>
          <Text style={styles.profileName}>{displayName}</Text>
          <Text style={styles.profileRole}>{displayRole}</Text>
          {profile?.id ? (
            <Text style={styles.profileId}>ID: {profile.id.slice(0, 8).toUpperCase()}</Text>
          ) : null}
        </View>

        <View style={styles.statsContainer}>
          <Card style={styles.statCard}>
            <Text style={styles.statNumber}>{guardsCount}</Text>
            <Text style={styles.statLabel}>Guards Managed</Text>
          </Card>
          <Card style={styles.statCard}>
            <Text style={styles.statNumber}>{sitesCount}</Text>
            <Text style={styles.statLabel}>Sites Supervised</Text>
          </Card>
        </View>

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Personal Information</Text>
          <Card style={styles.infoCard}>
            <View style={styles.infoRow}>
              <View style={styles.infoIconContainer}>
                <MaterialCommunityIcons name="email" size={20} color={COLORS.primary} />
              </View>
              <View style={styles.infoContent}>
                <Text style={styles.infoLabel}>Email</Text>
                <Text style={styles.infoValue}>{displayEmail}</Text>
              </View>
            </View>
            <View style={styles.infoRow}>
              <View style={styles.infoIconContainer}>
                <MaterialCommunityIcons name="phone" size={20} color={COLORS.primary} />
              </View>
              <View style={styles.infoContent}>
                <Text style={styles.infoLabel}>Phone</Text>
                <Text style={styles.infoValue}>{displayPhone || '—'}</Text>
              </View>
            </View>
            <View style={styles.infoRow}>
              <View style={styles.infoIconContainer}>
                <MaterialCommunityIcons name="calendar" size={20} color={COLORS.primary} />
              </View>
              <View style={styles.infoContent}>
                <Text style={styles.infoLabel}>Joined</Text>
                <Text style={styles.infoValue}>{displayJoined}</Text>
              </View>
            </View>
          </Card>
        </View>

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Settings</Text>
          <Card style={styles.settingsCard}>
            {profileOptions.map((option) => (
              <TouchableOpacity
                key={option.id}
                style={styles.settingItem}
                onPress={() => handleOptionPress(option)}
              >
                <View style={[styles.settingIcon, { backgroundColor: option.color + '15' }]}>
                  <MaterialCommunityIcons name={option.icon} size={22} color={option.color} />
                </View>
                <View style={styles.settingContent}>
                  <Text style={styles.settingTitle}>{option.title}</Text>
                  {option.subtitle ? (
                    <Text style={styles.settingSubtitle}>{option.subtitle}</Text>
                  ) : null}
                </View>
                <MaterialCommunityIcons name="chevron-right" size={20} color={COLORS.gray400} />
              </TouchableOpacity>
            ))}
            <TouchableOpacity style={styles.settingItem} onPress={handleAbout}>
              <View style={[styles.settingIcon, { backgroundColor: COLORS.gray600 + '15' }]}>
                <MaterialCommunityIcons name="information" size={22} color={COLORS.gray600} />
              </View>
              <View style={styles.settingContent}>
                <Text style={styles.settingTitle}>About</Text>
                <Text style={styles.settingSubtitle}>App version and information</Text>
              </View>
              <MaterialCommunityIcons name="chevron-right" size={20} color={COLORS.gray400} />
            </TouchableOpacity>
          </Card>
        </View>

        <View style={styles.logoutSection}>
          <TouchableOpacity style={styles.logoutButton} onPress={handleLogout}>
            <MaterialCommunityIcons name="logout" size={22} color={COLORS.error} />
            <Text style={styles.logoutButtonText}>Logout</Text>
          </TouchableOpacity>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center', gap: SIZES.md },
  loadingText: { fontSize: FONTS.body, color: COLORS.textSecondary },
  header: { alignItems: 'center', padding: SIZES.lg, backgroundColor: COLORS.white },
  profileImageContainer: { position: 'relative', marginBottom: SIZES.md },
  profileImage: { width: 100, height: 100, borderRadius: 50, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center' },
  profileInitials: { fontSize: FONTS.h2, fontWeight: 'bold', color: COLORS.white },
  editImageButton: { position: 'absolute', bottom: 0, right: 0, width: 32, height: 32, borderRadius: 16, backgroundColor: COLORS.secondary, justifyContent: 'center', alignItems: 'center' },
  profileName: { fontSize: FONTS.h3, fontWeight: 'bold', color: COLORS.textPrimary, marginBottom: SIZES.xs },
  profileRole: { fontSize: FONTS.body, color: COLORS.textSecondary, marginBottom: SIZES.xs },
  profileId: { fontSize: FONTS.caption, color: COLORS.gray500 },
  statsContainer: { flexDirection: 'row', justifyContent: 'space-around', padding: SIZES.md },
  statCard: { alignItems: 'center', padding: SIZES.md, minWidth: 80 },
  statNumber: { fontSize: FONTS.h3, fontWeight: 'bold', color: COLORS.primary },
  statLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, textAlign: 'center', marginTop: SIZES.xs },
  section: { padding: SIZES.md },
  sectionTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  infoCard: { padding: SIZES.md },
  infoRow: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.md },
  infoIconContainer: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center' },
  infoContent: { flex: 1, marginLeft: SIZES.sm },
  infoLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  infoValue: { fontSize: FONTS.body, color: COLORS.textPrimary, marginTop: SIZES.xs, fontWeight: '500' },
  settingsCard: { padding: 0 },
  settingItem: { flexDirection: 'row', alignItems: 'center', padding: SIZES.md, borderBottomWidth: 1, borderBottomColor: COLORS.gray100 },
  settingIcon: { width: 44, height: 44, borderRadius: 22, justifyContent: 'center', alignItems: 'center' },
  settingContent: { flex: 1, marginLeft: SIZES.sm },
  settingTitle: { fontSize: FONTS.body, fontWeight: '500', color: COLORS.textPrimary },
  settingSubtitle: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
  logoutSection: { padding: SIZES.lg },
  logoutButton: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.error + '10', paddingVertical: SIZES.md, borderRadius: SIZES.radiusMd, gap: SIZES.sm },
  logoutButtonText: { color: COLORS.error, fontSize: FONTS.body, fontWeight: '600' },
});

export default SupervisorProfileScreen;
