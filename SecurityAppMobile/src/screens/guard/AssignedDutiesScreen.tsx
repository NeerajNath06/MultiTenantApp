import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, RefreshControl, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { deploymentService } from '../../services/deploymentService';
import { authService } from '../../services/authService';
import { siteService } from '../../services/siteService';
import { attendanceService } from '../../services/attendanceService';

interface Duty {
  id: number | string;
  title: string;
  location: string;
  time: string;
  date: string;
  status: 'upcoming' | 'in-progress' | 'completed' | 'missed';
  priority: 'high' | 'medium' | 'low';
  description: string;
  checkpoints?: string[];
}

function AssignedDutiesScreen({ navigation }: any) {
  const [refreshing, setRefreshing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [selectedFilter, setSelectedFilter] = useState<'all' | 'today' | 'upcoming' | 'completed'>('all');
  const [duties, setDuties] = useState<Duty[]>([]);
  const [sites, setSites] = useState<any[]>([]);

  useEffect(() => {
    loadDeployments();
  }, []);

  const loadDeployments = async () => {
    try {
      setLoading(true);
      const user = await authService.getStoredUser();
      if (!user) {
        Alert.alert('Error', 'User not found. Please login again.');
        return;
      }

      const guardIdForApi = (user as { guardId?: string }).guardId || user.id;
      const today = new Date();
      const todayStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
      const d30 = new Date(today);
      d30.setDate(d30.getDate() - 30);
      const startStr = `${d30.getFullYear()}-${String(d30.getMonth() + 1).padStart(2, '0')}-${String(d30.getDate()).padStart(2, '0')}`;

      const [deploymentsResult, todayAttRes, listAttRes] = await Promise.all([
        deploymentService.getDeployments({ guardId: guardIdForApi, pageSize: 100, skipCache: true }),
        attendanceService.getGuardAttendance(guardIdForApi, todayStr),
        attendanceService.getAttendanceList({ guardId: guardIdForApi, startDate: startStr, endDate: todayStr, pageSize: 200, sortBy: 'date', sortDirection: 'desc' }),
      ]);

      const rawAtt = todayAttRes.data;
      const todayAttendanceList: any[] = Array.isArray(rawAtt) ? rawAtt : (rawAtt?.items ?? rawAtt?.data ?? []);
      const listRaw = listAttRes.data as any;
      const allAttendanceList: any[] = Array.isArray(listRaw) ? listRaw : (listRaw?.items ?? listRaw?.data ?? []);
      const attendanceByKey = new Map<string, { hasCheckIn: boolean; hasCheckOut: boolean }>();
      allAttendanceList.forEach((a: any) => {
        const dateStr = (a.attendanceDate ?? a.AttendanceDate ?? '').toString().slice(0, 10);
        const aid = String(a.assignmentId ?? a.AssignmentId ?? '');
        if (!dateStr || !aid) return;
        const key = `${dateStr}_${aid}`;
        const hasCheckIn = !!(a.checkInTime ?? a.CheckInTime);
        const hasCheckOut = !!(a.checkOutTime ?? a.CheckOutTime);
        if (!attendanceByKey.has(key)) attendanceByKey.set(key, { hasCheckIn, hasCheckOut });
        else {
          const prev = attendanceByKey.get(key)!;
          attendanceByKey.set(key, { hasCheckIn: prev.hasCheckIn || hasCheckIn, hasCheckOut: prev.hasCheckOut || hasCheckOut });
        }
      });

      if (deploymentsResult.success && deploymentsResult.data) {
        const raw = deploymentsResult.data as { items?: unknown[] } | unknown[];
        const deploymentsData = Array.isArray(raw)
          ? raw
          : (raw?.items ?? (deploymentsResult.data as { data?: unknown[] })?.data ?? []);

        const sitesResult = await siteService.getSites({ pageSize: 100 });
        let sitesData: any[] = [];
        if (sitesResult.success && sitesResult.data) {
          sitesData = Array.isArray(sitesResult.data) ? sitesResult.data : (sitesResult.data.data || []);
          setSites(sitesData);
        }

        const mappedDuties = deploymentsData.map((deployment: any) => {
          const depDateStr = (deployment.deploymentDate ?? deployment.DeploymentDate ?? '').toString().slice(0, 10);
          const startTime = deployment.startTime || '00:00';
          const endTime = deployment.endTime || '23:59';
          const site = sitesData.find((s: any) => (s.id ?? s.Id) === (deployment.siteId ?? deployment.SiteId)) || { siteName: deployment.siteName ?? deployment.SiteName ?? 'Unknown Site' };
          const siteName = site.siteName ?? site.SiteName ?? 'Unknown Location';

          const isToday = depDateStr === todayStr;
          const isPast = depDateStr < todayStr;
          const isFuture = depDateStr > todayStr;

          const assignmentId = deployment.id ?? deployment.Id;
          const key = `${depDateStr}_${assignmentId}`;
          const attState = attendanceByKey.get(key);
          const hasCheckIn = !!attState?.hasCheckIn || !!todayAttendanceList.find((a: any) => String(a.assignmentId ?? a.AssignmentId ?? '') === String(assignmentId))?.checkInTime;
          const hasCheckOut = !!attState?.hasCheckOut || !!todayAttendanceList.find((a: any) => String(a.assignmentId ?? a.AssignmentId ?? '') === String(assignmentId))?.checkOutTime;

          let status: 'upcoming' | 'in-progress' | 'completed' | 'missed' = 'upcoming';
          if (isFuture) {
            status = 'upcoming';
          } else if (isPast) {
            status = attState?.hasCheckIn ? 'completed' : 'missed';
          } else if (isToday) {
            if (hasCheckOut) status = 'completed';
            else if (hasCheckIn) status = 'in-progress';
            else status = 'upcoming';
          }

          return {
            id: deployment.id,
            title: deployment.shiftName || deployment.ShiftName || 'Duty Assignment',
            location: siteName,
            time: `${startTime} - ${endTime}`,
            date: isToday ? 'Today' : (depDateStr ? new Date(depDateStr + 'T12:00:00').toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) : ''),
            status,
            priority: 'medium' as const,
            description: `Deployment at ${siteName}`,
          };
        });

        setDuties(mappedDuties);
      }
    } catch (error) {
      console.error('Error loading deployments:', error);
      Alert.alert('Error', 'Failed to load assigned duties');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const onRefresh = () => {
    setRefreshing(true);
    loadDeployments();
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'completed': return COLORS.success;
      case 'in-progress': return COLORS.primary;
      case 'upcoming': return COLORS.warning;
      case 'missed': return COLORS.error;
      default: return COLORS.gray500;
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'high': return COLORS.error;
      case 'medium': return COLORS.warning;
      case 'low': return COLORS.info;
      default: return COLORS.gray500;
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'completed': return 'Completed';
      case 'in-progress': return 'In Progress';
      case 'upcoming': return 'Upcoming';
      case 'missed': return 'Missed';
      default: return status;
    }
  };

  const handleDutyPress = (duty: Duty) => {
    const actions = [{ text: 'Close', style: 'cancel' as const }];

    if (duty.status === 'upcoming') {
      actions.push({
        text: 'Start Duty',
        style: 'default' as const,
        onPress: () => handleStartDuty(duty),
      } as any);
    }

    if (duty.status === 'in-progress') {
      actions.push({
        text: 'Complete Duty',
        style: 'default' as const,
        onPress: () => handleCompleteDuty(duty),
      } as any);
      
      if (duty.checkpoints) {
        actions.push({
          text: 'View Checkpoints',
          style: 'default' as const,
          onPress: () => handleViewCheckpoints(duty),
        } as any);
      }
    }

    Alert.alert(
      duty.title,
      `📍 ${duty.location}\n⏰ ${duty.time}\n📅 ${duty.date}\n\n${duty.description}\n\nPriority: ${duty.priority.toUpperCase()}\nStatus: ${getStatusText(duty.status)}`,
      actions
    );
  };

  const handleStartDuty = (duty: Duty) => {
    setDuties(duties.map(d => d.id === duty.id ? { ...d, status: 'in-progress' as const } : d));
    Alert.alert('Duty Started', `You have started: ${duty.title}\n\nDon't forget to complete it when finished.`);
  };

  const handleCompleteDuty = (duty: Duty) => {
    Alert.alert(
      'Complete Duty',
      `Are you sure you want to mark "${duty.title}" as completed?`,
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Complete',
          onPress: () => {
            setDuties(duties.map(d => d.id === duty.id ? { ...d, status: 'completed' as const } : d));
            Alert.alert('Success', 'Duty marked as completed!');
          }
        }
      ]
    );
  };

  const handleViewCheckpoints = (duty: Duty) => {
    if (!duty.checkpoints) return;
    
    Alert.alert(
      'Checkpoints',
      duty.checkpoints.map((cp, i) => `${i + 1}. ${cp}`).join('\n'),
      [{ text: 'OK' }]
    );
  };

  const handleMarkAllComplete = () => {
    const inProgressDuties = duties.filter(d => d.status === 'in-progress');
    if (inProgressDuties.length === 0) {
      Alert.alert('No Active Duties', 'There are no in-progress duties to complete.');
      return;
    }

    Alert.alert(
      'Complete All Active Duties',
      `Mark ${inProgressDuties.length} duty(s) as completed?`,
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Complete All',
          onPress: () => {
            setDuties(duties.map(d => d.status === 'in-progress' ? { ...d, status: 'completed' as const } : d));
            Alert.alert('Success', 'All active duties marked as completed!');
          }
        }
      ]
    );
  };

  const handleViewTemplates = () => {
    Alert.alert(
      'Duty Templates',
      'Quick access to common duty types:\n\n• Morning Patrol\n• Gate Duty\n• CCTV Monitoring\n• Visitor Management\n• Emergency Response\n• Night Watch',
      [{ text: 'OK' }]
    );
  };

  const handleViewReports = () => {
    navigation.navigate('AttendanceHistory');
  };

  const handleViewCalendar = () => {
    Alert.alert(
      'Duty Calendar',
      'Your upcoming duties:\n\n📅 Today: 5 duties\n📅 Tomorrow: 1 duty\n📅 This Week: 7 duties\n\nView detailed calendar in the full version.',
      [{ text: 'OK' }]
    );
  };

  const filteredDuties = duties.filter(duty => {
    if (selectedFilter === 'all') return true;
    if (selectedFilter === 'today') return duty.date === 'Today';
    if (selectedFilter === 'upcoming') return duty.status === 'upcoming';
    if (selectedFilter === 'completed') return duty.status === 'completed';
    return true;
  });

  const todayDuties = duties.filter(d => d.date === 'Today').length;
  const completedDuties = duties.filter(d => d.status === 'completed').length;
  const inProgressDuties = duties.filter(d => d.status === 'in-progress').length;

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Assigned Duties</Text>
        <TouchableOpacity style={styles.refreshButton} onPress={onRefresh}>
          <MaterialCommunityIcons name="refresh" size={24} color={COLORS.primary} />
        </TouchableOpacity>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
        contentContainerStyle={styles.content}
      >
        {/* Stats Summary */}
        <View style={styles.statsContainer}>
          <Card style={[styles.statCard, { backgroundColor: COLORS.primary + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.primary }]}>{todayDuties}</Text>
            <Text style={styles.statLabel}>Today</Text>
          </Card>
          <Card style={[styles.statCard, { backgroundColor: COLORS.warning + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.warning }]}>{inProgressDuties}</Text>
            <Text style={styles.statLabel}>Active</Text>
          </Card>
          <Card style={[styles.statCard, { backgroundColor: COLORS.success + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.success }]}>{completedDuties}</Text>
            <Text style={styles.statLabel}>Done</Text>
          </Card>
        </View>

        {/* Quick Actions */}
        <Card style={styles.quickActionsCard}>
          <Text style={styles.quickActionsTitle}>Quick Actions</Text>
          <View style={styles.actionsGrid}>
            <TouchableOpacity style={styles.actionCard} onPress={handleMarkAllComplete}>
              <MaterialCommunityIcons name="check-all" size={24} color={COLORS.primary} />
              <Text style={styles.actionLabel}>Complete All</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={handleViewTemplates}>
              <MaterialCommunityIcons name="content-copy" size={24} color={COLORS.secondary} />
              <Text style={styles.actionLabel}>Templates</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={handleViewReports}>
              <MaterialCommunityIcons name="file-document-outline" size={24} color={COLORS.info} />
              <Text style={styles.actionLabel}>Reports</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={handleViewCalendar}>
              <MaterialCommunityIcons name="calendar" size={24} color={COLORS.warning} />
              <Text style={styles.actionLabel}>Calendar</Text>
            </TouchableOpacity>
          </View>
        </Card>

        {/* Filter Tabs */}
        <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.filterScroll}>
          {(['all', 'today', 'upcoming', 'completed'] as const).map((filter) => (
            <TouchableOpacity
              key={filter}
              style={[styles.filterTab, selectedFilter === filter && styles.filterTabActive]}
              onPress={() => setSelectedFilter(filter)}
            >
              <Text style={[styles.filterTabText, selectedFilter === filter && styles.filterTabTextActive]}>
                {filter.charAt(0).toUpperCase() + filter.slice(1)}
              </Text>
            </TouchableOpacity>
          ))}
        </ScrollView>

        {/* Duties List */}
        <Text style={styles.sectionTitle}>
          {selectedFilter === 'all' ? 'All Duties' : `${selectedFilter.charAt(0).toUpperCase() + selectedFilter.slice(1)} Duties`}
        </Text>
        
        {loading ? (
          <View style={styles.loadingContainer}>
            <ActivityIndicator size="large" color={COLORS.primary} />
            <Text style={styles.loadingText}>Loading duties...</Text>
          </View>
        ) : filteredDuties.length === 0 ? (
          <View style={styles.emptyState}>
            <MaterialCommunityIcons name="clipboard-check-outline" size={64} color={COLORS.gray300} />
            <Text style={styles.emptyTitle}>No duties found</Text>
            <Text style={styles.emptyText}>No {selectedFilter} duties at the moment</Text>
          </View>
        ) : (
          filteredDuties.map((duty) => (
            <TouchableOpacity
              key={duty.id}
              style={[styles.dutyCard, duty.status === 'in-progress' && styles.dutyCardActive]}
              onPress={() => handleDutyPress(duty)}
            >
              <View style={styles.dutyHeader}>
                <View style={[styles.priorityIndicator, { backgroundColor: getPriorityColor(duty.priority) }]} />
                <View style={styles.dutyTitleContainer}>
                  <Text style={styles.dutyTitle}>{duty.title}</Text>
                  <Text style={styles.dutyLocation}>
                    <MaterialCommunityIcons name="map-marker" size={12} color={COLORS.gray400} /> {duty.location}
                  </Text>
                </View>
                <View style={[styles.statusBadge, { backgroundColor: getStatusColor(duty.status) + '15' }]}>
                  <Text style={[styles.statusText, { color: getStatusColor(duty.status) }]}>
                    {getStatusText(duty.status)}
                  </Text>
                </View>
              </View>
              
              <View style={styles.dutyFooter}>
                <View style={styles.dutyTime}>
                  <MaterialCommunityIcons name="clock-outline" size={14} color={COLORS.gray400} />
                  <Text style={styles.dutyTimeText}>{duty.time}</Text>
                </View>
                <View style={styles.dutyDate}>
                  <MaterialCommunityIcons name="calendar" size={14} color={COLORS.gray400} />
                  <Text style={styles.dutyDateText}>{duty.date}</Text>
                </View>
              </View>

              {duty.status === 'in-progress' && duty.checkpoints && (
                <View style={styles.checkpointsBar}>
                  <Text style={styles.checkpointsText}>
                    <MaterialCommunityIcons name="checkbox-marked-circle-outline" size={12} color={COLORS.primary} />
                    {' '}{duty.checkpoints.length} checkpoints
                  </Text>
                </View>
              )}
            </TouchableOpacity>
          ))
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  refreshButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  content: { padding: SIZES.md },
  statsContainer: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: SIZES.md },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.md, marginHorizontal: SIZES.xs },
  statNumber: { fontSize: FONTS.h2, fontWeight: 'bold' },
  statLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
  quickActionsCard: { marginBottom: SIZES.md, padding: SIZES.md },
  quickActionsTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  actionsGrid: { flexDirection: 'row', justifyContent: 'space-around' },
  actionCard: { alignItems: 'center', padding: SIZES.sm },
  actionLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
  filterScroll: { marginBottom: SIZES.md },
  filterTab: { paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.white, borderRadius: SIZES.radiusFull, marginRight: SIZES.sm, ...SHADOWS.small },
  filterTabActive: { backgroundColor: COLORS.primary },
  filterTabText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, fontWeight: '500' },
  filterTabTextActive: { color: COLORS.white },
  sectionTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  dutyCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  dutyCardActive: { borderLeftWidth: 4, borderLeftColor: COLORS.primary },
  dutyHeader: { flexDirection: 'row', alignItems: 'flex-start', marginBottom: SIZES.sm },
  priorityIndicator: { width: 4, height: 40, borderRadius: 2, marginRight: SIZES.sm },
  dutyTitleContainer: { flex: 1 },
  dutyTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  dutyLocation: { fontSize: FONTS.caption, color: COLORS.gray500, marginTop: 2 },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  statusText: { fontSize: FONTS.caption, fontWeight: '600' },
  dutyFooter: { flexDirection: 'row', justifyContent: 'space-between' },
  dutyTime: { flexDirection: 'row', alignItems: 'center' },
  dutyTimeText: { fontSize: FONTS.caption, color: COLORS.gray500, marginLeft: 4 },
  dutyDate: { flexDirection: 'row', alignItems: 'center' },
  dutyDateText: { fontSize: FONTS.caption, color: COLORS.gray500, marginLeft: 4 },
  checkpointsBar: { marginTop: SIZES.sm, paddingTop: SIZES.sm, borderTopWidth: 1, borderTopColor: COLORS.gray100 },
  checkpointsText: { fontSize: FONTS.caption, color: COLORS.primary },
  emptyState: { alignItems: 'center', justifyContent: 'center', paddingVertical: SIZES.xxl },
  emptyTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginTop: SIZES.md },
  emptyText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginTop: SIZES.xs },
  loadingContainer: { alignItems: 'center', justifyContent: 'center', paddingVertical: SIZES.xxl },
  loadingText: { fontSize: FONTS.body, color: COLORS.textSecondary, marginTop: SIZES.md },
});

export default AssignedDutiesScreen;
