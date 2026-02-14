import React, { useState, useCallback, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Dimensions, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { authService } from '../../services/authService';
import { guardService } from '../../services/guardService';
import { attendanceService } from '../../services/attendanceService';
import { incidentService } from '../../services/incidentService';
import { leaveService } from '../../services/leaveService';
import { siteService } from '../../services/siteService';
import { deploymentService } from '../../services/deploymentService';

const { width } = Dimensions.get('window');
const CHART_WIDTH = width - 32;

interface MetricCard {
  label: string;
  value: string;
  change: number;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
  color: string;
}

interface BarData {
  label: string;
  value: number;
  color: string;
}

interface SitePerf {
  site: string;
  siteId: string;
  guards: number;
  attendance: number;
  incidents: number;
  rating: number;
}

interface TopGuard {
  name: string;
  attendance: number;
  incidents: number;
  rating: number;
}

function getDateRange(period: 'week' | 'month' | 'year') {
  const end = new Date();
  const start = new Date();
  if (period === 'week') start.setDate(start.getDate() - 7);
  else if (period === 'month') start.setDate(start.getDate() - 30);
  else start.setDate(start.getDate() - 365);
  return { start: start.toISOString().split('T')[0], end: end.toISOString().split('T')[0] };
}

function AnalyticsDashboardScreen({ navigation }: any) {
  const [selectedPeriod, setSelectedPeriod] = useState<'week' | 'month' | 'year'>('month');
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [metrics, setMetrics] = useState<MetricCard[]>([
    { label: 'Total Guards', value: '—', change: 0, icon: 'account-group', color: COLORS.primaryBlue },
    { label: 'Attendance Rate', value: '—', change: 0, icon: 'chart-arc', color: COLORS.success },
    { label: 'Incidents', value: '—', change: 0, icon: 'alert-circle', color: COLORS.warning },
    { label: 'On Leave', value: '—', change: 0, icon: 'account-off', color: COLORS.error },
  ]);
  const [attendanceData, setAttendanceData] = useState<BarData[]>([]);
  const [incidentData, setIncidentData] = useState<BarData[]>([]);
  const [sitePerformance, setSitePerformance] = useState<SitePerf[]>([]);
  const [topGuards, setTopGuards] = useState<TopGuard[]>([]);

  const loadData = useCallback(async () => {
    const user = await authService.getStoredUser();
    const supervisorId = user?.isSupervisor ? user.id : undefined;
    const { start, end } = getDateRange(selectedPeriod);

    const [guardsRes, attRes, incRes, leaveRes, sitesRes, rosterRes] = await Promise.all([
      guardService.getGuards({ supervisorId, pageSize: 500 }),
      attendanceService.getAttendanceList({ startDate: start, endDate: end, pageSize: 1000 }),
      incidentService.getIncidents({ startDate: start, endDate: end, pageSize: 500 }),
      leaveService.getLeaveRequests({ supervisorId, pageSize: 100, status: 'Approved' }),
      siteService.getSites({ supervisorId, pageSize: 100 }),
      deploymentService.getRoster(start, end, undefined, supervisorId),
    ]);

    const guardsList = guardsRes.success && guardsRes.data?.items ? guardsRes.data.items : [];
    const totalGuards = (guardsRes.data as any)?.totalCount ?? guardsList.length;

    const attRaw = attRes.success && attRes.data ? attRes.data : {};
    const attItems = (attRaw as any).items ?? (attRaw as any).Items ?? (Array.isArray(attRes.data) ? attRes.data : []);
    const deploymentsList = rosterRes.success && rosterRes.data ? ((rosterRes.data as any).deployments ?? (rosterRes.data as any).Deployments ?? []) : [];
    const scheduledTotal = (deploymentsList as any[]).length;
    const presentUnique = new Set((attItems as any[]).map((a: any) => (a.attendanceDate ?? a.AttendanceDate ?? '').toString().split('T')[0]).filter(Boolean)).size;
    const attendanceRate = scheduledTotal > 0 ? Math.round((presentUnique / Math.min(scheduledTotal, 500)) * 100) : 0;

    const incRaw = incRes.success && incRes.data ? incRes.data : {};
    const incItems = (incRaw as any).items ?? (incRaw as any).Items ?? (Array.isArray(incRes.data) ? incRes.data : []);
    const incidentsCount = Array.isArray(incItems) ? incItems.length : 0;

    const leaveRaw = leaveRes.success && leaveRes.data ? leaveRes.data : {};
    const leaveItems = (leaveRaw as any).items ?? (leaveRaw as any).Items ?? (Array.isArray(leaveRes.data) ? leaveRes.data : []);
    const onLeaveCount = Array.isArray(leaveItems) ? leaveItems.length : 0;

    setMetrics([
      { label: 'Total Guards', value: String(totalGuards), change: 0, icon: 'account-group', color: COLORS.primaryBlue },
      { label: 'Attendance Rate', value: scheduledTotal > 0 ? `${Math.min(100, attendanceRate)}%` : '—', change: 0, icon: 'chart-arc', color: COLORS.success },
      { label: 'Incidents', value: String(incidentsCount), change: 0, icon: 'alert-circle', color: COLORS.warning },
      { label: 'On Leave', value: String(onLeaveCount), change: 0, icon: 'account-off', color: COLORS.error },
    ]);

    const dayLabels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
    const byDay: Record<number, number> = { 0: 0, 1: 0, 2: 0, 3: 0, 4: 0, 5: 0, 6: 0 };
    (attItems as any[]).forEach((a: any) => {
      const d = (a.attendanceDate ?? a.AttendanceDate ?? '').toString();
      if (!d) return;
      const day = new Date(d).getDay();
      byDay[day] = (byDay[day] || 0) + 1;
    });
    const maxByDay = Math.max(1, ...Object.values(byDay));
    setAttendanceData(dayLabels.map((label, i) => {
      const dayNum = i === 6 ? 0 : i + 1;
      const v = byDay[dayNum] ?? 0;
      return { label, value: maxByDay ? Math.round((v / maxByDay) * 100) : 0, color: v >= maxByDay * 0.9 ? COLORS.success : COLORS.warning };
    }));

    const byType: Record<string, number> = {};
    (incItems as any[]).forEach((i: any) => {
      const t = (i.incidentType ?? i.IncidentType ?? 'Other').toString();
      byType[t] = (byType[t] || 0) + 1;
    });
    const colors = [COLORS.error, COLORS.warning, COLORS.success, COLORS.info];
    setIncidentData(Object.entries(byType).slice(0, 4).map(([label, value], idx) => ({ label: label.slice(0, 8), value, color: colors[idx % colors.length] })));
    if (Object.keys(byType).length === 0) setIncidentData([{ label: 'None', value: 0, color: COLORS.gray300 }]);

    const sitesRaw = sitesRes.success && sitesRes.data ? sitesRes.data : {};
    const sitesList = (sitesRaw as any).items ?? (sitesRaw as any).Items ?? (Array.isArray(sitesRes.data) ? sitesRes.data : []);
    const sitePerf: SitePerf[] = (sitesList as any[]).slice(0, 5).map((s: any) => ({
      site: (s.siteName ?? s.SiteName ?? '—').toString(),
      siteId: String(s.id ?? s.Id ?? ''),
      guards: 0,
      attendance: 0,
      incidents: 0,
      rating: 0,
    }));
    setSitePerformance(sitePerf);

    const guardIds = new Set(guardsList.map((g: any) => g.id));
    const presentByGuard: Record<string, Set<string>> = {};
    const incByGuard: Record<string, number> = {};
    (attItems as any[]).forEach((a: any) => {
      const gid = String(a.guardId ?? a.GuardId ?? '');
      if (!guardIds.has(gid)) return;
      if (!presentByGuard[gid]) presentByGuard[gid] = new Set();
      presentByGuard[gid].add((a.attendanceDate ?? a.AttendanceDate ?? '').toString().split('T')[0]);
    });
    (incItems as any[]).forEach((i: any) => {
      const gid = String(i.guardId ?? i.GuardId ?? '');
      if (!guardIds.has(gid)) return;
      incByGuard[gid] = (incByGuard[gid] || 0) + 1;
    });
    const scheduledByGuard: Record<string, number> = {};
    (deploymentsList as any[]).forEach((d: any) => {
      const gid = String(d.guardId ?? d.GuardId ?? '');
      if (!guardIds.has(gid)) return;
      scheduledByGuard[gid] = (scheduledByGuard[gid] || 0) + 1;
    });
    const top: TopGuard[] = guardsList.map((g: any) => {
      const id = g.id;
      const present = (presentByGuard[id]?.size ?? 0);
      const scheduled = scheduledByGuard[id] || 30;
      const att = scheduled > 0 ? Math.round((present / scheduled) * 100) : 0;
      const inc = incByGuard[id] ?? 0;
      const rating = Math.min(5, Math.max(0, 5 * (att / 100) - inc * 0.2));
      return {
        name: `${g.firstName ?? ''} ${g.lastName ?? ''}`.trim() || '—',
        attendance: att,
        incidents: inc,
        rating: Math.round(rating * 10) / 10,
      };
    }).sort((a: TopGuard, b: TopGuard) => b.rating - a.rating).slice(0, 5);
    setTopGuards(top);

    setLoading(false);
    setRefreshing(false);
  }, [selectedPeriod]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadData();
  }, [loadData]);

  const renderBarChart = (data: BarData[], maxValue: number, height: number = 120) => {
    const barWidth = (CHART_WIDTH - 40 - data.length * 8) / data.length;
    return (
      <View style={styles.chartContainer}>
        <View style={[styles.barChart, { height }]}>
          {data.map((item, index) => (
            <View key={index} style={styles.barContainer}>
              <View style={[styles.bar, { height: (item.value / maxValue) * height, backgroundColor: item.color, width: barWidth }]} />
              <Text style={styles.barLabel}>{item.label}</Text>
              <Text style={styles.barValue}>{item.value}{maxValue === 100 ? '%' : ''}</Text>
            </View>
          ))}
        </View>
      </View>
    );
  };

  const renderDonutChart = () => {
    const total = incidentData.reduce((sum, item) => sum + item.value, 0);
    let currentAngle = 0;
    
    return (
      <View style={styles.donutContainer}>
        <View style={styles.donut}>
          <View style={styles.donutInner}>
            <Text style={styles.donutValue}>{total}</Text>
            <Text style={styles.donutLabel}>Incidents</Text>
          </View>
        </View>
        <View style={styles.donutLegend}>
          {incidentData.map((item, index) => (
            <View key={index} style={styles.legendItem}>
              <View style={[styles.legendDot, { backgroundColor: item.color }]} />
              <Text style={styles.legendText}>{item.label}</Text>
              <Text style={styles.legendValue}>{item.value}</Text>
            </View>
          ))}
        </View>
      </View>
    );
  };

  const avgAttendance = metrics[1]?.value ?? '—';

  if (loading && metrics[0].value === '—') {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading analytics...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Analytics Dashboard</Text>
        <TouchableOpacity style={styles.exportBtn} onPress={onRefresh}>
          <MaterialCommunityIcons name="refresh" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        {/* Period Selector */}
        <View style={styles.periodContainer}>
          {(['week', 'month', 'year'] as const).map((period) => (
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

        {/* Metrics Grid */}
        <View style={styles.metricsGrid}>
          {metrics.map((metric, index) => (
            <View key={index} style={styles.metricCard}>
              <View style={[styles.metricIcon, { backgroundColor: metric.color + '15' }]}>
                <MaterialCommunityIcons name={metric.icon} size={24} color={metric.color} />
              </View>
              <Text style={styles.metricValue}>{metric.value}</Text>
              <Text style={styles.metricLabel}>{metric.label}</Text>
              <View style={[styles.changeBadge, { backgroundColor: metric.change >= 0 ? COLORS.success + '15' : COLORS.error + '15' }]}>
                <MaterialCommunityIcons 
                  name={metric.change >= 0 ? 'trending-up' : 'trending-down'} 
                  size={12} 
                  color={metric.change >= 0 ? COLORS.success : COLORS.error} 
                />
                <Text style={[styles.changeText, { color: metric.change >= 0 ? COLORS.success : COLORS.error }]}>
                  {Math.abs(metric.change)}%
                </Text>
              </View>
            </View>
          ))}
        </View>

        {/* Attendance Chart */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Weekly Attendance</Text>
            <Text style={styles.sectionSubtitle}>Avg: {avgAttendance}</Text>
          </View>
          <View style={styles.chartCard}>
            {attendanceData.length ? renderBarChart(attendanceData, 100) : <Text style={styles.emptyChartText}>No attendance data for this period</Text>}
          </View>
        </View>

        {/* Incidents Breakdown */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Incidents by Category</Text>
            <Text style={styles.sectionSubtitle}>This {selectedPeriod}</Text>
          </View>
          <View style={styles.chartCard}>
            {renderDonutChart()}
          </View>
        </View>

        {/* Site Performance */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Site Performance</Text>
            <TouchableOpacity>
              <Text style={styles.viewAll}>View All</Text>
            </TouchableOpacity>
          </View>
          {sitePerformance.length ? sitePerformance.map((site, index) => (
            <View key={site.siteId || index} style={styles.siteCard}>
              <View style={styles.siteHeader}>
                <Text style={styles.siteName}>{site.site}</Text>
              </View>
              <View style={styles.siteStats}>
                <View style={styles.siteStat}>
                  <Text style={styles.siteStatValue}>{site.guards}</Text>
                  <Text style={styles.siteStatLabel}>Guards</Text>
                </View>
                <View style={styles.siteStat}>
                  <Text style={[styles.siteStatValue, { color: COLORS.success }]}>{site.attendance}%</Text>
                  <Text style={styles.siteStatLabel}>Attendance</Text>
                </View>
                <View style={styles.siteStat}>
                  <Text style={[styles.siteStatValue, { color: site.incidents > 2 ? COLORS.error : COLORS.warning }]}>{site.incidents}</Text>
                  <Text style={styles.siteStatLabel}>Incidents</Text>
                </View>
              </View>
            </View>
          )) : <Text style={styles.emptyChartText}>No sites</Text>}
        </View>

        {/* Top Performers */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Top Performers</Text>
            <TouchableOpacity>
              <Text style={styles.viewAll}>View All</Text>
            </TouchableOpacity>
          </View>
          {topGuards.map((guard, index) => (
            <View key={index} style={styles.guardCard}>
              <View style={styles.rankBadge}>
                <Text style={styles.rankText}>{index + 1}</Text>
              </View>
              <View style={styles.guardAvatar}>
                <Text style={styles.avatarText}>{guard.name.split(' ').map(n => n[0]).join('')}</Text>
              </View>
              <View style={styles.guardInfo}>
                <Text style={styles.guardName}>{guard.name}</Text>
                <View style={styles.guardStats}>
                  <Text style={styles.guardStat}>{guard.attendance}% attendance</Text>
                  <Text style={styles.guardStat}>•</Text>
                  <Text style={styles.guardStat}>{guard.incidents} incidents</Text>
                </View>
              </View>
              <View style={styles.guardRating}>
                <MaterialCommunityIcons name="star" size={18} color={COLORS.warning} />
                <Text style={styles.guardRatingText}>{guard.rating}</Text>
              </View>
            </View>
          ))}
        </View>

        {/* Quick Actions */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Quick Reports</Text>
          <View style={styles.actionsGrid}>
            <TouchableOpacity style={styles.actionCard}>
              <MaterialCommunityIcons name="file-pdf-box" size={32} color={COLORS.error} />
              <Text style={styles.actionLabel}>Attendance Report</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionCard}>
              <MaterialCommunityIcons name="file-excel" size={32} color={COLORS.success} />
              <Text style={styles.actionLabel}>Incident Report</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionCard}>
              <MaterialCommunityIcons name="chart-line" size={32} color={COLORS.primaryBlue} />
              <Text style={styles.actionLabel}>Performance</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionCard}>
              <MaterialCommunityIcons name="cash-multiple" size={32} color={COLORS.warning} />
              <Text style={styles.actionLabel}>Payroll Summary</Text>
            </TouchableOpacity>
          </View>
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
  emptyChartText: { fontSize: FONTS.caption, color: COLORS.textSecondary, textAlign: 'center', padding: SIZES.md },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.primary },
  backBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.white },
  exportBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  periodContainer: { flexDirection: 'row', marginHorizontal: SIZES.md, marginTop: SIZES.md, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: 4, ...SHADOWS.small },
  periodBtn: { flex: 1, paddingVertical: SIZES.sm, alignItems: 'center', borderRadius: SIZES.radiusSm },
  periodBtnActive: { backgroundColor: COLORS.primary },
  periodText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, fontWeight: '500' },
  periodTextActive: { color: COLORS.white, fontWeight: '600' },
  metricsGrid: { flexDirection: 'row', flexWrap: 'wrap', paddingHorizontal: SIZES.md, marginTop: SIZES.md, gap: SIZES.sm },
  metricCard: { width: (width - 48) / 2, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, ...SHADOWS.small },
  metricIcon: { width: 48, height: 48, borderRadius: 24, justifyContent: 'center', alignItems: 'center', marginBottom: SIZES.sm },
  metricValue: { fontSize: FONTS.h2, fontWeight: 'bold', color: COLORS.textPrimary },
  metricLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  changeBadge: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusFull, alignSelf: 'flex-start', marginTop: SIZES.sm, gap: 2 },
  changeText: { fontSize: FONTS.tiny, fontWeight: '600' },
  section: { paddingHorizontal: SIZES.md, marginTop: SIZES.lg },
  sectionHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: SIZES.md },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  sectionSubtitle: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  viewAll: { fontSize: FONTS.bodySmall, color: COLORS.primary, fontWeight: '500' },
  chartCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, ...SHADOWS.small },
  chartContainer: { paddingTop: SIZES.sm },
  barChart: { flexDirection: 'row', alignItems: 'flex-end', justifyContent: 'space-between' },
  barContainer: { alignItems: 'center' },
  bar: { borderRadius: 4, minWidth: 20 },
  barLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 4 },
  barValue: { fontSize: FONTS.tiny, fontWeight: '600', color: COLORS.textPrimary },
  donutContainer: { flexDirection: 'row', alignItems: 'center', padding: SIZES.sm },
  donut: { width: 120, height: 120, borderRadius: 60, borderWidth: 20, borderColor: COLORS.gray200, justifyContent: 'center', alignItems: 'center' },
  donutInner: { alignItems: 'center' },
  donutValue: { fontSize: FONTS.h2, fontWeight: 'bold', color: COLORS.textPrimary },
  donutLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  donutLegend: { flex: 1, marginLeft: SIZES.lg },
  legendItem: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.sm },
  legendDot: { width: 12, height: 12, borderRadius: 6, marginRight: SIZES.sm },
  legendText: { flex: 1, fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  legendValue: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary },
  siteCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  siteHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: SIZES.sm },
  siteName: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary, flex: 1 },
  ratingBadge: { flexDirection: 'row', alignItems: 'center', gap: 2 },
  ratingText: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary },
  siteStats: { flexDirection: 'row', justifyContent: 'space-around' },
  siteStat: { alignItems: 'center' },
  siteStatValue: { fontSize: FONTS.h4, fontWeight: 'bold', color: COLORS.textPrimary },
  siteStatLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 2 },
  guardCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  rankBadge: { width: 28, height: 28, borderRadius: 14, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center' },
  rankText: { fontSize: FONTS.bodySmall, fontWeight: 'bold', color: COLORS.white },
  guardAvatar: { width: 44, height: 44, borderRadius: 22, backgroundColor: COLORS.primaryBlue, justifyContent: 'center', alignItems: 'center', marginLeft: SIZES.sm },
  avatarText: { fontSize: FONTS.bodySmall, fontWeight: 'bold', color: COLORS.white },
  guardInfo: { flex: 1, marginLeft: SIZES.sm },
  guardName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  guardStats: { flexDirection: 'row', alignItems: 'center', gap: SIZES.xs, marginTop: 2 },
  guardStat: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  guardRating: { flexDirection: 'row', alignItems: 'center', gap: 2 },
  guardRatingText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  actionsGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: SIZES.sm },
  actionCard: { width: (width - 48) / 2, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, alignItems: 'center', ...SHADOWS.small },
  actionLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.sm, textAlign: 'center' },
});

export default AnalyticsDashboardScreen;
