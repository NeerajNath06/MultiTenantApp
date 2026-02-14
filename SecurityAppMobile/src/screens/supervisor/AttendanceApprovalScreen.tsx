import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Alert, FlatList, RefreshControl, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { AttendanceApprovalScreenProps } from '../../types/navigation';
import { leaveService } from '../../services/leaveService';

interface AttendanceRequest {
  id: string;
  guardName: string;
  guardId: string;
  site: string;
  requestType: 'late_checkin' | 'early_checkout' | 'leave_request' | 'overtime';
  date: string;
  time: string;
  reason: string;
  status: 'pending' | 'approved' | 'rejected';
  submittedAt: string;
}

const AttendanceApprovalScreen: React.FC<AttendanceApprovalScreenProps> = ({ navigation }) => {
  const [requests, setRequests] = useState<AttendanceRequest[]>([]);
  const [filter, setFilter] = useState<'all' | 'pending' | 'approved' | 'rejected'>('pending');
  const [refreshing, setRefreshing] = useState(false);
  const [loading, setLoading] = useState(true);

  const loadRequests = useCallback(async () => {
    const res = await leaveService.getLeaveRequests({ pageNumber: 1, pageSize: 100 });
    if (res.success && res.data) {
      const raw = res.data as { items?: any[]; Items?: any[] };
      const list = raw?.items ?? raw?.Items ?? (Array.isArray(res.data) ? res.data : []);
      const mapped: AttendanceRequest[] = (list as any[]).map((r: any) => {
        const status = (r.status ?? r.Status ?? 'Pending').toString().toLowerCase();
        const reqStatus = status === 'approved' ? 'approved' : status === 'rejected' ? 'rejected' : 'pending';
        const startDate = (r.startDate ?? r.StartDate ?? '').toString().split('T')[0];
        const endDate = (r.endDate ?? r.EndDate ?? '').toString().split('T')[0];
        const timeStr = startDate === endDate ? 'Full Day' : `${startDate} to ${endDate}`;
        const submittedAt = (r.createdDate ?? r.CreatedDate ?? r.submittedAt ?? '').toString();
        const submittedTime = submittedAt ? new Date(submittedAt).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true }) : '—';
        return {
          id: String(r.id ?? r.Id),
          guardName: (r.guardName ?? r.GuardName ?? '—').toString(),
          guardId: (r.guardCode ?? r.GuardCode ?? r.guardId ?? r.GuardId ?? '—').toString(),
          site: (r.siteName ?? r.SiteName ?? '—').toString(),
          requestType: 'leave_request',
          date: startDate || '—',
          time: timeStr,
          reason: (r.reason ?? r.Reason ?? '—').toString(),
          status: reqStatus,
          submittedAt: submittedTime,
        };
      });
      setRequests(mapped);
    } else {
      setRequests([]);
    }
    setLoading(false);
    setRefreshing(false);
  }, []);

  useEffect(() => {
    loadRequests();
  }, [loadRequests]);

  const filteredRequests = requests.filter(request => filter === 'all' || request.status === filter);

  const getRequestTypeText = (type: string) => {
    switch (type) {
      case 'late_checkin': return 'Late Check-in';
      case 'early_checkout': return 'Early Check-out';
      case 'leave_request': return 'Leave Request';
      case 'overtime': return 'Overtime';
      default: return 'Unknown';
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'pending': return COLORS.warning;
      case 'approved': return COLORS.success;
      case 'rejected': return COLORS.error;
      default: return COLORS.gray400;
    }
  };

  const handleApprove = async (requestId: string) => {
    const res = await leaveService.approveLeaveRequest(requestId, { isApproved: true });
    if (res.success) {
      setRequests(requests.map(r => r.id === requestId ? { ...r, status: 'approved' as const } : r));
      Alert.alert('Success', 'Request approved successfully');
    } else {
      Alert.alert('Error', res.error?.message ?? 'Failed to approve');
    }
  };

  const handleReject = (requestId: string) => {
    Alert.alert('Reject Request', 'Are you sure you want to reject this request?', [
      { text: 'Cancel', style: 'cancel' },
      { text: 'Reject', style: 'destructive', onPress: async () => {
        const res = await leaveService.approveLeaveRequest(requestId, { isApproved: false, rejectionReason: 'Rejected by supervisor' });
        if (res.success) {
          setRequests(requests.map(r => r.id === requestId ? { ...r, status: 'rejected' as const } : r));
          Alert.alert('Success', 'Request rejected');
        } else {
          Alert.alert('Error', res.error?.message ?? 'Failed to reject');
        }
      }}
    ]);
  };

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadRequests();
  }, [loadRequests]);

  const renderRequest = ({ item }: { item: AttendanceRequest }) => (
    <Card style={styles.requestCard}>
      <View style={styles.requestHeader}>
        <View style={styles.guardInfo}>
          <Text style={styles.guardName}>{item.guardName}</Text>
          <Text style={styles.guardId}>{item.guardId}</Text>
          <Text style={styles.site}>{item.site}</Text>
        </View>
        <View style={[styles.statusBadge, { backgroundColor: getStatusColor(item.status) + '15' }]}>
          <Text style={[styles.statusText, { color: getStatusColor(item.status) }]}>
            {item.status.charAt(0).toUpperCase() + item.status.slice(1)}
          </Text>
        </View>
      </View>

      <View style={styles.requestDetails}>
        <View style={styles.detailRow}>
          <MaterialCommunityIcons name="clock-outline" size={16} color={COLORS.textSecondary} />
          <Text style={styles.detailText}>{getRequestTypeText(item.requestType)}</Text>
        </View>
        <View style={styles.detailRow}>
          <MaterialCommunityIcons name="calendar-outline" size={16} color={COLORS.textSecondary} />
          <Text style={styles.detailText}>{item.date}</Text>
        </View>
        <View style={styles.detailRow}>
          <MaterialCommunityIcons name="clock" size={16} color={COLORS.textSecondary} />
          <Text style={styles.detailText}>{item.time}</Text>
        </View>
      </View>

      <View style={styles.reasonSection}>
        <Text style={styles.reasonLabel}>Reason:</Text>
        <Text style={styles.reasonText}>{item.reason}</Text>
      </View>

      <View style={styles.submittedAt}>
        <Text style={styles.submittedText}>Submitted: {item.submittedAt}</Text>
      </View>

      {item.status === 'pending' && (
        <View style={styles.actionButtons}>
          <TouchableOpacity style={[styles.actionButton, styles.approveButton]} onPress={() => handleApprove(item.id)}>
            <MaterialCommunityIcons name="check" size={18} color={COLORS.white} />
            <Text style={styles.actionButtonText}>Approve</Text>
          </TouchableOpacity>
          <TouchableOpacity style={[styles.actionButton, styles.rejectButton]} onPress={() => handleReject(item.id)}>
            <MaterialCommunityIcons name="close" size={18} color={COLORS.white} />
            <Text style={styles.actionButtonText}>Reject</Text>
          </TouchableOpacity>
        </View>
      )}
    </Card>
  );

  const filterOptions = [
    { key: 'pending', label: 'Pending', count: requests.filter(r => r.status === 'pending').length },
    { key: 'approved', label: 'Approved', count: requests.filter(r => r.status === 'approved').length },
    { key: 'rejected', label: 'Rejected', count: requests.filter(r => r.status === 'rejected').length },
    { key: 'all', label: 'All', count: requests.length },
  ];

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Attendance Approval</Text>
          <View style={styles.placeholder} />
        </View>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading requests...</Text>
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
        <Text style={styles.headerTitle}>Attendance Approval</Text>
        <View style={styles.placeholder} />
      </View>

      <View style={styles.statsContainer}>
        <Card style={styles.statCard}>
          <Text style={styles.statNumber}>{requests.filter(r => r.status === 'pending').length}</Text>
          <Text style={styles.statLabel}>Pending</Text>
        </Card>
        <Card style={styles.statCard}>
          <Text style={styles.statNumber}>{requests.filter(r => r.status === 'approved').length}</Text>
          <Text style={styles.statLabel}>Approved</Text>
        </Card>
        <Card style={styles.statCard}>
          <Text style={styles.statNumber}>{requests.filter(r => r.status === 'rejected').length}</Text>
          <Text style={styles.statLabel}>Rejected</Text>
        </Card>
      </View>

      <View style={styles.filterRow}>
        {filterOptions.map((option) => (
          <TouchableOpacity
            key={option.key}
            style={[styles.filterButton, filter === option.key && styles.activeFilter]}
            onPress={() => setFilter(option.key as any)}
          >
            <Text style={[styles.filterText, filter === option.key && styles.activeFilterText]}>
              {option.label} ({option.count})
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      <FlatList
        data={filteredRequests}
        keyExtractor={item => item.id}
        renderItem={renderRequest}
        contentContainerStyle={styles.list}
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
        ListEmptyComponent={<Text style={styles.emptyText}>No leave requests to show.</Text>}
      />
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  statsContainer: { flexDirection: 'row', paddingHorizontal: SIZES.md, marginTop: SIZES.md, marginBottom: SIZES.sm, gap: SIZES.sm },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.sm },
  statNumber: { fontSize: FONTS.h3, fontWeight: 'bold', color: COLORS.textPrimary },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, textAlign: 'center', marginTop: SIZES.xs },
  filterRow: { flexDirection: 'row', paddingHorizontal: SIZES.md, marginBottom: SIZES.md, gap: SIZES.sm, flexWrap: 'wrap' },
  filterButton: { paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, backgroundColor: COLORS.white, borderRadius: SIZES.radiusFull, ...SHADOWS.small },
  activeFilter: { backgroundColor: COLORS.primary },
  filterText: { fontSize: FONTS.caption, color: COLORS.textSecondary, fontWeight: '500' },
  activeFilterText: { color: COLORS.white },
  list: { paddingHorizontal: SIZES.md, paddingBottom: SIZES.xl },
  requestCard: { padding: SIZES.md, marginBottom: SIZES.sm },
  requestHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: SIZES.md },
  guardInfo: { flex: 1 },
  guardName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  guardId: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  site: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginTop: 2 },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  statusText: { fontSize: FONTS.caption, fontWeight: '600' },
  requestDetails: { marginBottom: SIZES.md },
  detailRow: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.xs },
  detailText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginLeft: SIZES.sm },
  reasonSection: { backgroundColor: COLORS.gray100, padding: SIZES.sm, borderRadius: SIZES.radiusSm, marginBottom: SIZES.sm },
  reasonLabel: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.textPrimary, marginBottom: 4 },
  reasonText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  submittedAt: { marginBottom: SIZES.md },
  submittedText: { fontSize: FONTS.caption, color: COLORS.textSecondary, fontStyle: 'italic' },
  actionButtons: { flexDirection: 'row', gap: SIZES.sm },
  actionButton: { flex: 1, flexDirection: 'row', alignItems: 'center', justifyContent: 'center', paddingVertical: SIZES.sm, borderRadius: SIZES.radiusSm, gap: SIZES.xs },
  approveButton: { backgroundColor: COLORS.success },
  rejectButton: { backgroundColor: COLORS.error },
  actionButtonText: { fontSize: FONTS.bodySmall, color: COLORS.white, fontWeight: '600' },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: SIZES.xl },
  loadingText: { marginTop: SIZES.md, fontSize: FONTS.body, color: COLORS.textSecondary },
  emptyText: { textAlign: 'center', padding: SIZES.xl, color: COLORS.textSecondary, fontSize: FONTS.body },
});

export default AttendanceApprovalScreen;
