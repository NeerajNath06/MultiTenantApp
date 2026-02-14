import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { authService, StoredUserData } from '../../services/authService';
import { userService, UserProfile } from '../../services/userService';
import { attendanceService } from '../../services/attendanceService';
import { incidentService } from '../../services/incidentService';
import { deploymentService } from '../../services/deploymentService';

interface ProfileOption {
  id: number;
  title: string;
  subtitle?: string;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
  color: string;
  route?: string;
}

interface QuickStat {
  label: string;
  value: string;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
  color: string;
}

function ProfileScreen({ navigation }: any) {
  const [userData, setUserData] = useState<StoredUserData | null>(null);
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [quickStats, setQuickStats] = useState<QuickStat[]>([
    { label: 'Days Active', value: '—', icon: 'calendar-check', color: COLORS.primaryBlue },
    { label: 'Attendance', value: '—', icon: 'chart-arc', color: COLORS.success },
    { label: 'Rating', value: '—', icon: 'star', color: COLORS.accent },
    { label: 'Incidents', value: '—', icon: 'shield-check', color: COLORS.secondary },
  ]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadProfile = useCallback(async () => {
    const stored = await authService.getStoredUser();
    setUserData(stored ?? null);
    if (!stored?.id) {
      setLoading(false);
      setRefreshing(false);
      return;
    }
    const guardId = (stored as { guardId?: string }).guardId ?? stored.id;
    const end = new Date();
    const start = new Date();
    start.setDate(start.getDate() - 90);
    const startStr = start.toISOString().split('T')[0];
    const endStr = end.toISOString().split('T')[0];

    const [profileRes, attRes, incRes, depRes] = await Promise.all([
      userService.getProfile(stored.id),
      attendanceService.getAttendanceList({ guardId, startDate: startStr, endDate: endStr, pageSize: 500 }),
      incidentService.getIncidents({ guardId, startDate: startStr, endDate: endStr, pageSize: 500 }),
      deploymentService.getDeployments({ guardId, dateFrom: startStr, dateTo: endStr, pageSize: 500, skipCache: true }),
    ]);

    if (profileRes.success && profileRes.data) setProfile(profileRes.data);

    const attRaw = attRes.success && attRes.data ? attRes.data : {};
    const attItems = (attRaw as any).items ?? (attRaw as any).Items ?? (Array.isArray(attRes.data) ? attRes.data : []);
    const presentDates = new Set((attItems as any[]).map((a: any) => (a.attendanceDate ?? a.AttendanceDate ?? '').toString().split('T')[0]).filter(Boolean));
    const daysActive = presentDates.size;

    const incRaw = incRes.success && incRes.data ? incRes.data : {};
    const incItems = (incRaw as any).items ?? (incRaw as any).Items ?? (Array.isArray(incRes.data) ? incRes.data : []);
    const incidentsCount = Array.isArray(incItems) ? incItems.length : 0;

    const deploymentsList = depRes.success && depRes.data
      ? (Array.isArray(depRes.data) ? depRes.data : (depRes.data as any).items ?? (depRes.data as any).Items ?? [])
      : [];
    const scheduledDates = new Set(
      (deploymentsList as any[]).map((d: any) => (d.deploymentDate ?? d.DeploymentDate ?? '').toString()).filter(Boolean)
    );
    const scheduledDays = scheduledDates.size || 90;
    const attendancePct = scheduledDays > 0 ? Math.round((presentDates.size / scheduledDays) * 100) : 0;
    const rating: string = scheduledDays > 0 ? Math.min(5, Math.max(0, 5 * (attendancePct / 100) - incidentsCount * 0.2)).toFixed(1) : '—';

    setQuickStats([
      { label: 'Days Active', value: String(daysActive), icon: 'calendar-check', color: COLORS.primaryBlue },
      { label: 'Attendance', value: scheduledDays > 0 ? `${attendancePct}%` : '—', icon: 'chart-arc', color: COLORS.success },
      { label: 'Rating', value: rating, icon: 'star', color: COLORS.accent },
      { label: 'Incidents', value: String(incidentsCount), icon: 'shield-check', color: COLORS.secondary },
    ]);
    setLoading(false);
    setRefreshing(false);
  }, []);

  const onRefresh = useCallback(async () => {
    setRefreshing(true);
    await loadProfile();
  }, [loadProfile]);

  useEffect(() => {
    loadProfile();
  }, [loadProfile]);

  const handleLogout = (): void => {
    Alert.alert('Logout', 'Are you sure you want to logout?', [
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
    ]);
  };

  const displayName = profile
    ? `${profile.firstName || ''} ${profile.lastName || ''}`.trim() || profile.userName
    : userData?.username ?? '—';
  const displayEmail = profile?.email ?? userData?.email ?? '—';
  const displayRole = profile?.roles?.[0] ?? userData?.role ?? '—';
  const avatarInitials = displayName !== '—' ? displayName.split(/\s+/).map((n) => n[0]).slice(0, 2).join('').toUpperCase() : '?';

  const profileOptions: ProfileOption[] = [
    { id: 1, title: 'Edit Profile', subtitle: 'Update personal information', icon: 'account-edit', color: COLORS.primaryBlue, route: 'EditProfile' },
    { id: 2, title: 'Documents', subtitle: 'Certificates & ID proofs', icon: 'file-document-multiple', color: COLORS.secondary, route: 'Documents' },
    { id: 3, title: 'Attendance History', subtitle: 'View attendance records', icon: 'calendar-clock', color: COLORS.info, route: 'AttendanceHistory' },
    { id: 4, title: 'Assigned Duties', subtitle: 'Your duty schedule', icon: 'clipboard-list', color: COLORS.warning, route: 'AssignedDuties' },
    { id: 5, title: 'Salary & Payslips', subtitle: 'View salary details', icon: 'cash-multiple', color: COLORS.success, route: 'Salary' },
    { id: 6, title: 'Leave Requests', subtitle: 'Apply & track leaves', icon: 'calendar-remove', color: COLORS.error, route: 'LeaveRequest' },
  ];

  const settingsOptions: ProfileOption[] = [
    { id: 7, title: 'Notifications', subtitle: 'Manage alerts', icon: 'bell-outline', color: COLORS.accent, route: 'Notifications' },
    { id: 8, title: 'Settings', subtitle: 'App preferences', icon: 'cog-outline', color: COLORS.gray600, route: 'Settings' },
    { id: 9, title: 'Help & Support', subtitle: 'Get assistance', icon: 'help-circle-outline', color: COLORS.info, route: 'Support' },
  ];

  const handleOptionPress = (option: ProfileOption) => {
    if (option.route) {
      navigation.navigate(option.route);
    }
  };

  if (loading) {
    return (
      <SafeAreaView style={styles.container} edges={['top']}>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading profile...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container} edges={['top']}>
      <ScrollView
        showsVerticalScrollIndicator={false}
        bounces={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        {/* Header with Gradient Background */}
        <View style={styles.headerGradient}>
          <View style={styles.headerContent}>
            <TouchableOpacity style={styles.settingsBtn} onPress={() => navigation.navigate('Settings')}>
              <MaterialCommunityIcons name="cog" size={22} color={COLORS.white} />
            </TouchableOpacity>
            <View style={styles.avatarContainer}>
              <View style={styles.avatar}>
                <Text style={styles.avatarText}>{avatarInitials}</Text>
              </View>
              <TouchableOpacity style={styles.editAvatarBtn} onPress={() => navigation.navigate('EditProfile')}>
                <MaterialCommunityIcons name="camera" size={14} color={COLORS.white} />
              </TouchableOpacity>
              <View style={styles.onlineIndicator} />
            </View>
            <Text style={styles.userName}>{displayName}</Text>
            <Text style={styles.userRole}>{displayRole}</Text>
            <View style={styles.badgeContainer}>
              <View style={styles.badge}>
                <MaterialCommunityIcons name="shield-star" size={14} color={COLORS.accent} />
                <Text style={styles.badgeText}>Verified Guard</Text>
              </View>
              <Text style={styles.userId}>{userData?.guardId ? `Guard ID: ${userData.guardId}` : displayEmail}</Text>
            </View>
          </View>
        </View>

        {/* Quick Stats */}
        <View style={styles.statsWrapper}>
          <View style={styles.statsContainer}>
            {quickStats.map((stat, index) => (
              <View key={index} style={styles.statCard}>
                <View style={[styles.statIconBg, { backgroundColor: stat.color + '15' }]}>
                  <MaterialCommunityIcons name={stat.icon} size={20} color={stat.color} />
                </View>
                <Text style={styles.statValue}>{stat.value}</Text>
                <Text style={styles.statLabel}>{stat.label}</Text>
              </View>
            ))}
          </View>
        </View>

        {/* Profile Options */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Account</Text>
          <View style={styles.optionsCard}>
            {profileOptions.map((option, index) => (
              <TouchableOpacity
                key={option.id}
                style={[styles.optionItem, index !== profileOptions.length - 1 && styles.optionBorder]}
                onPress={() => handleOptionPress(option)}
                activeOpacity={0.7}
              >
                <View style={[styles.optionIcon, { backgroundColor: option.color + '12' }]}>
                  <MaterialCommunityIcons name={option.icon} size={22} color={option.color} />
                </View>
                <View style={styles.optionContent}>
                  <Text style={styles.optionTitle}>{option.title}</Text>
                  <Text style={styles.optionSubtitle}>{option.subtitle}</Text>
                </View>
                <MaterialCommunityIcons name="chevron-right" size={22} color={COLORS.gray400} />
              </TouchableOpacity>
            ))}
          </View>
        </View>

        {/* Settings Options */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Preferences</Text>
          <View style={styles.optionsCard}>
            {settingsOptions.map((option, index) => (
              <TouchableOpacity
                key={option.id}
                style={[styles.optionItem, index !== settingsOptions.length - 1 && styles.optionBorder]}
                onPress={() => handleOptionPress(option)}
                activeOpacity={0.7}
              >
                <View style={[styles.optionIcon, { backgroundColor: option.color + '12' }]}>
                  <MaterialCommunityIcons name={option.icon} size={22} color={option.color} />
                </View>
                <View style={styles.optionContent}>
                  <Text style={styles.optionTitle}>{option.title}</Text>
                  <Text style={styles.optionSubtitle}>{option.subtitle}</Text>
                </View>
                <MaterialCommunityIcons name="chevron-right" size={22} color={COLORS.gray400} />
              </TouchableOpacity>
            ))}
          </View>
        </View>

        {/* Logout Button */}
        <View style={styles.section}>
          <TouchableOpacity style={styles.logoutButton} onPress={handleLogout} activeOpacity={0.8}>
            <MaterialCommunityIcons name="logout" size={22} color={COLORS.error} />
            <Text style={styles.logoutText}>Logout</Text>
          </TouchableOpacity>
        </View>

        {/* App Version */}
        <View style={styles.footer}>
          <Text style={styles.versionText}>Security Guard App v1.0.0</Text>
          <Text style={styles.copyrightText}>© 2024 Navya Cloud Solutions</Text>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  headerGradient: {
    backgroundColor: COLORS.primary,
    paddingBottom: 60,
    borderBottomLeftRadius: 30,
    borderBottomRightRadius: 30,
  },
  headerContent: {
    alignItems: 'center',
    paddingTop: SIZES.lg,
    paddingHorizontal: SIZES.md,
  },
  settingsBtn: {
    position: 'absolute',
    top: SIZES.md,
    right: SIZES.md,
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: 'rgba(255,255,255,0.15)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  avatarContainer: {
    position: 'relative',
    marginBottom: SIZES.md,
  },
  avatar: {
    width: 100,
    height: 100,
    borderRadius: 50,
    backgroundColor: COLORS.primaryBlue,
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 4,
    borderColor: 'rgba(255,255,255,0.3)',
  },
  avatarText: {
    fontSize: 36,
    fontWeight: 'bold',
    color: COLORS.white,
  },
  editAvatarBtn: {
    position: 'absolute',
    bottom: 4,
    right: 4,
    width: 28,
    height: 28,
    borderRadius: 14,
    backgroundColor: COLORS.secondary,
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 2,
    borderColor: COLORS.white,
  },
  onlineIndicator: {
    position: 'absolute',
    top: 8,
    right: 8,
    width: 14,
    height: 14,
    borderRadius: 7,
    backgroundColor: COLORS.success,
    borderWidth: 2,
    borderColor: COLORS.white,
  },
  userName: {
    fontSize: FONTS.h3,
    fontWeight: 'bold',
    color: COLORS.white,
    marginBottom: 4,
  },
  userRole: {
    fontSize: FONTS.body,
    color: 'rgba(255,255,255,0.8)',
    marginBottom: SIZES.sm,
  },
  badgeContainer: {
    alignItems: 'center',
  },
  badge: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(255,255,255,0.15)',
    paddingHorizontal: SIZES.sm,
    paddingVertical: 4,
    borderRadius: SIZES.radiusFull,
    marginBottom: 6,
  },
  badgeText: {
    fontSize: FONTS.caption,
    color: COLORS.accent,
    fontWeight: '600',
    marginLeft: 4,
  },
  userId: {
    fontSize: FONTS.caption,
    color: 'rgba(255,255,255,0.6)',
  },
  statsWrapper: {
    marginTop: -40,
    paddingHorizontal: SIZES.md,
  },
  statsContainer: {
    flexDirection: 'row',
    backgroundColor: COLORS.white,
    borderRadius: SIZES.radiusLg,
    padding: SIZES.sm,
    ...SHADOWS.large,
  },
  statCard: {
    flex: 1,
    alignItems: 'center',
    paddingVertical: SIZES.sm,
  },
  statIconBg: {
    width: 40,
    height: 40,
    borderRadius: 20,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 6,
  },
  statValue: {
    fontSize: FONTS.h4,
    fontWeight: 'bold',
    color: COLORS.textPrimary,
  },
  statLabel: {
    fontSize: FONTS.tiny,
    color: COLORS.textSecondary,
    marginTop: 2,
  },
  section: {
    paddingHorizontal: SIZES.md,
    marginTop: SIZES.lg,
  },
  sectionTitle: {
    fontSize: FONTS.bodySmall,
    fontWeight: '600',
    color: COLORS.textSecondary,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
    marginBottom: SIZES.sm,
    marginLeft: SIZES.xs,
  },
  optionsCard: {
    backgroundColor: COLORS.white,
    borderRadius: SIZES.radiusLg,
    ...SHADOWS.small,
    overflow: 'hidden',
  },
  optionItem: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: SIZES.md,
    paddingHorizontal: SIZES.md,
  },
  optionBorder: {
    borderBottomWidth: 1,
    borderBottomColor: COLORS.gray100,
  },
  optionIcon: {
    width: 44,
    height: 44,
    borderRadius: 12,
    justifyContent: 'center',
    alignItems: 'center',
  },
  optionContent: {
    flex: 1,
    marginLeft: SIZES.sm,
  },
  optionTitle: {
    fontSize: FONTS.body,
    fontWeight: '600',
    color: COLORS.textPrimary,
  },
  optionSubtitle: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
    marginTop: 2,
  },
  logoutButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: COLORS.error + '10',
    paddingVertical: SIZES.md,
    borderRadius: SIZES.radiusLg,
    gap: SIZES.sm,
  },
  logoutText: {
    fontSize: FONTS.body,
    fontWeight: '600',
    color: COLORS.error,
  },
  loadingWrap: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    gap: SIZES.sm,
  },
  loadingText: {
    fontSize: FONTS.body,
    color: COLORS.textSecondary,
  },
  footer: {
    alignItems: 'center',
    paddingVertical: SIZES.xl,
    paddingBottom: SIZES.xxl,
  },
  versionText: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
  },
  copyrightText: {
    fontSize: FONTS.tiny,
    color: COLORS.gray400,
    marginTop: 4,
  },
});

export default ProfileScreen;
