import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, RefreshControl, Alert, Share, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { attendanceService } from '../../services/attendanceService';
import { authService } from '../../services/authService';
import { formatTimeIST, formatDateShortIST, formatDayIST } from '../../utils/dateUtils';

interface AttendanceRecord {
  id: string;
  date: string;
  day: string;
  checkIn: string;
  checkOut: string;
  totalHours: string;
  status: 'present' | 'late' | 'absent' | 'half-day' | 'leave';
  site: string;
}

type FilterType = 'all' | 'present' | 'late' | 'absent' | 'leave';

function AttendanceHistoryScreen({ navigation }: any) {
  const [refreshing, setRefreshing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [selectedMonth, setSelectedMonth] = useState(new Date().toLocaleDateString('en-US', { month: 'long', year: 'numeric' }));
  const [selectedFilter, setSelectedFilter] = useState<FilterType>('all');
  const [attendanceRecords, setAttendanceRecords] = useState<AttendanceRecord[]>([]);

  const loadAttendance = useCallback(async () => {
    try {
      setLoading(true);
      const user = await authService.getStoredUser();
      if (!user) {
        setAttendanceRecords([]);
        return;
      }
      const guardId = (user as { guardId?: string }).guardId || user.id;
      const end = new Date();
      const start = new Date();
      start.setDate(start.getDate() - 30);
      const result = await attendanceService.getAttendanceList({
        guardId,
        startDate: start.toISOString().split('T')[0],
        endDate: end.toISOString().split('T')[0],
        pageSize: 100,
        sortBy: 'date',
        sortDirection: 'desc',
      });
      if (result.success && result.data != null) {
        const raw = result.data as any;
        const items = Array.isArray(raw) ? raw : (raw?.items ?? raw?.Items ?? raw?.data ?? raw?.Data ?? []);
        const records: AttendanceRecord[] = (items || []).map((a: any, idx: number) => {
          const attDate = a.attendanceDate || a.AttendanceDate;
          const checkIn = a.checkInTime ?? a.CheckInTime;
          const checkOut = a.checkOutTime ?? a.CheckOutTime;
          const siteName = a.siteName ?? a.SiteName ?? '-';
          const statusStr = (a.status ?? a.Status ?? 'Present').toLowerCase();
          let status: AttendanceRecord['status'] = 'present';
          if (statusStr === 'late') status = 'late';
          else if (statusStr === 'absent') status = 'absent';
          else if (statusStr === 'halfday' || statusStr === 'half-day') status = 'half-day';
          else if (statusStr === 'leave') status = 'leave';
          else status = 'present';
          let totalHours = '-';
          if (checkIn && checkOut) {
            const cin = new Date(checkIn).getTime();
            const cout = new Date(checkOut).getTime();
            const mins = Math.round((cout - cin) / 60000);
            totalHours = `${Math.floor(mins / 60)}h ${mins % 60}m`;
          }
          return {
            id: String(a.id ?? a.Id ?? idx),
            date: attDate ? formatDateShortIST(attDate) : '-',
            day: attDate ? formatDayIST(attDate) : '-',
            checkIn: formatTimeIST(checkIn),
            checkOut: formatTimeIST(checkOut),
            totalHours,
            status,
            site: siteName,
          };
        });
        setAttendanceRecords(records);
      } else {
        setAttendanceRecords([]);
      }
    } catch (_) {
      setAttendanceRecords([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadAttendance();
  }, [loadAttendance]);

  const onRefresh = useCallback(async () => {
    setRefreshing(true);
    await loadAttendance();
    setRefreshing(false);
  }, [loadAttendance]);

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'present': return COLORS.success;
      case 'late': return COLORS.warning;
      case 'absent': return COLORS.error;
      case 'half-day': return COLORS.info;
      case 'leave': return COLORS.secondary;
      default: return COLORS.gray500;
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'present': return 'Present';
      case 'late': return 'Late';
      case 'absent': return 'Absent';
      case 'half-day': return 'Half Day';
      case 'leave': return 'On Leave';
      default: return status;
    }
  };

  const filteredRecords = selectedFilter === 'all' 
    ? attendanceRecords 
    : attendanceRecords.filter(r => r.status === selectedFilter);

  const handleRecordPress = (record: AttendanceRecord) => {
    Alert.alert(
      `Attendance - ${record.date}`,
      `Date: ${record.date} (${record.day})\nSite: ${record.site}\n\nCheck-in: ${record.checkIn}\nCheck-out: ${record.checkOut}\nTotal Hours: ${record.totalHours}\n\nStatus: ${getStatusText(record.status)}`,
      [
        { text: 'Close', style: 'cancel' },
        ...(record.status !== 'present' ? [{
          text: 'Request Correction',
          onPress: () => handleRequestCorrection(record),
        }] : []),
      ]
    );
  };

  const handleRequestCorrection = (record: AttendanceRecord) => {
    Alert.alert(
      'Request Correction',
      `Submit a correction request for ${record.date}?\n\nYour supervisor will be notified and can approve the correction.`,
      [
        { text: 'Cancel', style: 'cancel' },
        { 
          text: 'Submit Request', 
          onPress: () => Alert.alert('Success', 'Correction request submitted successfully. Your supervisor will review it shortly.')
        },
      ]
    );
  };

  const handleExportReport = () => {
    Alert.alert(
      'Export Report',
      'Choose export format for your attendance report',
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'PDF Report', onPress: () => generateReport('pdf') },
        { text: 'Excel Sheet', onPress: () => generateReport('excel') },
        { text: 'Share via Email', onPress: () => shareReport() },
      ]
    );
  };

  const generateReport = (format: string) => {
    Alert.alert('Generating Report', `Your ${format.toUpperCase()} report is being generated...\n\nYou will be notified when it's ready for download.`);
  };

  const shareReport = async () => {
    const presentDays = attendanceRecords.filter(r => r.status === 'present').length;
    const lateDays = attendanceRecords.filter(r => r.status === 'late').length;
    const absentDays = attendanceRecords.filter(r => r.status === 'absent').length;
    const total = attendanceRecords.length;
    const rate = total > 0 ? Math.round((presentDays + lateDays) / total * 100) : 0;
    try {
      await Share.share({
        message: `Attendance Report - ${selectedMonth}\n\nSummary:\n✅ Present: ${presentDays} days\n⚠️ Late: ${lateDays} days\n❌ Absent: ${absentDays} days\n\nTotal Records: ${total}\nAttendance Rate: ${rate}%`,
        title: 'Attendance Report',
      });
    } catch (error) {
      Alert.alert('Error', 'Unable to share report');
    }
  };

  const handleFilterAttendance = () => {
    Alert.alert(
      'Filter Attendance',
      'Show records for:',
      [
        { text: 'All Records', onPress: () => setSelectedFilter('all') },
        { text: 'Present Only', onPress: () => setSelectedFilter('present') },
        { text: 'Late Only', onPress: () => setSelectedFilter('late') },
        { text: 'Absent Only', onPress: () => setSelectedFilter('absent') },
        { text: 'Leave Only', onPress: () => setSelectedFilter('leave') },
        { text: 'Cancel', style: 'cancel' },
      ]
    );
  };

  const presentDays = attendanceRecords.filter(r => r.status === 'present').length;
  const lateDays = attendanceRecords.filter(r => r.status === 'late').length;
  const absentDays = attendanceRecords.filter(r => r.status === 'absent').length;
  const leaveDays = attendanceRecords.filter(r => r.status === 'leave').length;
  const totalForRate = attendanceRecords.length;
  const attendanceRatePct = totalForRate > 0 ? Math.round((presentDays + lateDays) / totalForRate * 100) : 0;

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Attendance History</Text>
          <View style={styles.exportButton} />
        </View>
        <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={{ marginTop: SIZES.md, color: COLORS.textSecondary }}>Loading attendance...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Attendance History</Text>
        <TouchableOpacity style={styles.exportButton} onPress={handleExportReport}>
          <MaterialCommunityIcons name="download" size={24} color={COLORS.primary} />
        </TouchableOpacity>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
        contentContainerStyle={styles.content}
      >
        {/* Month Selector */}
        <Card style={styles.monthCard}>
          <TouchableOpacity style={styles.monthSelector}>
            <MaterialCommunityIcons name="chevron-left" size={24} color={COLORS.primary} />
            <Text style={styles.monthText}>{selectedMonth}</Text>
            <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.primary} />
          </TouchableOpacity>
        </Card>

        {/* Stats Summary */}
        <View style={styles.statsContainer}>
          <Card style={[styles.statCard, { backgroundColor: COLORS.success + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.success }]}>{presentDays}</Text>
            <Text style={styles.statLabel}>Present</Text>
          </Card>
          <Card style={[styles.statCard, { backgroundColor: COLORS.warning + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.warning }]}>{lateDays}</Text>
            <Text style={styles.statLabel}>Late</Text>
          </Card>
          <Card style={[styles.statCard, { backgroundColor: COLORS.error + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.error }]}>{absentDays}</Text>
            <Text style={styles.statLabel}>Absent</Text>
          </Card>
          <Card style={[styles.statCard, { backgroundColor: COLORS.secondary + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.secondary }]}>{leaveDays}</Text>
            <Text style={styles.statLabel}>Leave</Text>
          </Card>
        </View>

        {/* Quick Actions */}
        <Card style={styles.quickActionsCard}>
          <Text style={styles.quickActionsTitle}>Quick Actions</Text>
          <View style={styles.actionsGrid}>
            <TouchableOpacity style={styles.actionCard} onPress={handleExportReport}>
              <MaterialCommunityIcons name="file-export" size={24} color={COLORS.primary} />
              <Text style={styles.actionLabel}>Export</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={handleFilterAttendance}>
              <MaterialCommunityIcons name="filter-variant" size={24} color={COLORS.secondary} />
              <Text style={styles.actionLabel}>Filter</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={() => Alert.alert('Calendar View', 'Calendar view shows your attendance in a monthly calendar format.')}>
              <MaterialCommunityIcons name="calendar" size={24} color={COLORS.warning} />
              <Text style={styles.actionLabel}>Calendar</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={shareReport}>
              <MaterialCommunityIcons name="chart-bar" size={24} color={COLORS.info} />
              <Text style={styles.actionLabel}>Statistics</Text>
            </TouchableOpacity>
          </View>
        </Card>

        {/* Filter Badge */}
        {selectedFilter !== 'all' && (
          <View style={styles.filterBadge}>
            <Text style={styles.filterBadgeText}>Showing: {getStatusText(selectedFilter)}</Text>
            <TouchableOpacity onPress={() => setSelectedFilter('all')}>
              <MaterialCommunityIcons name="close-circle" size={20} color={COLORS.primary} />
            </TouchableOpacity>
          </View>
        )}

        {/* Attendance Records */}
        <Text style={styles.sectionTitle}>Daily Records</Text>
        {filteredRecords.length === 0 ? (
          <Card style={styles.recordCard}>
            <Text style={styles.dayText}>No attendance records found for this period.</Text>
          </Card>
        ) : filteredRecords.map((record) => (
          <TouchableOpacity
            key={record.id}
            style={styles.recordCard}
            onPress={() => handleRecordPress(record)}
          >
            <View style={styles.dateContainer}>
              <Text style={styles.dateText}>{record.date}</Text>
              <Text style={styles.dayText}>{record.day}</Text>
            </View>
            <View style={styles.recordDetails}>
              <View style={styles.timeRow}>
                <View style={styles.timeItem}>
                  <MaterialCommunityIcons name="login" size={16} color={COLORS.success} />
                  <Text style={styles.timeText}>{record.checkIn}</Text>
                </View>
                <View style={styles.timeItem}>
                  <MaterialCommunityIcons name="logout" size={16} color={COLORS.error} />
                  <Text style={styles.timeText}>{record.checkOut}</Text>
                </View>
              </View>
              <Text style={styles.hoursText}>{record.totalHours}</Text>
            </View>
            <View style={[styles.statusBadge, { backgroundColor: getStatusColor(record.status) + '15' }]}>
              <Text style={[styles.statusText, { color: getStatusColor(record.status) }]}>
                {getStatusText(record.status)}
              </Text>
            </View>
          </TouchableOpacity>
        ))}

        {/* Attendance Rate */}
        <Card style={styles.rateCard}>
          <View style={styles.rateHeader}>
            <MaterialCommunityIcons name="chart-donut" size={24} color={COLORS.primary} />
            <Text style={styles.rateTitle}>Attendance Rate (last 30 days)</Text>
          </View>
          <View style={styles.rateBar}>
            <View style={[styles.rateProgress, { width: `${Math.min(100, attendanceRatePct)}%` }]} />
          </View>
          <Text style={styles.rateText}>
            {attendanceRatePct}% - {presentDays + lateDays} out of {totalForRate} records
          </Text>
        </Card>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  exportButton: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center' },
  content: { padding: SIZES.md },
  monthCard: { marginBottom: SIZES.md, padding: SIZES.sm },
  monthSelector: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  monthText: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  statsContainer: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: SIZES.md },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.sm, marginHorizontal: 2 },
  statNumber: { fontSize: FONTS.h3, fontWeight: 'bold' },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 2 },
  quickActionsCard: { marginBottom: SIZES.md, padding: SIZES.md },
  quickActionsTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  actionsGrid: { flexDirection: 'row', justifyContent: 'space-around' },
  actionCard: { alignItems: 'center', padding: SIZES.sm },
  actionLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
  filterBadge: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', backgroundColor: COLORS.primary + '10', paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusSm, marginBottom: SIZES.md },
  filterBadgeText: { fontSize: FONTS.bodySmall, color: COLORS.primary, fontWeight: '500' },
  sectionTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  recordCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  dateContainer: { alignItems: 'center', marginRight: SIZES.md, minWidth: 50 },
  dateText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  dayText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  recordDetails: { flex: 1 },
  timeRow: { flexDirection: 'row', marginBottom: SIZES.xs },
  timeItem: { flexDirection: 'row', alignItems: 'center', marginRight: SIZES.md },
  timeText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginLeft: 4 },
  hoursText: { fontSize: FONTS.caption, color: COLORS.gray400 },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  statusText: { fontSize: FONTS.caption, fontWeight: '600' },
  rateCard: { padding: SIZES.md, marginBottom: SIZES.xxl },
  rateHeader: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.sm },
  rateTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginLeft: SIZES.sm },
  rateBar: { height: 8, backgroundColor: COLORS.gray200, borderRadius: 4, overflow: 'hidden', marginBottom: SIZES.sm },
  rateProgress: { height: '100%', backgroundColor: COLORS.primary, borderRadius: 4 },
  rateText: { fontSize: FONTS.caption, color: COLORS.textSecondary, textAlign: 'center' },
});

export default AttendanceHistoryScreen;
