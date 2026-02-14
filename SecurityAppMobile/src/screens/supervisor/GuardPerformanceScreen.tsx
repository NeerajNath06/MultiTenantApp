import React, { useState, useCallback, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Dimensions,
  ActivityIndicator,
  RefreshControl,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { authService } from '../../services/authService';
import { guardService, GuardItem } from '../../services/guardService';
import { attendanceService } from '../../services/attendanceService';
import { incidentService } from '../../services/incidentService';
import { deploymentService } from '../../services/deploymentService';

const { width } = Dimensions.get('window');

interface GuardPerformanceItem {
  id: string;
  name: string;
  siteName: string;
  attendance: number;
  punctuality: number;
  incidents: number;
  patrolCompletion: number;
  rating: number;
  trend: 'up' | 'down' | 'stable';
  presentDays: number;
  scheduledDays: number;
}

function getDateRange(period: 'week' | 'month' | 'quarter'): { start: string; end: string; days: number } {
  const end = new Date();
  const start = new Date();
  let days = 7;
  if (period === 'week') {
    start.setDate(start.getDate() - 7);
    days = 7;
  } else if (period === 'month') {
    start.setDate(start.getDate() - 30);
    days = 30;
  } else {
    start.setDate(start.getDate() - 90);
    days = 90;
  }
  return {
    start: start.toISOString().split('T')[0],
    end: end.toISOString().split('T')[0],
    days,
  };
}

function computeRating(attendancePct: number, incidents: number): number {
  let r = 5;
  r = r * (attendancePct / 100);
  r = Math.max(0, r - incidents * 0.3);
  return Math.round(Math.min(5, Math.max(0, r)) * 10) / 10;
}

function GuardPerformanceScreen({ navigation }: any) {
  const [selectedPeriod, setSelectedPeriod] = useState<'week' | 'month' | 'quarter'>('month');
  const [sortBy, setSortBy] = useState<'rating' | 'attendance' | 'name'>('rating');
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [guards, setGuards] = useState<GuardPerformanceItem[]>([]);
  const [overallStats, setOverallStats] = useState({
    avgAttendance: 0,
    avgPunctuality: 0,
    totalIncidents: 0,
    avgPatrol: 0,
  });

  const loadData = useCallback(async () => {
    setError(null);
    const user = await authService.getStoredUser();
    const supervisorId = user?.isSupervisor ? user.id : undefined;
    const { start, end, days } = getDateRange(selectedPeriod);

    const [guardsRes, attendanceRes, incidentsRes, rosterRes] = await Promise.all([
      guardService.getGuards({ supervisorId, pageSize: 200, includeInactive: false }),
      attendanceService.getAttendanceList({ startDate: start, endDate: end, pageSize: 500 }),
      incidentService.getIncidents({ startDate: start, endDate: end, pageSize: 500 }),
      deploymentService.getRoster(start, end, undefined, supervisorId),
    ]);

    const guardList: GuardItem[] = guardsRes.success && guardsRes.data?.items ? guardsRes.data.items : [];
    const guardIds = new Set(guardList.map(g => g.id));

    const attRaw = attendanceRes.success && attendanceRes.data ? attendanceRes.data : {};
    const attItems = (attRaw as any).items ?? (attRaw as any).Items ?? (Array.isArray(attendanceRes.data) ? attendanceRes.data : []);
    const presentByGuard: Record<string, Set<string>> = {};
    (attItems as any[]).forEach((a: any) => {
      const gid = String(a.guardId ?? a.GuardId ?? '');
      if (!guardIds.has(gid)) return;
      if (!presentByGuard[gid]) presentByGuard[gid] = new Set();
      const d = (a.attendanceDate ?? a.AttendanceDate ?? '').toString().split('T')[0];
      if (d) presentByGuard[gid].add(d);
    });

    const incRaw = incidentsRes.success && incidentsRes.data ? incidentsRes.data : {};
    const incItems = (incRaw as any).items ?? (incRaw as any).Items ?? (Array.isArray(incidentsRes.data) ? incidentsRes.data : []);
    const incidentsByGuard: Record<string, number> = {};
    (incItems as any[]).forEach((i: any) => {
      const gid = String(i.guardId ?? i.GuardId ?? '');
      if (!gid) return;
      if (!guardIds.has(gid)) return;
      incidentsByGuard[gid] = (incidentsByGuard[gid] || 0) + 1;
    });

    const rosterData = rosterRes.success ? rosterRes.data : null;
    const deploymentsList = rosterData
      ? ((rosterData as any).deployments ?? (rosterData as any).Deployments ?? (Array.isArray(rosterData) ? rosterData : []))
      : [];
    const deploymentsArray = Array.isArray(deploymentsList) ? deploymentsList : [];
    const scheduledByGuard: Record<string, Set<string>> = {};
    deploymentsArray.forEach((d: any) => {
      const gid = String(d.guardId ?? d.GuardId ?? '');
      if (!guardIds.has(gid)) return;
      if (!scheduledByGuard[gid]) scheduledByGuard[gid] = new Set();
      const dateStr = (d.deploymentDate ?? d.DeploymentDate ?? '').toString();
      if (dateStr) scheduledByGuard[gid].add(dateStr);
    });

    const items: GuardPerformanceItem[] = guardList.map(g => {
      const presentDays = (presentByGuard[g.id]?.size ?? 0);
      const scheduledDays = (scheduledByGuard[g.id]?.size ?? 0) || days;
      const attendance = scheduledDays > 0 ? Math.round((presentDays / scheduledDays) * 100) : (days > 0 ? Math.round((presentDays / days) * 100) : 0);
      const punctuality = attendance;
      const incidents = incidentsByGuard[g.id] ?? 0;
      const patrolCompletion = attendance;
      const rating = computeRating(attendance, incidents);
      return {
        id: g.id,
        name: `${g.firstName ?? ''} ${g.lastName ?? ''}`.trim() || '—',
        siteName: '—',
        attendance: Math.min(100, attendance),
        punctuality: Math.min(100, punctuality),
        incidents,
        patrolCompletion: Math.min(100, patrolCompletion),
        rating,
        trend: 'stable' as const,
        presentDays,
        scheduledDays,
      };
    });

    const sorted = [...items].sort((a, b) => {
      if (sortBy === 'rating') return b.rating - a.rating;
      if (sortBy === 'attendance') return b.attendance - a.attendance;
      return a.name.localeCompare(b.name);
    });

    setGuards(sorted);

    if (sorted.length > 0) {
      const sumAtt = sorted.reduce((s, g) => s + g.attendance, 0);
      const sumPunc = sorted.reduce((s, g) => s + g.punctuality, 0);
      const sumPatrol = sorted.reduce((s, g) => s + g.patrolCompletion, 0);
      setOverallStats({
        avgAttendance: Math.round(sumAtt / sorted.length),
        avgPunctuality: Math.round(sumPunc / sorted.length),
        totalIncidents: sorted.reduce((s, g) => s + g.incidents, 0),
        avgPatrol: Math.round(sumPatrol / sorted.length),
      });
    } else {
      setOverallStats({ avgAttendance: 0, avgPunctuality: 0, totalIncidents: 0, avgPatrol: 0 });
    }
    setLoading(false);
    setRefreshing(false);
  }, [selectedPeriod, sortBy]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadData();
  }, [loadData]);

  const getTrendIcon = (trend: string) => {
    switch (trend) {
      case 'up': return { icon: 'trending-up' as const, color: COLORS.success };
      case 'down': return { icon: 'trending-down' as const, color: COLORS.error };
      default: return { icon: 'minus' as const, color: COLORS.gray500 };
    }
  };

  const getRatingColor = (rating: number) => {
    if (rating >= 4.5) return COLORS.success;
    if (rating >= 4.0) return COLORS.warning;
    return COLORS.error;
  };

  const renderProgressBar = (value: number, color: string) => (
    <View style={styles.progressBarContainer}>
      <View style={styles.progressBar}>
        <View style={[styles.progressFill, { width: `${Math.min(100, value)}%`, backgroundColor: color }]} />
      </View>
      <Text style={styles.progressValue}>{value}%</Text>
    </View>
  );

  const cycleSort = () => {
    setSortBy(prev => (prev === 'rating' ? 'attendance' : prev === 'attendance' ? 'name' : 'rating'));
  };

  if (loading && guards.length === 0) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading performance data...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Guard Performance</Text>
        <TouchableOpacity style={styles.filterBtn} onPress={cycleSort}>
          <MaterialCommunityIcons name="sort" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
      </View>

      {error ? (
        <View style={styles.errorBanner}>
          <Text style={styles.errorText}>{error}</Text>
        </View>
      ) : null}

      <View style={styles.periodContainer}>
        {(['week', 'month', 'quarter'] as const).map((period) => (
          <TouchableOpacity
            key={period}
            style={[styles.periodBtn, selectedPeriod === period && styles.periodBtnActive]}
            onPress={() => setSelectedPeriod(period)}
          >
            <Text style={[styles.periodText, selectedPeriod === period && styles.periodTextActive]}>
              {period.charAt(0).toUpperCase() + period.slice(1)}
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        <View style={styles.statsGrid}>
          <View style={[styles.statCard, { backgroundColor: COLORS.success + '15' }]}>
            <MaterialCommunityIcons name="account-check" size={24} color={COLORS.success} />
            <Text style={[styles.statValue, { color: COLORS.success }]}>{overallStats.avgAttendance}%</Text>
            <Text style={styles.statLabel}>Avg Attendance</Text>
          </View>
          <View style={[styles.statCard, { backgroundColor: COLORS.primaryBlue + '15' }]}>
            <MaterialCommunityIcons name="clock-check" size={24} color={COLORS.primaryBlue} />
            <Text style={[styles.statValue, { color: COLORS.primaryBlue }]}>{overallStats.avgPunctuality}%</Text>
            <Text style={styles.statLabel}>Avg Punctuality</Text>
          </View>
          <View style={[styles.statCard, { backgroundColor: COLORS.warning + '15' }]}>
            <MaterialCommunityIcons name="alert-circle" size={24} color={COLORS.warning} />
            <Text style={[styles.statValue, { color: COLORS.warning }]}>{overallStats.totalIncidents}</Text>
            <Text style={styles.statLabel}>Total Incidents</Text>
          </View>
          <View style={[styles.statCard, { backgroundColor: COLORS.secondary + '15' }]}>
            <MaterialCommunityIcons name="walk" size={24} color={COLORS.secondary} />
            <Text style={[styles.statValue, { color: COLORS.secondary }]}>{overallStats.avgPatrol}%</Text>
            <Text style={styles.statLabel}>Avg Patrol</Text>
          </View>
        </View>

        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Guard Rankings</Text>
            <TouchableOpacity style={styles.sortBtn} onPress={cycleSort}>
              <MaterialCommunityIcons name="sort" size={18} color={COLORS.primary} />
              <Text style={styles.sortText}>By {sortBy}</Text>
            </TouchableOpacity>
          </View>

          {guards.length === 0 ? (
            <View style={styles.emptyCard}>
              <Text style={styles.emptyText}>No guards under your supervision for this period, or no attendance/roster data yet.</Text>
            </View>
          ) : (
            guards.map((guard, index) => {
              const trendInfo = getTrendIcon(guard.trend);
              return (
                <View key={guard.id} style={styles.guardCard}>
                  <View style={[styles.rankBadge, index < 3 && styles.topRankBadge]}>
                    <Text style={[styles.rankText, index < 3 && styles.topRankText]}>{index + 1}</Text>
                  </View>

                  <View style={styles.guardInfo}>
                    <View style={styles.guardHeader}>
                      <View style={styles.guardAvatar}>
                        <Text style={styles.avatarText}>
                          {guard.name.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase() || 'G'}
                        </Text>
                      </View>
                      <View style={styles.guardDetails}>
                        <Text style={styles.guardName}>{guard.name}</Text>
                        <Text style={styles.guardSite}>{guard.siteName || `${guard.presentDays}/${guard.scheduledDays} days`}</Text>
                      </View>
                      <View style={styles.ratingContainer}>
                        <MaterialCommunityIcons name="star" size={18} color={getRatingColor(guard.rating)} />
                        <Text style={[styles.ratingText, { color: getRatingColor(guard.rating) }]}>{guard.rating.toFixed(1)}</Text>
                        <MaterialCommunityIcons name={trendInfo.icon} size={16} color={trendInfo.color} />
                      </View>
                    </View>

                    <View style={styles.metricsGrid}>
                      <View style={styles.metricItem}>
                        <Text style={styles.metricLabel}>Attendance</Text>
                        {renderProgressBar(guard.attendance, COLORS.success)}
                      </View>
                      <View style={styles.metricItem}>
                        <Text style={styles.metricLabel}>Punctuality</Text>
                        {renderProgressBar(guard.punctuality, COLORS.primaryBlue)}
                      </View>
                      <View style={styles.metricItem}>
                        <Text style={styles.metricLabel}>Patrol</Text>
                        {renderProgressBar(guard.patrolCompletion, COLORS.secondary)}
                      </View>
                    </View>

                    <View style={styles.incidentRow}>
                      <MaterialCommunityIcons name="alert-circle-outline" size={16} color={guard.incidents > 0 ? COLORS.warning : COLORS.success} />
                      <Text style={styles.incidentText}>{guard.incidents} incident(s) this period</Text>
                    </View>
                  </View>
                </View>
              );
            })
          )}
        </View>

        <View style={{ height: 50 }} />
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center', gap: SIZES.md },
  loadingText: { fontSize: FONTS.body, color: COLORS.textSecondary },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  filterBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  errorBanner: { backgroundColor: COLORS.error + '20', padding: SIZES.sm, marginHorizontal: SIZES.md },
  errorText: { fontSize: FONTS.caption, color: COLORS.error },
  periodContainer: { flexDirection: 'row', marginHorizontal: SIZES.md, marginTop: SIZES.md, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: 4, ...SHADOWS.small },
  periodBtn: { flex: 1, paddingVertical: SIZES.sm, alignItems: 'center', borderRadius: SIZES.radiusSm },
  periodBtnActive: { backgroundColor: COLORS.primary },
  periodText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, fontWeight: '500' },
  periodTextActive: { color: COLORS.white, fontWeight: '600' },
  statsGrid: { flexDirection: 'row', flexWrap: 'wrap', paddingHorizontal: SIZES.md, marginTop: SIZES.md, gap: SIZES.sm },
  statCard: { width: (width - 48) / 2, padding: SIZES.md, borderRadius: SIZES.radiusMd, alignItems: 'center' },
  statValue: { fontSize: FONTS.h3, fontWeight: 'bold', marginTop: SIZES.xs },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 2 },
  section: { padding: SIZES.md },
  sectionHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: SIZES.md },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  sortBtn: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  sortText: { fontSize: FONTS.caption, color: COLORS.primary },
  emptyCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.lg, marginBottom: SIZES.sm, ...SHADOWS.small },
  emptyText: { fontSize: FONTS.caption, color: COLORS.textSecondary, textAlign: 'center' },
  guardCard: { flexDirection: 'row', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  rankBadge: { width: 28, height: 28, borderRadius: 14, backgroundColor: COLORS.gray200, justifyContent: 'center', alignItems: 'center', marginRight: SIZES.sm },
  topRankBadge: { backgroundColor: COLORS.warning },
  rankText: { fontSize: FONTS.bodySmall, fontWeight: 'bold', color: COLORS.textSecondary },
  topRankText: { color: COLORS.white },
  guardInfo: { flex: 1 },
  guardHeader: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.sm },
  guardAvatar: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center' },
  avatarText: { fontSize: FONTS.bodySmall, fontWeight: 'bold', color: COLORS.white },
  guardDetails: { flex: 1, marginLeft: SIZES.sm },
  guardName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  guardSite: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  ratingContainer: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  ratingText: { fontSize: FONTS.body, fontWeight: 'bold' },
  metricsGrid: { marginBottom: SIZES.sm },
  metricItem: { marginBottom: SIZES.xs },
  metricLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginBottom: 2 },
  progressBarContainer: { flexDirection: 'row', alignItems: 'center' },
  progressBar: { flex: 1, height: 6, backgroundColor: COLORS.gray200, borderRadius: 3, marginRight: SIZES.sm },
  progressFill: { height: '100%', borderRadius: 3 },
  progressValue: { fontSize: FONTS.tiny, fontWeight: '600', color: COLORS.textPrimary, width: 35 },
  incidentRow: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  incidentText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
});

export default GuardPerformanceScreen;
