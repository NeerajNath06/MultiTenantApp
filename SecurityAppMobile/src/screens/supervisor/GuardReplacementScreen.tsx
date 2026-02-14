import React, { useState, useCallback, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Alert,
  RefreshControl,
  Linking,
  ActivityIndicator,
  Modal,
  FlatList,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { authService } from '../../services/authService';
import { leaveService } from '../../services/leaveService';
import { guardService, GuardItem } from '../../services/guardService';
import { deploymentService } from '../../services/deploymentService';

interface ReplacementRequest {
  id: string;
  guardId: string;
  guardName: string;
  guardCode: string;
  siteId: string;
  siteName: string;
  shiftId: string;
  shiftName: string;
  startDate: string;
  endDate: string;
  dateLabel: string;
  reason: string;
  leaveType: string;
  status: string;
  replacementGuardName?: string;
  priority: 'urgent' | 'normal';
}

function formatDateRange(start: string, end: string): string {
  try {
    const s = new Date(start);
    const e = new Date(end);
    if (s.toDateString() === e.toDateString()) return s.toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' });
    return `${s.toLocaleDateString('en-IN', { day: 'numeric', month: 'short' })} - ${e.toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' })}`;
  } catch {
    return start;
  }
}

function GuardReplacementScreen({ navigation }: any) {
  const [refreshing, setRefreshing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [selectedTab, setSelectedTab] = useState<'requests' | 'available'>('requests');
  const [requests, setRequests] = useState<ReplacementRequest[]>([]);
  const [availableGuards, setAvailableGuards] = useState<GuardItem[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [assignModal, setAssignModal] = useState<{ request: ReplacementRequest } | null>(null);
  const [saving, setSaving] = useState(false);

  const loadLeaveRequests = useCallback(async () => {
    const user = await authService.getStoredUser();
    const supervisorId = user?.isSupervisor ? user.id : undefined;
    const res = await leaveService.getLeaveRequests({
      supervisorId,
      pageNumber: 1,
      pageSize: 100,
    });
    if (!res.success || !res.data) {
      setRequests([]);
      return [];
    }
    const raw = res.data as { items?: any[]; Items?: any[] };
    const items = raw.items ?? raw.Items ?? (Array.isArray(res.data) ? res.data : []);
    return items as any[];
  }, []);

  const loadGuards = useCallback(async () => {
    const user = await authService.getStoredUser();
    const supervisorId = user?.isSupervisor ? user.id : undefined;
    const res = await guardService.getGuards({ supervisorId, pageSize: 200, includeInactive: false });
    if (res.success && res.data?.items) setAvailableGuards(res.data.items);
    else setAvailableGuards([]);
  }, []);

  const enrichWithDeployment = useCallback(async (item: any): Promise<ReplacementRequest> => {
    const guardId = String(item.guardId ?? item.GuardId ?? '');
    const startDate = (item.startDate ?? item.StartDate ?? '').toString().split('T')[0];
    const endDate = (item.endDate ?? item.EndDate ?? '').toString().split('T')[0];
    let siteName = '—';
    let siteId = '';
    let shiftName = '—';
    let shiftId = '';
    const depRes = await deploymentService.getDeployments({
      guardId,
      dateFrom: startDate,
      dateTo: endDate,
      pageSize: 1,
      skipCache: true,
    });
    if (depRes.success && depRes.data) {
      const list = Array.isArray(depRes.data) ? depRes.data : (depRes.data as any).items ?? (depRes.data as any).Items ?? [];
      const first = list[0];
      if (first) {
        siteName = first.siteName ?? first.SiteName ?? '—';
        siteId = String(first.siteId ?? first.SiteId ?? '');
        shiftName = first.shiftName ?? first.ShiftName ?? '—';
        shiftId = String(first.shiftId ?? first.ShiftId ?? '');
      }
    }
    const guardName = item.guardName ?? item.GuardName ?? '—';
    const reason = item.reason ?? item.Reason ?? '—';
    const leaveType = item.leaveType ?? item.LeaveType ?? 'Casual';
    const status = (item.status ?? item.Status ?? 'Pending').toLowerCase();
    const priority: 'urgent' | 'normal' = (leaveType === 'Emergency' || leaveType === 'Sick') ? 'urgent' : 'normal';
    return {
      id: String(item.id ?? item.Id ?? ''),
      guardId,
      guardName,
      guardCode: item.guardCode ?? item.GuardCode ?? '',
      siteId,
      siteName,
      shiftId,
      shiftName,
      startDate,
      endDate,
      dateLabel: formatDateRange(startDate, endDate),
      reason,
      leaveType,
      status,
      priority,
    };
  }, []);

  const loadAll = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      await loadGuards();
      const leaveItems = await loadLeaveRequests();
      const enriched: ReplacementRequest[] = await Promise.all(leaveItems.map((item: any) => enrichWithDeployment(item)));
      setRequests(enriched);
    } catch (e) {
      setError((e as Error).message ?? 'Failed to load');
      setRequests([]);
    }
    setLoading(false);
    setRefreshing(false);
  }, [loadGuards, loadLeaveRequests, enrichWithDeployment]);

  useEffect(() => {
    loadAll();
  }, []);

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadAll();
  }, [loadAll]);

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'pending': return COLORS.warning;
      case 'approved': return COLORS.success;
      case 'rejected': return COLORS.error;
      default: return COLORS.gray500;
    }
  };

  const handleApprove = useCallback(async (requestId: string) => {
    setSaving(true);
    const res = await leaveService.approveLeaveRequest(requestId, { isApproved: true });
    setSaving(false);
    if (res.success) {
      await loadAll();
      Alert.alert('Approved', 'Leave request approved. Assign a replacement guard for the shift.');
    } else {
      Alert.alert('Error', res.error?.message ?? 'Failed to approve');
    }
  }, [loadAll]);

  const handleReject = useCallback(async (requestId: string) => {
    Alert.alert(
      'Reject Leave Request',
      'Are you sure you want to reject this leave request?',
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Reject',
          style: 'destructive',
          onPress: async () => {
            setSaving(true);
            const res = await leaveService.approveLeaveRequest(requestId, {
              isApproved: false,
              rejectionReason: 'Rejected by supervisor',
            });
            setSaving(false);
            if (res.success) {
              await loadAll();
              Alert.alert('Rejected', 'Leave request has been rejected.');
            } else {
              Alert.alert('Error', res.error?.message ?? 'Failed to reject');
            }
          },
        },
      ]
    );
  }, [loadAll]);

  const handleAssignGuard = useCallback((request: ReplacementRequest) => {
    if (!request.siteId || !request.shiftId) {
      Alert.alert('Cannot Assign', 'Site/shift info not found for this leave period. The guard may not have an assignment on these dates.');
      return;
    }
    setAssignModal({ request });
  }, []);

  const confirmAssignReplacement = useCallback(async (request: ReplacementRequest, replacementGuardId: string) => {
    setSaving(true);
    const user = await authService.getStoredUser();
    const res = await deploymentService.createAssignment({
      guardId: replacementGuardId,
      siteId: request.siteId,
      shiftId: request.shiftId,
      supervisorId: user?.isSupervisor ? user.id : undefined,
      assignmentStartDate: request.startDate,
      assignmentEndDate: request.endDate,
      remarks: `Replacement for ${request.guardName} (leave)`,
    });
    setSaving(false);
    setAssignModal(null);
    if (res.success) {
      await loadAll();
      const guard = availableGuards.find(g => g.id === replacementGuardId);
      Alert.alert('Guard Assigned', `${guard?.firstName ?? ''} ${guard?.lastName ?? ''} has been assigned as replacement for ${request.guardName}.`);
    } else {
      Alert.alert('Error', res.error?.message ?? 'Failed to assign');
    }
  }, [availableGuards, loadAll]);

  const handleRequestPress = useCallback((request: ReplacementRequest) => {
    const actions: { text: string; style?: 'default' | 'cancel' | 'destructive'; onPress?: () => void }[] = [{ text: 'Close', style: 'cancel' }];

    if (request.status === 'pending') {
      actions.push({
        text: 'Approve',
        style: 'default',
        onPress: () => handleApprove(request.id),
      });
      actions.push({
        text: 'Reject',
        style: 'destructive',
        onPress: () => handleReject(request.id),
      });
    }

    if (request.status === 'approved' && request.siteId && request.shiftId) {
      actions.push({
        text: 'Assign Replacement Guard',
        style: 'default',
        onPress: () => handleAssignGuard(request),
      });
    }

    Alert.alert(
      'Leave / Replacement',
      `Guard: ${request.guardName}\nSite: ${request.siteName}\nShift: ${request.shiftName}\nDate: ${request.dateLabel}\n\nReason: ${request.reason}\nStatus: ${request.status.toUpperCase()}`,
      actions
    );
  }, [handleApprove, handleReject, handleAssignGuard]);

  const handleCreateRequest = useCallback(() => {
    Alert.alert(
      'Create Leave Request',
      'Guards submit leave requests from their app. As supervisor you approve or reject requests and assign replacement guards here.',
      [{ text: 'OK' }]
    );
  }, []);

  const handleSchedule = useCallback(() => {
    navigation.navigate('RosterManagement');
  }, [navigation]);

  const handleAnalytics = useCallback(() => {
    const pending = requests.filter(r => r.status === 'pending').length;
    const approved = requests.filter(r => r.status === 'approved').length;
    const rejected = requests.filter(r => r.status === 'rejected').length;
    Alert.alert(
      'Replacement Analytics',
      `Total: ${requests.length}\n\n⏳ Pending: ${pending}\n✅ Approved: ${approved}\n❌ Rejected: ${rejected}\n\nAvailable guards: ${availableGuards.length}`,
      [{ text: 'OK' }]
    );
  }, [requests, availableGuards.length]);

  const handleGuardPress = useCallback((guard: GuardItem) => {
    const phone = guard.phoneNumber?.trim() || '';
    Alert.alert(
      `${guard.firstName} ${guard.lastName}`.trim(),
      phone ? `Phone: ${guard.phoneNumber}` : 'No phone number',
      [
        { text: 'Close', style: 'cancel' },
        ...(phone ? [{ text: 'Call', onPress: () => Linking.openURL(`tel:${phone}`) }] : []),
      ].filter(Boolean) as { text: string; style?: 'cancel'; onPress?: () => void }[]
    );
  }, []);

  const handleEmergencyContact = useCallback(() => {
    Alert.alert(
      'Emergency Coordinator',
      'Contact the emergency coordinator for urgent replacement needs?\n\n📞 Emergency Hotline\n+91 1800 123 4567',
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Call Now', onPress: () => Linking.openURL('tel:+911800123456') },
      ]
    );
  }, []);

  const pendingCount = requests.filter(r => r.status === 'pending').length;
  const urgentCount = requests.filter(r => r.priority === 'urgent' && r.status === 'pending').length;
  const availableCount = availableGuards.length;

  if (loading && requests.length === 0) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading replacement requests...</Text>
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
        <Text style={styles.headerTitle}>Guard Replacement</Text>
        <TouchableOpacity style={styles.addButton} onPress={handleCreateRequest}>
          <MaterialCommunityIcons name="information-outline" size={24} color={COLORS.primary} />
        </TouchableOpacity>
      </View>

      {error ? (
        <View style={styles.errorBanner}>
          <Text style={styles.errorText}>{error}</Text>
        </View>
      ) : null}

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
        contentContainerStyle={styles.content}
      >
        <View style={styles.statsContainer}>
          <Card style={[styles.statCard, { backgroundColor: COLORS.warning + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.warning }]}>{pendingCount}</Text>
            <Text style={styles.statLabel}>Pending</Text>
          </Card>
          <Card style={[styles.statCard, { backgroundColor: COLORS.error + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.error }]}>{urgentCount}</Text>
            <Text style={styles.statLabel}>Urgent</Text>
          </Card>
          <Card style={[styles.statCard, { backgroundColor: COLORS.success + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.success }]}>{availableCount}</Text>
            <Text style={styles.statLabel}>Guards</Text>
          </Card>
        </View>

        <View style={styles.actionsContainer}>
          <TouchableOpacity style={styles.actionButton} onPress={handleSchedule}>
            <MaterialCommunityIcons name="calendar" size={20} color={COLORS.primary} />
            <Text style={styles.actionButtonText}>Roster</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.actionButton} onPress={() => setSelectedTab('available')}>
            <MaterialCommunityIcons name="account-group" size={20} color={COLORS.secondary} />
            <Text style={styles.actionButtonText}>Available</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.actionButton} onPress={handleAnalytics}>
            <MaterialCommunityIcons name="chart-line" size={20} color={COLORS.info} />
            <Text style={styles.actionButtonText}>Analytics</Text>
          </TouchableOpacity>
        </View>

        <View style={styles.tabContainer}>
          <TouchableOpacity
            style={[styles.tab, selectedTab === 'requests' && styles.tabActive]}
            onPress={() => setSelectedTab('requests')}
          >
            <Text style={[styles.tabText, selectedTab === 'requests' && styles.tabTextActive]}>
              Requests ({requests.length})
            </Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.tab, selectedTab === 'available' && styles.tabActive]}
            onPress={() => setSelectedTab('available')}
          >
            <Text style={[styles.tabText, selectedTab === 'available' && styles.tabTextActive]}>
              Guards ({availableCount})
            </Text>
          </TouchableOpacity>
        </View>

        {selectedTab === 'requests' ? (
          <>
            {requests.length === 0 ? (
              <Card style={styles.emptyCard}>
                <Text style={styles.emptyText}>No leave requests. Guards submit leave from their app; you approve and assign replacements here.</Text>
              </Card>
            ) : (
              requests.map((request) => (
                <TouchableOpacity
                  key={request.id}
                  style={[styles.requestCard, request.priority === 'urgent' && styles.requestCardUrgent]}
                  onPress={() => handleRequestPress(request)}
                  disabled={saving}
                >
                  <View style={styles.requestHeader}>
                    {request.priority === 'urgent' && (
                      <View style={styles.urgentBadge}>
                        <Text style={styles.urgentText}>URGENT</Text>
                      </View>
                    )}
                    <View style={[styles.statusBadge, { backgroundColor: getStatusColor(request.status) + '15' }]}>
                      <Text style={[styles.statusText, { color: getStatusColor(request.status) }]}>
                        {request.status.toUpperCase()}
                      </Text>
                    </View>
                  </View>
                  <Text style={styles.guardName}>{request.guardName}</Text>
                  <Text style={styles.siteText}>{request.siteName}</Text>
                  <View style={styles.requestDetails}>
                    <View style={styles.detailItem}>
                      <MaterialCommunityIcons name="clock-outline" size={14} color={COLORS.gray400} />
                      <Text style={styles.detailText}>{request.shiftName}</Text>
                    </View>
                    <View style={styles.detailItem}>
                      <MaterialCommunityIcons name="calendar" size={14} color={COLORS.gray400} />
                      <Text style={styles.detailText}>{request.dateLabel}</Text>
                    </View>
                  </View>
                  <Text style={styles.reasonText}>Reason: {request.reason}</Text>
                </TouchableOpacity>
              ))
            )}
          </>
        ) : (
          <>
            {availableGuards.length === 0 ? (
              <Card style={styles.emptyCard}>
                <Text style={styles.emptyText}>No guards under your supervision.</Text>
              </Card>
            ) : (
              availableGuards.map((guard) => (
                <TouchableOpacity
                  key={guard.id}
                  style={styles.guardCard}
                  onPress={() => handleGuardPress(guard)}
                >
                  <View style={styles.guardAvatar}>
                    <Text style={styles.guardInitials}>
                      {(`${(guard.firstName ?? '')[0] || ''}${(guard.lastName ?? '')[0] || ''}`.toUpperCase() || 'G')}
                    </Text>
                  </View>
                  <View style={styles.guardInfo}>
                    <Text style={styles.guardName}>{guard.firstName} {guard.lastName}</Text>
                    <Text style={styles.guardSite}>{guard.guardCode || guard.phoneNumber || '—'}</Text>
                  </View>
                  {guard.phoneNumber ? (
                    <TouchableOpacity
                      style={styles.callButton}
                      onPress={() => Linking.openURL(`tel:${guard.phoneNumber}`)}
                    >
                      <MaterialCommunityIcons name="phone" size={20} color={COLORS.primary} />
                    </TouchableOpacity>
                  ) : null}
                </TouchableOpacity>
              ))
            )}
          </>
        )}

        <Card style={styles.emergencyCard}>
          <Text style={styles.emergencyText}>
            For urgent replacement needs outside business hours, contact the emergency coordinator immediately.
          </Text>
          <TouchableOpacity style={styles.emergencyButton} onPress={handleEmergencyContact}>
            <MaterialCommunityIcons name="phone" size={20} color={COLORS.white} />
            <Text style={styles.emergencyButtonText}>Emergency Contact</Text>
          </TouchableOpacity>
        </Card>
      </ScrollView>

      {/* Assign replacement guard modal */}
      {assignModal?.request && (
        <Modal visible transparent animationType="slide">
          <View style={styles.modalOverlay}>
            <View style={styles.modalContent}>
              <Text style={styles.modalTitle}>Assign replacement for {assignModal.request.guardName}</Text>
              <Text style={styles.modalSubtitle}>{assignModal.request.siteName} • {assignModal.request.shiftName} • {assignModal.request.dateLabel}</Text>
              <FlatList
                data={availableGuards.filter(g => g.id !== assignModal.request.guardId)}
                keyExtractor={(g) => g.id}
                style={styles.modalList}
                renderItem={({ item }) => (
                  <TouchableOpacity
                    style={styles.modalOption}
                    onPress={() => confirmAssignReplacement(assignModal.request, item.id)}
                    disabled={saving}
                  >
                    <Text style={styles.modalOptionText}>{item.firstName} {item.lastName}</Text>
                    {item.phoneNumber ? <Text style={styles.modalOptionSubtext}>{item.phoneNumber}</Text> : null}
                  </TouchableOpacity>
                )}
              />
              <TouchableOpacity style={[styles.modalOption, styles.modalCancel]} onPress={() => setAssignModal(null)}>
                <Text style={[styles.modalOptionText, { color: COLORS.error }]}>Cancel</Text>
              </TouchableOpacity>
            </View>
          </View>
        </Modal>
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center', gap: SIZES.md },
  loadingText: { fontSize: FONTS.body, color: COLORS.textSecondary },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  addButton: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center' },
  errorBanner: { backgroundColor: COLORS.error + '20', padding: SIZES.sm, marginHorizontal: SIZES.md },
  errorText: { fontSize: FONTS.caption, color: COLORS.error },
  content: { padding: SIZES.md },
  statsContainer: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: SIZES.md },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.md, marginHorizontal: SIZES.xs },
  statNumber: { fontSize: FONTS.h2, fontWeight: 'bold' },
  statLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
  actionsContainer: { flexDirection: 'row', justifyContent: 'space-around', marginBottom: SIZES.md },
  actionButton: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusMd, ...SHADOWS.small },
  actionButtonText: { fontSize: FONTS.bodySmall, color: COLORS.textPrimary, marginLeft: SIZES.xs, fontWeight: '500' },
  tabContainer: { flexDirection: 'row', backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusMd, padding: 4, marginBottom: SIZES.md },
  tab: { flex: 1, paddingVertical: SIZES.sm, alignItems: 'center', borderRadius: SIZES.radiusSm },
  tabActive: { backgroundColor: COLORS.white, ...SHADOWS.small },
  tabText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  tabTextActive: { color: COLORS.primary, fontWeight: '600' },
  requestCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  requestCardUrgent: { borderLeftWidth: 4, borderLeftColor: COLORS.error },
  requestHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: SIZES.sm },
  urgentBadge: { backgroundColor: COLORS.error, paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  urgentText: { fontSize: FONTS.tiny, color: COLORS.white, fontWeight: '600' },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  statusText: { fontSize: FONTS.tiny, fontWeight: '600' },
  guardName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  siteText: { fontSize: FONTS.caption, color: COLORS.gray500, marginBottom: SIZES.sm },
  requestDetails: { flexDirection: 'row', marginBottom: SIZES.xs },
  detailItem: { flexDirection: 'row', alignItems: 'center', marginRight: SIZES.md },
  detailText: { fontSize: FONTS.caption, color: COLORS.gray500, marginLeft: 4 },
  reasonText: { fontSize: FONTS.caption, color: COLORS.textSecondary, fontStyle: 'italic' },
  emptyCard: { padding: SIZES.lg, marginBottom: SIZES.sm },
  emptyText: { fontSize: FONTS.caption, color: COLORS.textSecondary, textAlign: 'center' },
  guardCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  guardAvatar: { width: 48, height: 48, borderRadius: 24, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center', marginRight: SIZES.md },
  guardInitials: { fontSize: FONTS.body, fontWeight: 'bold', color: COLORS.white },
  guardInfo: { flex: 1 },
  guardSite: { fontSize: FONTS.caption, color: COLORS.gray500 },
  callButton: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center' },
  emergencyCard: { marginBottom: SIZES.xxl, padding: SIZES.md, backgroundColor: COLORS.error + '08' },
  emergencyText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginBottom: SIZES.md },
  emergencyButton: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.error, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusMd },
  emergencyButtonText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.white, marginLeft: SIZES.xs },
  modalOverlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'center', padding: SIZES.md },
  modalContent: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, maxHeight: '80%' },
  modalTitle: { fontSize: FONTS.h4, fontWeight: '600', marginBottom: SIZES.xs },
  modalSubtitle: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginBottom: SIZES.md },
  modalList: { maxHeight: 320 },
  modalOption: { padding: SIZES.md, borderBottomWidth: 1, borderBottomColor: COLORS.gray100 },
  modalOptionText: { fontSize: FONTS.body, color: COLORS.textPrimary },
  modalOptionSubtext: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  modalCancel: { borderTopWidth: 1, borderTopColor: COLORS.gray200, marginTop: SIZES.sm },
});

export default GuardReplacementScreen;
