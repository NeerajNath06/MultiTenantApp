import React, { useState, useEffect, useCallback } from 'react';
import { 
  View, 
  Text, 
  StyleSheet, 
  ScrollView, 
  TouchableOpacity, 
  RefreshControl
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { authService } from '../../services/authService';
import { guardService } from '../../services/guardService';
import { siteService } from '../../services/siteService';
import { attendanceService } from '../../services/attendanceService';
import { incidentService } from '../../services/incidentService';
import { deploymentService } from '../../services/deploymentService';
import { leaveService } from '../../services/leaveService';

// Simple inline Card component to avoid import issues
const SimpleCard = ({ children, style }: { children: React.ReactNode; style?: any }) => (
  <View style={[simpleCardStyles.card, style]}>{children}</View>
);

const simpleCardStyles = StyleSheet.create({
  card: {
    backgroundColor: '#FFFFFF',
    borderRadius: 12,
    padding: 16,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 2,
  },
});

interface KPIMetric {
  id: string;
  title: string;
  value: string;
  change: string;
  changeType: 'increase' | 'decrease' | 'neutral';
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
  color: string;
}

interface AlertItem {
  id: string;
  type: 'critical' | 'warning' | 'info';
  title: string;
  message: string;
  timestamp: string;
  location: string;
}

interface GuardActivity {
  id: string;
  name: string;
  site: string;
  status: 'on_duty' | 'break' | 'off_duty' | 'late';
  lastCheckIn: string;
  location: string;
  avatar: string;
}

function SupervisorDashboardScreen({ navigation }: any) {
  const [refreshing, setRefreshing] = useState(false);
  const [selectedTimeframe, setSelectedTimeframe] = useState<'today' | 'week' | 'month'>('today');
  const [assignedGuardsCount, setAssignedGuardsCount] = useState<number | null>(null);
  const [assignedGuardsList, setAssignedGuardsList] = useState<GuardActivity[]>([]);
  const [alerts, setAlerts] = useState<AlertItem[]>([]);
  const [sitesCount, setSitesCount] = useState<number | null>(null);
  const [attendanceTodayCount, setAttendanceTodayCount] = useState<number | null>(null);
  const [incidentsPendingCount, setIncidentsPendingCount] = useState<number | null>(null);
  const [incidentsTodayCount, setIncidentsTodayCount] = useState<number | null>(null);
  const [deploymentsCount, setDeploymentsCount] = useState<number | null>(null);
  const [pendingApprovalsCount, setPendingApprovalsCount] = useState<number>(0);

  const loadSupervisorData = useCallback(async () => {
    const user = await authService.getStoredUser();
    if (!user?.isSupervisor) return;
    const supervisorId = user.id;

    const today = new Date();
    const todayStr = today.toISOString().split('T')[0];

    const [guardsRes, sitesRes, attendanceRes, incidentsRes, deploymentsRes, leaveRes] = await Promise.all([
      guardService.getGuards({ supervisorId, pageSize: 50 }),
      siteService.getSites({ supervisorId, pageSize: 100 }),
      attendanceService.getAttendanceList({ startDate: todayStr, endDate: todayStr, pageNumber: 1, pageSize: 1 }),
      incidentService.getIncidents({ pageNumber: 1, pageSize: 100 }),
      deploymentService.getDeployments({ pageNumber: 1, pageSize: 100 }),
      leaveService.getLeaveRequests({ status: 'pending', pageNumber: 1, pageSize: 1 }),
    ]);

    if (guardsRes.success && guardsRes.data) {
      setAssignedGuardsCount(guardsRes.data.totalCount);
      const list = (guardsRes.data.items || []).slice(0, 8).map((g: any) => ({
        id: String(g.id ?? g.Id),
        name: `${g.firstName ?? g.FirstName ?? ''} ${g.lastName ?? g.LastName ?? ''}`.trim() || (g.guardCode ?? g.GuardCode ?? '—'),
        site: g.siteName ?? g.SiteName ?? '—',
        status: 'on_duty' as const,
        lastCheckIn: '—',
        location: '—',
        avatar: ((g.firstName ?? g.FirstName ?? '')?.charAt(0) || '') + ((g.lastName ?? g.LastName ?? '')?.charAt(0) || '') || 'G',
      }));
      setAssignedGuardsList(list);
    }

    if (sitesRes.success && sitesRes.data) {
      const raw = sitesRes.data as { items?: any[]; Items?: any[]; totalCount?: number; TotalCount?: number };
      const items = raw?.items ?? raw?.Items ?? (Array.isArray(raw) ? raw : []);
      const total = typeof raw?.totalCount === 'number' ? raw.totalCount : (typeof (raw as any)?.TotalCount === 'number' ? (raw as any).TotalCount : (Array.isArray(items) ? items.length : 0));
      setSitesCount(total);
    }

    if (attendanceRes.success && attendanceRes.data) {
      const raw = attendanceRes.data as { items?: any[]; Items?: any[]; totalCount?: number; TotalCount?: number };
      const total = raw?.totalCount ?? (raw as any)?.TotalCount ?? (raw?.items ?? (raw as any)?.Items ?? []).length;
      setAttendanceTodayCount(typeof total === 'number' ? total : 0);
    }

    if (incidentsRes.success && incidentsRes.data) {
      const raw = incidentsRes.data as { items?: any[]; Items?: any[]; totalCount?: number; TotalCount?: number };
      const list = raw?.items ?? (raw as any)?.Items ?? (Array.isArray(raw) ? raw : []);
      const arr = Array.isArray(list) ? list : [];
      const pending = arr.filter((i: any) => (i.status ?? i.Status ?? '').toString().toLowerCase() === 'pending').length;
      const todayIncidents = arr.filter((i: any) => {
        const d = i.reportedAt ?? i.ReportedAt ?? i.createdDate ?? i.CreatedDate;
        if (!d) return false;
        const dateStr = typeof d === 'string' ? d.split('T')[0] : '';
        return dateStr === todayStr;
      }).length;
      setIncidentsPendingCount(pending);
      setIncidentsTodayCount(todayIncidents);
    }

    if (deploymentsRes.success && deploymentsRes.data) {
      const list = Array.isArray(deploymentsRes.data) ? deploymentsRes.data : (deploymentsRes.data?.items ?? deploymentsRes.data?.Items ?? []);
      setDeploymentsCount(list.length);
    }

    if (leaveRes.success && leaveRes.data) {
      const raw = leaveRes.data as { totalCount?: number; TotalCount?: number; items?: any[] };
      const total = raw?.totalCount ?? raw?.TotalCount ?? (Array.isArray(raw?.items) ? raw.items.length : 0);
      setPendingApprovalsCount(typeof total === 'number' ? total : 0);
    }

    setAlerts([]);
  }, []);

  useEffect(() => {
    loadSupervisorData();
  }, [loadSupervisorData]);

  const totalGuardsValue = assignedGuardsCount !== null ? String(assignedGuardsCount) : '—';
  const activeSitesValue = sitesCount !== null ? String(sitesCount) : '—';
  const attendanceRateValue = (assignedGuardsCount != null && assignedGuardsCount > 0 && attendanceTodayCount != null)
    ? `${Math.round((attendanceTodayCount / assignedGuardsCount) * 100)}%`
    : '—';
  const incidentsTodayValue = incidentsTodayCount !== null ? String(incidentsTodayCount) : '—';

  const kpiMetrics: KPIMetric[] = [
    { id: '1', title: 'Assigned Guards', value: totalGuardsValue, change: '', changeType: 'neutral', icon: 'account-group', color: COLORS.primary },
    { id: '2', title: 'Active Sites', value: activeSitesValue, change: '', changeType: 'neutral', icon: 'office-building', color: COLORS.success },
    { id: '3', title: 'Attendance Rate', value: attendanceRateValue, change: '', changeType: 'neutral', icon: 'check-circle', color: COLORS.info },
    { id: '4', title: 'Security Score', value: '—', change: '', changeType: 'neutral', icon: 'shield-check', color: COLORS.warning },
    { id: '5', title: 'Incidents Today', value: incidentsTodayValue, change: '', changeType: 'neutral', icon: 'alert-circle', color: COLORS.error },
    { id: '6', title: 'Response Time', value: '—', change: '', changeType: 'neutral', icon: 'clock-outline', color: COLORS.secondary }
  ];

  const onRefresh = useCallback(async () => {
    setRefreshing(true);
    await loadSupervisorData();
    setTimeout(() => setRefreshing(false), 500);
  }, [loadSupervisorData]);

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'on_duty': return COLORS.success;
      case 'break': return COLORS.warning;
      case 'late': return COLORS.error;
      case 'off_duty': return COLORS.gray400;
      default: return COLORS.gray500;
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'on_duty': return 'On Duty';
      case 'break': return 'Break';
      case 'late': return 'Late';
      case 'off_duty': return 'Off Duty';
      default: return 'Unknown';
    }
  };

  const getAlertColor = (type: string) => {
    switch (type) {
      case 'critical': return COLORS.error;
      case 'warning': return COLORS.warning;
      case 'info': return COLORS.info;
      default: return COLORS.gray500;
    }
  };

  const getAlertIcon = (type: string): keyof typeof MaterialCommunityIcons.glyphMap => {
    switch (type) {
      case 'critical': return 'alert';
      case 'warning': return 'alert-circle';
      case 'info': return 'information';
      default: return 'help-circle';
    }
  };

  const handleNavigateToScreen = (screenName: string) => {
    navigation.navigate(screenName);
  };

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
        showsVerticalScrollIndicator={false}
        keyboardShouldPersistTaps="handled"
      >
        {/* Header */}
        <View style={styles.header}>
          <View style={styles.headerTop}>
            <View>
              <Text style={styles.greeting}>Security Command Center</Text>
              <Text style={styles.userName}>Supervisor Dashboard</Text>
              <Text style={styles.subtitle}>Real-time monitoring</Text>
            </View>
            <View style={styles.headerActions}>
              <TouchableOpacity style={styles.notificationBtn}>
                <MaterialCommunityIcons name="bell" size={24} color={COLORS.white} />
                <View style={styles.notificationBadge}><Text style={styles.badgeText}>7</Text></View>
              </TouchableOpacity>
              <TouchableOpacity style={styles.settingsBtn}>
                <MaterialCommunityIcons name="cog" size={24} color={COLORS.white} />
              </TouchableOpacity>
            </View>
          </View>
          
          {/* Time Filter */}
          <View style={styles.timeFilter}>
            {(['today', 'week', 'month'] as const).map((timeframe) => (
              <TouchableOpacity
                key={timeframe}
                style={[styles.timeFilterBtn, selectedTimeframe === timeframe && styles.timeFilterBtnActive]}
                onPress={() => setSelectedTimeframe(timeframe)}
              >
                <Text style={[styles.timeFilterText, selectedTimeframe === timeframe && styles.timeFilterTextActive]}>
                  {timeframe.charAt(0).toUpperCase() + timeframe.slice(1)}
                </Text>
              </TouchableOpacity>
            ))}
          </View>
        </View>

        {/* KPIs */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Key Performance Indicators</Text>
            <TouchableOpacity><Text style={styles.viewAll}>View Report</Text></TouchableOpacity>
          </View>
          
          <View style={styles.kpiGrid}>
            {kpiMetrics.map((metric) => (
              <View key={metric.id} style={styles.kpiCard}>
                <SimpleCard style={styles.kpiContent}>
                  <View style={styles.kpiHeader}>
                    <View style={[styles.kpiIcon, { backgroundColor: metric.color + '15' }]}>
                      <MaterialCommunityIcons name={metric.icon} size={20} color={metric.color} />
                    </View>
                    <Text style={[styles.changeText, { color: metric.changeType === 'increase' ? COLORS.success : metric.changeType === 'decrease' ? COLORS.error : COLORS.gray500 }]}>
                      {metric.change}
                    </Text>
                  </View>
                  <Text style={styles.kpiValue}>{metric.value}</Text>
                  <Text style={styles.kpiTitle}>{metric.title}</Text>
                </SimpleCard>
              </View>
            ))}
          </View>
        </View>

        {/* Critical Alerts */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Critical Alerts</Text>
          </View>
          <SimpleCard style={styles.alertsCard}>
            {alerts.length === 0 ? (
              <Text style={styles.emptyAlertsText}>No critical alerts</Text>
            ) : (
              alerts.map((alert, index) => (
                <View key={alert.id} style={[styles.alertItem, index !== alerts.length - 1 && styles.alertItemBorder]}>
                  <View style={[styles.alertIcon, { backgroundColor: getAlertColor(alert.type) + '15' }]}>
                    <MaterialCommunityIcons name={getAlertIcon(alert.type)} size={20} color={getAlertColor(alert.type)} />
                  </View>
                  <View style={styles.alertContent}>
                    <Text style={styles.alertTitle}>{alert.title}</Text>
                    <Text style={styles.alertMessage}>{alert.message}</Text>
                    <Text style={styles.alertMeta}>{alert.timestamp} - {alert.location}</Text>
                  </View>
                  <TouchableOpacity style={styles.alertAction}>
                    <MaterialCommunityIcons name="chevron-right" size={20} color={COLORS.gray400} />
                  </TouchableOpacity>
                </View>
              ))
            )}
          </SimpleCard>
        </View>

        {/* Quick Actions */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Quick Actions</Text>
          <View style={styles.actionsGrid}>
            <TouchableOpacity style={styles.actionCard} onPress={() => handleNavigateToScreen('SiteWiseGuardList')}>
              <View style={[styles.actionIcon, { backgroundColor: COLORS.primary + '15' }]}>
                <MaterialCommunityIcons name="account-group" size={24} color={COLORS.primary} />
              </View>
              <Text style={styles.actionTitle}>Guard Management</Text>
              <Text style={styles.actionSubtitle}>{assignedGuardsCount != null ? `${assignedGuardsCount} assigned guards` : 'View guards'}</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={() => handleNavigateToScreen('LiveAttendance')}>
              <View style={[styles.actionIcon, { backgroundColor: COLORS.success + '15' }]}>
                <MaterialCommunityIcons name="eye" size={24} color={COLORS.success} />
              </View>
              <Text style={styles.actionTitle}>Live Monitoring</Text>
              <Text style={styles.actionSubtitle}>Real-time tracking</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={() => handleNavigateToScreen('IncidentReview')}>
              <View style={[styles.actionIcon, { backgroundColor: COLORS.warning + '15' }]}>
                <MaterialCommunityIcons name="file-document-outline" size={24} color={COLORS.warning} />
              </View>
              <Text style={styles.actionTitle}>Incident Review</Text>
              <Text style={styles.actionSubtitle}>
                {incidentsPendingCount != null ? `${incidentsPendingCount} pending review${incidentsPendingCount !== 1 ? 's' : ''}` : 'View incidents'}
              </Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={() => handleNavigateToScreen('AttendanceApproval')}>
              <View style={[styles.actionIcon, { backgroundColor: COLORS.info + '15' }]}>
                <MaterialCommunityIcons name="check-all" size={24} color={COLORS.info} />
              </View>
              <Text style={styles.actionTitle}>Approvals</Text>
              <Text style={styles.actionSubtitle}>
                {pendingApprovalsCount > 0 ? `${pendingApprovalsCount} pending requests` : 'Attendance & leave approvals'}
              </Text>
            </TouchableOpacity>
          </View>
        </View>

        {/* Management Features – Tiles */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Management Features</Text>
          <View style={styles.managementTilesGrid}>
            <TouchableOpacity style={styles.managementTile} onPress={() => handleNavigateToScreen('RosterManagement')}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.primaryBlue + '15' }]}>
                <MaterialCommunityIcons name="calendar-month" size={24} color={COLORS.primaryBlue} />
              </View>
              <Text style={styles.managementTileTitle}>Roster Management</Text>
              <Text style={styles.managementTileSubtitle}>
                {deploymentsCount != null ? `${deploymentsCount} assignments` : 'Guard schedules'}
              </Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.managementTile} onPress={() => handleNavigateToScreen('GuardMap')}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.success + '15' }]}>
                <MaterialCommunityIcons name="map-marker-radius" size={24} color={COLORS.success} />
              </View>
              <Text style={styles.managementTileTitle}>Live Guard Map</Text>
              <Text style={styles.managementTileSubtitle}>Track guard locations</Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.managementTile} onPress={() => handleNavigateToScreen('SiteManagement')}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.warning + '15' }]}>
                <MaterialCommunityIcons name="office-building" size={24} color={COLORS.warning} />
              </View>
              <Text style={styles.managementTileTitle}>Site Management</Text>
              <Text style={styles.managementTileSubtitle}>
                {sitesCount != null ? `${sitesCount} sites` : 'Manage sites'}
              </Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.managementTile} onPress={() => handleNavigateToScreen('GuardReplacement')}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.error + '15' }]}>
                <MaterialCommunityIcons name="account-switch" size={24} color={COLORS.error} />
              </View>
              <Text style={styles.managementTileTitle}>Guard Replacement</Text>
              <Text style={styles.managementTileSubtitle}>Manage replacements</Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.managementTile} onPress={() => handleNavigateToScreen('GuardPerformance')}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.secondary + '15' }]}>
                <MaterialCommunityIcons name="chart-line" size={24} color={COLORS.secondary} />
              </View>
              <Text style={styles.managementTileTitle}>Guard Performance</Text>
              <Text style={styles.managementTileSubtitle}>
                {assignedGuardsCount != null ? `${assignedGuardsCount} guards` : 'Performance metrics'}
              </Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.managementTile} onPress={() => handleNavigateToScreen('PayrollManagement')}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.success + '15' }]}>
                <MaterialCommunityIcons name="cash-multiple" size={24} color={COLORS.success} />
              </View>
              <Text style={styles.managementTileTitle}>Payroll Management</Text>
              <Text style={styles.managementTileSubtitle}>Process salaries</Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.managementTile} onPress={() => handleNavigateToScreen('TrainingAssignment')}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.info + '15' }]}>
                <MaterialCommunityIcons name="school" size={24} color={COLORS.info} />
              </View>
              <Text style={styles.managementTileTitle}>Training Assignment</Text>
              <Text style={styles.managementTileSubtitle}>Assign trainings</Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.managementTile} onPress={() => handleNavigateToScreen('ComplianceDashboard')}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.warning + '15' }]}>
                <MaterialCommunityIcons name="shield-check" size={24} color={COLORS.warning} />
              </View>
              <Text style={styles.managementTileTitle}>Compliance Dashboard</Text>
              <Text style={styles.managementTileSubtitle}>Track compliance</Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.managementTile} onPress={() => handleNavigateToScreen('AssetManagement')}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.primary + '15' }]}>
                <MaterialCommunityIcons name="package-variant" size={24} color={COLORS.primary} />
              </View>
              <Text style={styles.managementTileTitle}>Asset Management</Text>
              <Text style={styles.managementTileSubtitle}>Track equipment</Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.managementTile} onPress={() => navigation.navigate('Announcements', { manageMode: true })}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.warning + '15' }]}>
                <MaterialCommunityIcons name="bullhorn" size={24} color={COLORS.warning} />
              </View>
              <Text style={styles.managementTileTitle}>Announcements</Text>
              <Text style={styles.managementTileSubtitle}>Create & manage announcements</Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.managementTile} onPress={() => handleNavigateToScreen('VisitorAnalytics')}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.secondary + '15' }]}>
                <MaterialCommunityIcons name="account-multiple-check" size={24} color={COLORS.secondary} />
              </View>
              <Text style={styles.managementTileTitle}>Visitor Analytics</Text>
              <Text style={styles.managementTileSubtitle}>Visitor insights</Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.managementTile} onPress={() => handleNavigateToScreen('SiteVehicleLog')}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.warning + '15' }]}>
                <MaterialCommunityIcons name="car-side" size={24} color={COLORS.warning} />
              </View>
              <Text style={styles.managementTileTitle}>Vehicle Log by Site</Text>
              <Text style={styles.managementTileSubtitle}>View vehicle entries per site</Text>
            </TouchableOpacity>

            <TouchableOpacity style={styles.managementTile} onPress={() => handleNavigateToScreen('AnalyticsDashboard')}>
              <View style={[styles.managementTileIcon, { backgroundColor: COLORS.primaryBlue + '15' }]}>
                <MaterialCommunityIcons name="chart-box" size={24} color={COLORS.primaryBlue} />
              </View>
              <Text style={styles.managementTileTitle}>Analytics Dashboard</Text>
              <Text style={styles.managementTileSubtitle}>Reports & charts</Text>
            </TouchableOpacity>
          </View>
        </View>

        {/* My Assigned Guards (real data) */}
        <View style={[styles.section, { marginBottom: 48 }]}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>My Assigned Guards</Text>
            <TouchableOpacity onPress={() => handleNavigateToScreen('SiteWiseGuardList')}><Text style={styles.viewAll}>View All</Text></TouchableOpacity>
          </View>
          <SimpleCard style={styles.activityCard}>
            {assignedGuardsList.length === 0 ? (
              <Text style={styles.emptyAlertsText}>No assigned guards. Assign guards from the web portal.</Text>
            ) : (
              assignedGuardsList.map((guard, index) => (
                <View key={guard.id} style={[styles.guardItem, index !== assignedGuardsList.length - 1 && styles.guardItemBorder]}>
                  <View style={styles.guardAvatarContainer}>
                    <Text style={styles.guardAvatarText}>{guard.avatar}</Text>
                  </View>
                  <View style={styles.guardInfo}>
                    <Text style={styles.guardName}>{guard.name}</Text>
                    <Text style={styles.guardSite}>{guard.site}</Text>
                    <Text style={styles.guardLocation}>{guard.location}</Text>
                  </View>
                  <View style={styles.guardStatus}>
                    <View style={[styles.statusDot, { backgroundColor: getStatusColor(guard.status) }]} />
                    <Text style={[styles.statusTextStyle, { color: getStatusColor(guard.status) }]}>{getStatusText(guard.status)}</Text>
                    <Text style={styles.checkInTime}>{guard.lastCheckIn}</Text>
                  </View>
                </View>
              ))
            )}
          </SimpleCard>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  scrollContent: { flexGrow: 1, paddingBottom: 24 },
  header: { backgroundColor: COLORS.primary, padding: 16, paddingBottom: 24 },
  headerTop: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 16 },
  greeting: { fontSize: 12, color: COLORS.white, opacity: 0.8, marginBottom: 4 },
  userName: { fontSize: 20, fontWeight: 'bold', color: COLORS.white, marginBottom: 2 },
  subtitle: { fontSize: 12, color: COLORS.white, opacity: 0.7 },
  headerActions: { flexDirection: 'row', gap: 8 },
  notificationBtn: { position: 'relative', width: 40, height: 40, justifyContent: 'center', alignItems: 'center' },
  notificationBadge: { position: 'absolute', top: 0, right: 0, backgroundColor: COLORS.error, borderRadius: 10, minWidth: 20, height: 20, justifyContent: 'center', alignItems: 'center' },
  badgeText: { fontSize: 10, color: COLORS.white, fontWeight: 'bold' },
  settingsBtn: { width: 40, height: 40, justifyContent: 'center', alignItems: 'center' },
  timeFilter: { flexDirection: 'row', backgroundColor: 'rgba(255,255,255,0.15)', borderRadius: 12, padding: 4 },
  timeFilterBtn: { flex: 1, paddingVertical: 8, alignItems: 'center', borderRadius: 8 },
  timeFilterBtnActive: { backgroundColor: COLORS.white },
  timeFilterText: { fontSize: 14, color: COLORS.white, fontWeight: '500' },
  timeFilterTextActive: { color: COLORS.primary },
  section: { padding: 16 },
  sectionHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 },
  sectionTitle: { fontSize: 18, fontWeight: '600', color: COLORS.textPrimary },
  viewAll: { fontSize: 14, color: COLORS.primary, fontWeight: '500' },
  kpiGrid: { flexDirection: 'row', flexWrap: 'wrap', justifyContent: 'space-between' },
  kpiCard: { width: '48%', marginBottom: 8 },
  kpiContent: { padding: 16 },
  kpiHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 },
  kpiIcon: { width: 36, height: 36, borderRadius: 18, justifyContent: 'center', alignItems: 'center' },
  changeText: { fontSize: 12, fontWeight: '600' },
  kpiValue: { fontSize: 20, fontWeight: 'bold', color: COLORS.textPrimary, marginBottom: 2 },
  kpiTitle: { fontSize: 12, color: COLORS.textSecondary },
  alertsCard: { padding: 16 },
  alertItem: { flexDirection: 'row', alignItems: 'flex-start', paddingVertical: 8 },
  alertItemBorder: { borderBottomWidth: 1, borderBottomColor: COLORS.gray100 },
  alertIcon: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center', marginRight: 8 },
  alertContent: { flex: 1 },
  alertTitle: { fontSize: 16, fontWeight: '600', color: COLORS.textPrimary, marginBottom: 2 },
  alertMessage: { fontSize: 12, color: COLORS.textSecondary, lineHeight: 16, marginBottom: 4 },
  alertMeta: { fontSize: 10, color: COLORS.gray500 },
  alertAction: { padding: 4 },
  emptyAlertsText: { fontSize: 14, color: COLORS.textSecondary, paddingVertical: 12, textAlign: 'center' },
  actionsGrid: { flexDirection: 'row', flexWrap: 'wrap', justifyContent: 'space-between' },
  actionCard: { width: '48%', backgroundColor: COLORS.white, borderRadius: 12, padding: 16, marginBottom: 8, ...SHADOWS.small },
  actionIcon: { width: 48, height: 48, borderRadius: 24, justifyContent: 'center', alignItems: 'center', marginBottom: 8 },
  actionTitle: { fontSize: 16, fontWeight: '600', color: COLORS.textPrimary, marginBottom: 2 },
  actionSubtitle: { fontSize: 12, color: COLORS.textSecondary },
  activityCard: { padding: 16 },
  guardItem: { flexDirection: 'row', alignItems: 'center', paddingVertical: 8 },
  guardItemBorder: { borderBottomWidth: 1, borderBottomColor: COLORS.gray100 },
  guardAvatarContainer: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center', marginRight: 8 },
  guardAvatarText: { fontSize: 14, fontWeight: 'bold', color: COLORS.primary },
  guardInfo: { flex: 1 },
  guardName: { fontSize: 16, fontWeight: '600', color: COLORS.textPrimary },
  guardSite: { fontSize: 12, color: COLORS.textSecondary },
  guardLocation: { fontSize: 10, color: COLORS.gray500 },
  guardStatus: { alignItems: 'flex-end' },
  statusDot: { width: 8, height: 8, borderRadius: 4, marginBottom: 4 },
  statusTextStyle: { fontSize: 12, fontWeight: '600' },
  checkInTime: { fontSize: 10, color: COLORS.gray500, marginTop: 2 },
  managementTilesGrid: { flexDirection: 'row', flexWrap: 'wrap', justifyContent: 'space-between' },
  managementTile: { width: '48%', backgroundColor: COLORS.white, borderRadius: 12, padding: 16, marginBottom: 12, ...SHADOWS.small },
  managementTileIcon: { width: 48, height: 48, borderRadius: 24, justifyContent: 'center', alignItems: 'center', marginBottom: 10 },
  managementTileTitle: { fontSize: 15, fontWeight: '600', color: COLORS.textPrimary, marginBottom: 2 },
  managementTileSubtitle: { fontSize: 12, color: COLORS.textSecondary },
});

export default SupervisorDashboardScreen;
