import React, { useState, useEffect, useCallback } from 'react';
import { useFocusEffect } from '@react-navigation/native';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  RefreshControl,
  ActivityIndicator,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { SafeAreaView } from 'react-native-safe-area-context';
import { GuardDashboardScreenProps } from '../../types/navigation';
import { authService } from '../../services/authService';
import { attendanceService } from '../../services/attendanceService';
import { incidentService } from '../../services/incidentService';
import { deploymentService } from '../../services/deploymentService';
import { formatTimeIST } from '../../utils/dateUtils';

interface QuickAction {
  id: number;
  title: string;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
  color: string;
  route: string;
}

interface TodayStat {
  label: string;
  value: string;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
}

const DashboardScreen: React.FC<GuardDashboardScreenProps> = ({ navigation }) => {
  const [refreshing, setRefreshing] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(true);
  const [isCheckedIn, setIsCheckedIn] = useState<boolean>(false);
  const [user, setUser] = useState<any>(null);
  const [siteName, setSiteName] = useState<string>('');
  const [todayShift, setTodayShift] = useState<{ startTime: string; endTime: string; siteName: string; supervisor?: string } | null>(null);
  const [assignedDuties, setAssignedDuties] = useState<{ id: string; title: string; time: string; status: string }[]>([]);
  const [recentActivity, setRecentActivity] = useState<{ text: string; time: string }[]>([]);
  const [todayStats, setTodayStats] = useState<TodayStat[]>([
    { label: 'Total Hours', value: '0h 0m', icon: 'clock' },
    { label: 'Check-ins', value: '0', icon: 'location-enter' },
    { label: 'Incidents', value: '0', icon: 'alert-circle' },
  ]);

  useEffect(() => {
    loadDashboardData();
  }, []);

  useFocusEffect(
    useCallback(() => {
      loadDashboardData();
    }, [])
  );

  const loadDashboardData = async (): Promise<void> => {
    try {
      setLoading(true);
      const userData = await authService.getStoredUser();
      if (userData) {
        setUser(userData);
      }

      const guardId = (userData as { guardId?: string })?.guardId || userData?.id;
      if (!guardId) {
        setLoading(false);
        setRefreshing(false);
        return;
      }

      const today = new Date();
      today.setHours(0, 0, 0, 0);
      const todayStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;

      // Today's attendance via correct API (guard/{guardId}?date=)
      const attendanceRes = await attendanceService.getGuardAttendance(guardId, todayStr);
      const rawAtt = attendanceRes.data;
      const todayList: any[] = Array.isArray(rawAtt) ? rawAtt : (rawAtt?.items ?? rawAtt?.data ?? []);

      const currentAttendance = todayList.find((att: any) => {
        const hasCheckIn = att.checkInTime ?? att.CheckInTime;
        const hasCheckOut = att.checkOutTime ?? att.CheckOutTime;
        return hasCheckIn && !hasCheckOut;
      });
      setIsCheckedIn(!!currentAttendance);
      setSiteName(currentAttendance ? (currentAttendance.siteName ?? currentAttendance.SiteName ?? '') : '');

      // Total hours and stats from today's list
      let totalMinutes = 0;
      todayList.forEach((att: any) => {
        const cin = att.checkInTime ?? att.CheckInTime;
        const cout = att.checkOutTime ?? att.CheckOutTime;
        if (cin && cout) {
          const checkIn = new Date(cin);
          const checkOut = new Date(cout);
          totalMinutes += (checkOut.getTime() - checkIn.getTime()) / (1000 * 60);
        }
      });
      const hours = Math.floor(totalMinutes / 60);
      const minutes = Math.floor(totalMinutes % 60);

      let incidentsCount = 0;
      const incidentsResult = await incidentService.getIncidents({
        guardId,
        dateFrom: todayStr,
        dateTo: todayStr,
        pageSize: 100,
      });
      if (incidentsResult.success && incidentsResult.data) {
        const rawInc = incidentsResult.data;
        const incidents = Array.isArray(rawInc) ? rawInc : (rawInc?.items ?? rawInc?.data ?? []);
        incidentsCount = incidents.length;
      }

      setTodayStats([
        { label: 'Total Hours', value: `${hours}h ${minutes}m`, icon: 'clock' },
        { label: 'Check-ins', value: todayList.length.toString(), icon: 'location-enter' },
        { label: 'Incidents', value: incidentsCount.toString(), icon: 'alert-circle' },
      ]);

      // Today's shift from deployments (dateFrom/dateTo = today so API returns per-day rows for today)
      const deploymentsRes = await deploymentService.getDeployments({
        guardId,
        dateFrom: todayStr,
        dateTo: todayStr,
        pageSize: 50,
        skipCache: true,
      });
      const rawDep = deploymentsRes.data;
      const allDeployments: any[] = Array.isArray(rawDep) ? rawDep : (rawDep?.items ?? rawDep?.data ?? []);
      const deploymentsList = allDeployments.filter(
        (d: any) => (d.deploymentDate ?? d.DeploymentDate ?? '').toString().slice(0, 10) === todayStr
      );
      const firstDeployment = deploymentsList[0];
      if (firstDeployment) {
        const deploymentSiteName = firstDeployment.siteName ?? firstDeployment.SiteName ?? '—';
        setTodayShift({
          startTime: firstDeployment.startTime ?? '08:00',
          endTime: firstDeployment.endTime ?? '20:00',
          siteName: deploymentSiteName,
          supervisor: firstDeployment.supervisorName,
        });
        const assignmentIdsWithCheckIn = new Set(
          todayList
            .filter((a: any) => !(a.checkOutTime ?? a.CheckOutTime))
            .map((a: any) => String(a.assignmentId ?? a.AssignmentId ?? ''))
        );
        setAssignedDuties(
          deploymentsList.slice(0, 5).map((d: any) => {
            const aid = String(d.id ?? d.Id);
            const hasCheckedIn = assignmentIdsWithCheckIn.has(aid);
            const status = hasCheckedIn ? 'in-progress' : 'upcoming';
            return {
              id: aid,
              title: d.shiftName ?? d.ShiftName ?? 'Duty',
              time: `${d.startTime ?? '00:00'} - ${d.endTime ?? '23:59'}`,
              status,
            };
          })
        );
        if (!currentAttendance) setSiteName(deploymentSiteName === '—' ? '' : deploymentSiteName);
      } else {
        setTodayShift(null);
        setAssignedDuties([]);
      }

      // Recent activity from attendance list (last 30 days, last 5 records)
      const d30 = new Date(today);
      d30.setDate(d30.getDate() - 30);
      const startStr = `${d30.getFullYear()}-${String(d30.getMonth() + 1).padStart(2, '0')}-${String(d30.getDate()).padStart(2, '0')}`;
      const listRes = await attendanceService.getAttendanceList({
        guardId,
        startDate: startStr,
        endDate: todayStr,
        pageSize: 10,
        sortBy: 'date',
        sortDirection: 'desc',
      });
      const listRaw = listRes.data as any;
      const listItems = Array.isArray(listRaw) ? listRaw : (listRaw?.items ?? listRaw?.data ?? []);
      const activities = (listItems || []).slice(0, 5).map((a: any) => {
        const site = a.siteName ?? a.SiteName ?? 'Site';
        const checkIn = a.checkInTime ?? a.CheckInTime;
        const checkOut = a.checkOutTime ?? a.CheckOutTime;
        if (checkOut) {
          return { text: `Checked out at ${site}`, time: formatTimeIST(checkOut) };
        }
        if (checkIn) {
          return { text: `Checked in at ${site}`, time: formatTimeIST(checkIn) };
        }
        return { text: 'Attendance', time: '—' };
      }).filter((x: { text: string; time: string }) => x.time !== '—');
      setRecentActivity(activities);
    } catch (error) {
      console.error('Error loading dashboard data:', error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const onRefresh = (): void => {
    setRefreshing(true);
    loadDashboardData();
  };

  const quickActions: QuickAction[] = [
    { id: 1, title: 'Check In', icon: 'login', color: COLORS.success, route: 'CheckIn' },
    { id: 2, title: 'Check Out', icon: 'logout', color: COLORS.error, route: 'CheckOut' },
    { id: 3, title: 'Report', icon: 'alert', color: COLORS.warning, route: 'IncidentReporting' },
    { id: 4, title: 'Documents', icon: 'file-document-outline', color: COLORS.info, route: 'Documents' },
  ];


  const moreFeatures = [
    { id: 1, title: 'Patrol Tracking', icon: 'walk' as const, color: COLORS.success, route: 'PatrolTracking' },
    { id: 2, title: 'Visitor Log', icon: 'account-group' as const, color: COLORS.info, route: 'VisitorManagement' },
    { id: 3, title: 'Vehicle Log', icon: 'car' as const, color: COLORS.warning, route: 'VehicleLog' },
    { id: 4, title: 'Key Management', icon: 'key-variant' as const, color: COLORS.secondary, route: 'KeyManagement' },
    { id: 5, title: 'Daily Journal', icon: 'notebook' as const, color: COLORS.primary, route: 'DailyJournal' },
    { id: 6, title: 'Leave Request', icon: 'calendar-clock' as const, color: COLORS.error, route: 'LeaveRequest' },
    { id: 7, title: 'Salary/Payslip', icon: 'cash' as const, color: COLORS.success, route: 'Salary' },
    { id: 8, title: 'Training', icon: 'school' as const, color: COLORS.primaryBlue, route: 'Training' },
    { id: 9, title: 'Announcements', icon: 'bullhorn' as const, color: COLORS.warning, route: 'Announcements' },
    { id: 10, title: 'Overtime', icon: 'clock-plus' as const, color: COLORS.info, route: 'OvertimeRequest' },
    { id: 11, title: 'Shift Handover', icon: 'swap-horizontal' as const, color: COLORS.secondary, route: 'ShiftHandover' },
    { id: 12, title: 'Attendance', icon: 'calendar-check' as const, color: COLORS.primary, route: 'AttendanceHistory' },
  ];

  const handleQuickAction = (route: string) => {
    if (route === 'IncidentReporting') {
      // Navigate to tab screen
      navigation.navigate('IncidentReporting' as any);
    } else {
      // Navigate to stack screen
      navigation.navigate(route as any);
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
        showsVerticalScrollIndicator={false}
        keyboardShouldPersistTaps="handled"
      >
        {/* Header */}
        <LinearGradient
          colors={[COLORS.primary, COLORS.primaryLight]}
          style={styles.header}
        >
          <View style={styles.headerTop}>
            <View>
              <Text style={styles.greeting}>
                {new Date().getHours() < 12 ? 'Good Morning' : 
                 new Date().getHours() < 17 ? 'Good Afternoon' : 'Good Evening'},
              </Text>
              <Text style={styles.userName}>{user?.username || 'Guard'}</Text>
            </View>
            <TouchableOpacity
              style={styles.profileButton}
              onPress={() => navigation.navigate('Profile')}
            >
              <MaterialCommunityIcons name="account-circle" size={44} color={COLORS.white} />
            </TouchableOpacity>
          </View>

          {/* Current Status Card */}
          <View style={styles.statusCard}>
            <View style={styles.statusLeft}>
              <View style={[styles.statusDot, { backgroundColor: isCheckedIn ? COLORS.success : COLORS.gray400 }]} />
              <View>
                <Text style={styles.statusLabel}>Current Status</Text>
                <Text style={styles.statusValue}>{isCheckedIn ? 'On Duty' : 'Off Duty'}</Text>
              </View>
            </View>
            <View style={styles.statusRight}>
              <Text style={styles.siteLabel}>Site</Text>
              <Text style={styles.siteName}>{siteName || '—'}</Text>
            </View>
          </View>
        </LinearGradient>

        {/* Quick Actions */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Quick Actions</Text>
          <View style={styles.actionsGrid}>
            {quickActions.map((action) => (
              <TouchableOpacity
                key={action.id}
                style={styles.actionCard}
                onPress={() => handleQuickAction(action.route)}
              >
                <View style={[styles.actionIcon, { backgroundColor: `${action.color}15` }]}>
                  <MaterialCommunityIcons name={action.icon} size={28} color={action.color} />
                </View>
                <Text style={styles.actionTitle}>{action.title}</Text>
              </TouchableOpacity>
            ))}
          </View>
        </View>

        {/* Today's Shift */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Today's Shift</Text>
            <TouchableOpacity onPress={() => navigation.navigate('TodayShift')}>
              <Text style={styles.viewAll}>View Details</Text>
            </TouchableOpacity>
          </View>
          <Card style={styles.shiftCard}>
            {todayShift ? (
              <>
                <View style={styles.shiftHeader}>
                  <View style={styles.shiftTime}>
                    <MaterialCommunityIcons name="clock-outline" size={20} color={COLORS.primary} />
                    <Text style={styles.shiftTimeText}>{todayShift.startTime} - {todayShift.endTime}</Text>
                  </View>
                  <View style={[styles.shiftBadge, { backgroundColor: COLORS.success + '20' }]}>
                    <Text style={[styles.shiftBadgeText, { color: COLORS.success }]}>Active</Text>
                  </View>
                </View>
                <View style={styles.shiftDetails}>
                  <View style={styles.shiftDetail}>
                    <Text style={styles.shiftDetailLabel}>Site</Text>
                    <Text style={styles.shiftDetailValue}>{todayShift.siteName}</Text>
                  </View>
                  {todayShift.supervisor ? (
                    <View style={styles.shiftDetail}>
                      <Text style={styles.shiftDetailLabel}>Supervisor</Text>
                      <Text style={styles.shiftDetailValue}>{todayShift.supervisor}</Text>
                    </View>
                  ) : null}
                </View>
              </>
            ) : (
              <Text style={styles.shiftDetailValue}>No shift scheduled for today</Text>
            )}
          </Card>
        </View>

        {/* Today's Stats */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Today's Summary</Text>
          <View style={styles.statsRow}>
            {todayStats.map((stat, index) => (
              <Card key={index} style={styles.statCard}>
                <MaterialCommunityIcons name={stat.icon} size={24} color={COLORS.primary} />
                <Text style={styles.statValue}>{stat.value}</Text>
                <Text style={styles.statLabel}>{stat.label}</Text>
              </Card>
            ))}
          </View>
        </View>

        {/* Assigned Duties */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Assigned Duties</Text>
            <TouchableOpacity onPress={() => navigation.navigate('AssignedDuties')}>
              <Text style={styles.viewAll}>View All</Text>
            </TouchableOpacity>
          </View>
          <Card style={styles.dutyCard}>
            {assignedDuties.length > 0 ? assignedDuties.map((duty, index) => (
              <React.Fragment key={duty.id}>
                {index > 0 && <View style={styles.dutyDivider} />}
                <View style={styles.dutyItem}>
                  <View style={styles.dutyIcon}>
                    <MaterialCommunityIcons
                      name={duty.status === 'in-progress' ? 'clock' : 'check-circle'}
                      size={24}
                      color={duty.status === 'in-progress' ? COLORS.warning : COLORS.success}
                    />
                  </View>
                  <View style={styles.dutyContent}>
                    <Text style={styles.dutyTitle}>{duty.title}</Text>
                    <Text style={styles.dutyTime}>{duty.time}</Text>
                  </View>
                  <Text style={[styles.dutyStatus, duty.status === 'in-progress' && { color: COLORS.warning }]}>
                    {duty.status === 'in-progress' ? 'Pending' : 'Completed'}
                  </Text>
                </View>
              </React.Fragment>
            )) : (
              <Text style={styles.dutyTime}>No duties assigned for today</Text>
            )}
          </Card>
        </View>

        {/* Recent Activity */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Recent Activity</Text>
          </View>
          <Card style={styles.activityCard}>
            {recentActivity.length > 0 ? recentActivity.map((act, index) => (
              <View key={index} style={styles.activityItem}>
                <View style={[styles.activityDot, { backgroundColor: COLORS.success }]} />
                <View style={styles.activityContent}>
                  <Text style={styles.activityText}>{act.text}</Text>
                  <Text style={styles.activityTime}>{act.time}</Text>
                </View>
              </View>
            )) : (
              <Text style={styles.activityTime}>No recent activity</Text>
            )}
          </Card>
        </View>

        {/* More Features */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>More Features</Text>
          <View style={styles.featuresGrid}>
            {moreFeatures.map((feature) => (
              <TouchableOpacity
                key={feature.id}
                style={styles.featureCard}
                onPress={() => navigation.navigate(feature.route as any)}
              >
                <View style={[styles.featureIcon, { backgroundColor: feature.color + '15' }]}>
                  <MaterialCommunityIcons name={feature.icon} size={22} color={feature.color} />
                </View>
                <Text style={styles.featureTitle}>{feature.title}</Text>
              </TouchableOpacity>
            ))}
          </View>
        </View>

        {/* Utility Actions */}
        <View style={[styles.section, { marginBottom: SIZES.xxl }]}>
          <Text style={styles.sectionTitle}>Utilities</Text>
          <View style={styles.utilityRow}>
            <TouchableOpacity style={styles.utilityCard} onPress={() => navigation.navigate('EmergencySOS')}>
              <View style={[styles.utilityIcon, { backgroundColor: COLORS.error + '15' }]}>
                <MaterialCommunityIcons name="alarm-light" size={28} color={COLORS.error} />
              </View>
              <Text style={styles.utilityTitle}>Emergency SOS</Text>
              <Text style={styles.utilitySubtitle}>One tap alert</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.utilityCard} onPress={() => navigation.navigate('QRScanner')}>
              <View style={[styles.utilityIcon, { backgroundColor: COLORS.primary + '15' }]}>
                <MaterialCommunityIcons name="qrcode-scan" size={28} color={COLORS.primary} />
              </View>
              <Text style={styles.utilityTitle}>Scan QR</Text>
              <Text style={styles.utilitySubtitle}>Checkpoint scan</Text>
            </TouchableOpacity>
          </View>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  scrollContent: {
    flexGrow: 1,
    paddingBottom: SIZES.xxl,
  },
  header: {
    paddingTop: SIZES.lg,
    paddingHorizontal: SIZES.md,
    paddingBottom: SIZES.xl,
    borderBottomLeftRadius: SIZES.radiusXl,
    borderBottomRightRadius: SIZES.radiusXl,
  },
  headerTop: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: SIZES.md,
  },
  greeting: {
    fontSize: FONTS.bodySmall,
    color: 'rgba(255,255,255,0.8)',
  },
  userName: {
    fontSize: FONTS.h3,
    fontWeight: 'bold',
    color: COLORS.white,
  },
  profileButton: {
    width: 48,
    height: 48,
    borderRadius: 24,
    justifyContent: 'center',
    alignItems: 'center',
  },
  statusCard: {
    backgroundColor: 'rgba(255,255,255,0.15)',
    borderRadius: SIZES.radiusMd,
    padding: SIZES.md,
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  statusLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: SIZES.sm,
  },
  statusDot: {
    width: 12,
    height: 12,
    borderRadius: 6,
  },
  statusLabel: {
    fontSize: FONTS.caption,
    color: 'rgba(255,255,255,0.7)',
  },
  statusValue: {
    fontSize: FONTS.body,
    fontWeight: '600',
    color: COLORS.white,
  },
  statusRight: {
    alignItems: 'flex-end',
  },
  siteLabel: {
    fontSize: FONTS.caption,
    color: 'rgba(255,255,255,0.7)',
  },
  siteName: {
    fontSize: FONTS.bodySmall,
    fontWeight: '500',
    color: COLORS.white,
  },
  section: {
    paddingHorizontal: SIZES.md,
    marginTop: SIZES.lg,
  },
  sectionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: SIZES.sm,
  },
  sectionTitle: {
    fontSize: FONTS.h4,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: SIZES.sm,
  },
  viewAll: {
    fontSize: FONTS.bodySmall,
    color: COLORS.primary,
    fontWeight: '500',
  },
  actionsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
  },
  actionCard: {
    width: '48%',
    backgroundColor: COLORS.white,
    borderRadius: SIZES.radiusMd,
    padding: SIZES.md,
    alignItems: 'center',
    marginBottom: SIZES.sm,
    ...SHADOWS.small,
  },
  actionIcon: {
    width: 56,
    height: 56,
    borderRadius: 28,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: SIZES.sm,
  },
  actionTitle: {
    fontSize: FONTS.bodySmall,
    fontWeight: '600',
    color: COLORS.textPrimary,
  },
  shiftCard: {
    padding: SIZES.md,
  },
  shiftHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: SIZES.md,
  },
  shiftTime: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: SIZES.xs,
  },
  shiftTimeText: {
    fontSize: FONTS.body,
    fontWeight: '600',
    color: COLORS.textPrimary,
  },
  shiftBadge: {
    paddingHorizontal: SIZES.sm,
    paddingVertical: SIZES.xs,
    borderRadius: SIZES.radiusSm,
  },
  shiftBadgeText: {
    fontSize: FONTS.caption,
    fontWeight: '600',
  },
  shiftDetails: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  shiftDetail: {},
  shiftDetailLabel: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
  },
  shiftDetailValue: {
    fontSize: FONTS.bodySmall,
    fontWeight: '500',
    color: COLORS.textPrimary,
    marginTop: 2,
  },
  statsRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  statCard: {
    flex: 1,
    alignItems: 'center',
    marginHorizontal: SIZES.xs,
    padding: SIZES.md,
  },
  statValue: {
    fontSize: FONTS.h3,
    fontWeight: 'bold',
    color: COLORS.textPrimary,
    marginTop: SIZES.sm,
  },
  statLabel: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
    marginTop: SIZES.xs,
  },
  dutyCard: {
    padding: SIZES.md,
  },
  dutyItem: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  dutyIcon: {
    marginRight: SIZES.sm,
  },
  dutyContent: {
    flex: 1,
  },
  dutyTitle: {
    fontSize: FONTS.body,
    fontWeight: '500',
    color: COLORS.textPrimary,
  },
  dutyTime: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
    marginTop: 2,
  },
  dutyStatus: {
    fontSize: FONTS.caption,
    fontWeight: '600',
    color: COLORS.success,
  },
  dutyDivider: {
    height: 1,
    backgroundColor: COLORS.gray200,
    marginVertical: SIZES.sm,
  },
  activityCard: {
    padding: SIZES.md,
  },
  activityItem: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    marginBottom: SIZES.md,
  },
  activityDot: {
    width: 10,
    height: 10,
    borderRadius: 5,
    marginTop: 4,
    marginRight: SIZES.sm,
  },
  activityContent: {
    flex: 1,
  },
  activityText: {
    fontSize: FONTS.bodySmall,
    color: COLORS.textPrimary,
  },
  activityTime: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
    marginTop: 2,
  },
  featuresGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
  },
  featureCard: {
    width: '23%',
    backgroundColor: COLORS.white,
    borderRadius: SIZES.radiusMd,
    padding: SIZES.sm,
    alignItems: 'center',
    marginBottom: SIZES.sm,
    ...SHADOWS.small,
  },
  featureIcon: {
    width: 44,
    height: 44,
    borderRadius: 22,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: SIZES.xs,
  },
  featureTitle: {
    fontSize: FONTS.tiny,
    fontWeight: '500',
    color: COLORS.textPrimary,
    textAlign: 'center',
  },
  utilityRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  utilityCard: {
    width: '48%',
    backgroundColor: COLORS.white,
    borderRadius: SIZES.radiusMd,
    padding: SIZES.md,
    alignItems: 'center',
    ...SHADOWS.small,
  },
  utilityIcon: {
    width: 56,
    height: 56,
    borderRadius: 28,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: SIZES.sm,
  },
  utilityTitle: {
    fontSize: FONTS.body,
    fontWeight: '600',
    color: COLORS.textPrimary,
  },
  utilitySubtitle: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
    marginTop: 2,
  },
});

export default DashboardScreen;
