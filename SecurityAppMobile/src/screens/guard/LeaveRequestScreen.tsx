import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS, scaleWidth } from '../../constants/theme';
import Input from '../../components/common/Input';
import Button from '../../components/common/Button';
import { leaveService } from '../../services/leaveService';
import { authService } from '../../services/authService';

interface LeaveRequest {
  id: number;
  type: string;
  fromDate: string;
  toDate: string;
  days: number;
  reason: string;
  status: 'pending' | 'approved' | 'rejected';
  appliedOn: string;
  approvedBy?: string;
}

interface LeaveBalance {
  type: string;
  total: number;
  used: number;
  remaining: number;
  color: string;
}

function LeaveRequestScreen({ navigation }: any) {
  const [activeTab, setActiveTab] = useState<'apply' | 'history'>('apply');
  const [leaveType, setLeaveType] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [fromDateISO, setFromDateISO] = useState('');
  const [toDateISO, setToDateISO] = useState('');
  const [reason, setReason] = useState('');
  const [loading, setLoading] = useState(false);
  const [showFromDatePicker, setShowFromDatePicker] = useState(false);
  const [showToDatePicker, setShowToDatePicker] = useState(false);

  // Generate dates for next 30 days
  const getUpcomingDates = () => {
    const dates = [];
    const today = new Date();
    for (let i = 0; i < 30; i++) {
      const date = new Date(today);
      date.setDate(today.getDate() + i);
      dates.push({
        date: date,
        display: date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }),
        dayName: date.toLocaleDateString('en-US', { weekday: 'short' }),
      });
    }
    return dates;
  };

  const upcomingDates = getUpcomingDates();

  const [leaveBalance, setLeaveBalance] = useState<LeaveBalance[]>([]);
  const [leaveHistory, setLeaveHistory] = useState<LeaveRequest[]>([]);
  const [loadingBalance, setLoadingBalance] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  useEffect(() => {
    loadLeaveData();
  }, []);

  const loadLeaveData = async () => {
    try {
      setLoadingBalance(true);
      const user = await authService.getStoredUser();
      if (!user) return;

      const guardIdForApi = (user as { guardId?: string }).guardId || user.id;

      // Load leave balance (API expects SecurityGuard id)
      const balanceResult = await leaveService.getLeaveBalance(guardIdForApi);
      if (balanceResult.success && balanceResult.data) {
        const balance = balanceResult.data;
        setLeaveBalance([
          { type: 'Casual Leave', total: balance.totalLeaves ?? 12, used: balance.usedLeaves ?? 0, remaining: balance.remainingLeaves ?? 12, color: COLORS.primaryBlue },
          { type: 'Sick Leave', total: 10, used: 0, remaining: 10, color: COLORS.error },
          { type: 'Earned Leave', total: 15, used: 0, remaining: 15, color: COLORS.success },
          { type: 'Comp Off', total: 4, used: 0, remaining: 4, color: COLORS.warning },
        ]);
      } else {
        setLeaveBalance([
          { type: 'Casual Leave', total: 12, used: 0, remaining: 12, color: COLORS.primaryBlue },
          { type: 'Sick Leave', total: 10, used: 0, remaining: 10, color: COLORS.error },
          { type: 'Earned Leave', total: 15, used: 0, remaining: 15, color: COLORS.success },
          { type: 'Comp Off', total: 4, used: 0, remaining: 4, color: COLORS.warning },
        ]);
      }

      // Load leave history - API returns { items: LeaveRequestDto[], totalCount, ... }
      const historyResult = await leaveService.getLeaveRequests({
        guardId: guardIdForApi,
        pageSize: 50,
      });

      if (historyResult.success && historyResult.data) {
        const list = historyResult.data.items ?? historyResult.data.Items ?? (Array.isArray(historyResult.data) ? historyResult.data : []);
        const mappedHistory = list.map((req: any) => {
          const start = req.startDate ?? req.StartDate;
          const end = req.endDate ?? req.EndDate;
          const created = req.createdDate ?? req.CreatedDate ?? req.createdAt ?? req.createdOn;
          const days = req.totalDays ?? (start && end ? Math.ceil((new Date(end).getTime() - new Date(start).getTime()) / (1000 * 60 * 60 * 24)) + 1 : 0);
          return {
            id: req.id ?? req.Id,
            type: req.leaveType ?? req.LeaveType ?? 'Casual',
            fromDate: start ? new Date(start).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }) : '—',
            toDate: end ? new Date(end).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }) : '—',
            days,
            reason: req.reason ?? req.Reason ?? '',
            status: (req.status ?? req.Status ?? 'pending').toString().toLowerCase(),
            appliedOn: created ? new Date(created).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }) : '—',
            approvedBy: req.approvedBy ?? req.approvedByName ?? undefined,
          };
        });
        setLeaveHistory(mappedHistory);
      } else {
        setLeaveHistory([]);
      }
    } catch (error) {
      console.error('Error loading leave data:', error);
    } finally {
      setLoadingBalance(false);
      setRefreshing(false);
    }
  };

  const onRefresh = () => {
    setRefreshing(true);
    loadLeaveData();
  };

  const leaveTypes = ['Casual Leave', 'Sick Leave', 'Earned Leave', 'Comp Off', 'Half Day'];

  // Map display label to API leave type (API allows: Casual, Sick, Emergency, Annual, Unpaid)
  const getApiLeaveType = (label: string): string => {
    const map: Record<string, string> = {
      'Casual Leave': 'Casual',
      'Sick Leave': 'Sick',
      'Earned Leave': 'Annual',
      'Comp Off': 'Unpaid',
      'Half Day': 'Casual',
    };
    return map[label] || 'Casual';
  };

  const handleApplyLeave = async () => {
    if (!leaveType) {
      Alert.alert('Error', 'Please select leave type');
      return;
    }
    if (!fromDate || !toDate) {
      Alert.alert('Error', 'Please select dates');
      return;
    }
    if (!fromDateISO || !toDateISO) {
      Alert.alert('Error', 'Please select dates');
      return;
    }
    if (!reason.trim()) {
      Alert.alert('Error', 'Please provide a reason');
      return;
    }

    setLoading(true);
    try {
      const user = await authService.getStoredUser();
      if (!user) {
        Alert.alert('Error', 'User not found. Please login again.');
        setLoading(false);
        return;
      }

      const guardIdForApi = (user as { guardId?: string }).guardId || user.id;
      const result = await leaveService.createLeaveRequest({
        guardId: guardIdForApi,
        leaveType: getApiLeaveType(leaveType),
        startDate: fromDateISO,
        endDate: toDateISO,
        reason: reason.trim(),
      });

      if (result.success) {
        Alert.alert(
          'Success',
          'Leave request submitted successfully. Your supervisor will review it shortly.',
          [
            {
              text: 'OK',
              onPress: () => {
                setLeaveType('');
                setFromDate('');
                setToDate('');
                setFromDateISO('');
                setToDateISO('');
                setReason('');
                setActiveTab('history');
                loadLeaveData();
              },
            },
          ]
        );
      } else {
        Alert.alert('Error', result.error?.message || 'Failed to submit leave request');
      }
    } catch (error) {
      console.error('Error applying leave:', error);
      Alert.alert('Error', 'Failed to submit leave request. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'approved': return COLORS.success;
      case 'pending': return COLORS.warning;
      case 'rejected': return COLORS.error;
      default: return COLORS.gray500;
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Leave Request</Text>
        <View style={styles.placeholder} />
      </View>

      {/* Leave Balance Cards */}
      <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.balanceScroll} contentContainerStyle={styles.balanceContainer}>
        {leaveBalance.map((balance, index) => (
          <View key={index} style={[styles.balanceCard, { borderLeftColor: balance.color }]}>
            <Text style={styles.balanceType}>{balance.type}</Text>
            <View style={styles.balanceRow}>
              <View style={styles.balanceItem}>
                <Text style={styles.balanceNumber}>{balance.remaining}</Text>
                <Text style={styles.balanceLabel}>Available</Text>
              </View>
              <View style={styles.balanceDivider} />
              <View style={styles.balanceItem}>
                <Text style={[styles.balanceNumber, { color: COLORS.gray500 }]}>{balance.used}</Text>
                <Text style={styles.balanceLabel}>Used</Text>
              </View>
            </View>
            <View style={styles.progressBar}>
              <View style={[styles.progressFill, { width: `${(balance.used / balance.total) * 100}%`, backgroundColor: balance.color }]} />
            </View>
          </View>
        ))}
      </ScrollView>

      {/* Tabs */}
      <View style={styles.tabContainer}>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'apply' && styles.tabActive]}
          onPress={() => setActiveTab('apply')}
        >
          <MaterialCommunityIcons name="plus-circle" size={20} color={activeTab === 'apply' ? COLORS.primary : COLORS.gray500} />
          <Text style={[styles.tabText, activeTab === 'apply' && styles.tabTextActive]}>Apply Leave</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'history' && styles.tabActive]}
          onPress={() => setActiveTab('history')}
        >
          <MaterialCommunityIcons name="history" size={20} color={activeTab === 'history' ? COLORS.primary : COLORS.gray500} />
          <Text style={[styles.tabText, activeTab === 'history' && styles.tabTextActive]}>History</Text>
        </TouchableOpacity>
      </View>

      <ScrollView 
        showsVerticalScrollIndicator={false} 
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />
        }
      >
        {activeTab === 'apply' ? (
          <View style={styles.formContainer}>
            {/* Leave Type Selection */}
            <Text style={styles.label}>Leave Type</Text>
            <View style={styles.typeGrid}>
              {leaveTypes.map((type, index) => (
                <TouchableOpacity
                  key={index}
                  style={[styles.typeChip, leaveType === type && styles.typeChipActive]}
                  onPress={() => setLeaveType(type)}
                >
                  <Text style={[styles.typeChipText, leaveType === type && styles.typeChipTextActive]}>{type}</Text>
                </TouchableOpacity>
              ))}
            </View>

            {/* Date Selection */}
            <View style={styles.dateRow}>
              <View style={styles.dateField}>
                <Text style={styles.label}>From Date</Text>
                <TouchableOpacity 
                  style={[styles.dateInput, fromDate && styles.dateInputSelected]}
                  onPress={() => setShowFromDatePicker(!showFromDatePicker)}
                >
                  <MaterialCommunityIcons name="calendar" size={20} color={fromDate ? COLORS.primary : COLORS.gray500} />
                  <Text style={[styles.dateText, fromDate && styles.dateTextSelected]}>{fromDate || 'Select Date'}</Text>
                </TouchableOpacity>
              </View>
              <View style={styles.dateField}>
                <Text style={styles.label}>To Date</Text>
                <TouchableOpacity 
                  style={[styles.dateInput, toDate && styles.dateInputSelected]}
                  onPress={() => setShowToDatePicker(!showToDatePicker)}
                >
                  <MaterialCommunityIcons name="calendar" size={20} color={toDate ? COLORS.primary : COLORS.gray500} />
                  <Text style={[styles.dateText, toDate && styles.dateTextSelected]}>{toDate || 'Select Date'}</Text>
                </TouchableOpacity>
              </View>
            </View>

            {/* From Date Picker */}
            {showFromDatePicker && (
              <View style={styles.datePickerContainer}>
                <View style={styles.datePickerHeader}>
                  <Text style={styles.datePickerTitle}>Select From Date</Text>
                  <TouchableOpacity onPress={() => setShowFromDatePicker(false)}>
                    <MaterialCommunityIcons name="close" size={24} color={COLORS.gray500} />
                  </TouchableOpacity>
                </View>
                <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.dateScroll}>
                  {upcomingDates.map((item, index) => (
                    <TouchableOpacity
                      key={index}
                      style={[styles.dateChip, fromDate === item.display && styles.dateChipActive]}
                      onPress={() => {
                        setFromDate(item.display);
                        setFromDateISO(item.date.toISOString().slice(0, 10));
                        setShowFromDatePicker(false);
                      }}
                    >
                      <Text style={[styles.dateDayName, fromDate === item.display && styles.dateChipTextActive]}>{item.dayName}</Text>
                      <Text style={[styles.dateDay, fromDate === item.display && styles.dateChipTextActive]}>{item.date.getDate()}</Text>
                      <Text style={[styles.dateMonth, fromDate === item.display && styles.dateChipTextActive]}>{item.date.toLocaleDateString('en-US', { month: 'short' })}</Text>
                    </TouchableOpacity>
                  ))}
                </ScrollView>
              </View>
            )}

            {/* To Date Picker */}
            {showToDatePicker && (
              <View style={styles.datePickerContainer}>
                <View style={styles.datePickerHeader}>
                  <Text style={styles.datePickerTitle}>Select To Date</Text>
                  <TouchableOpacity onPress={() => setShowToDatePicker(false)}>
                    <MaterialCommunityIcons name="close" size={24} color={COLORS.gray500} />
                  </TouchableOpacity>
                </View>
                <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.dateScroll}>
                  {upcomingDates.map((item, index) => (
                    <TouchableOpacity
                      key={index}
                      style={[styles.dateChip, toDate === item.display && styles.dateChipActive]}
                      onPress={() => {
                        setToDate(item.display);
                        setToDateISO(item.date.toISOString().slice(0, 10));
                        setShowToDatePicker(false);
                      }}
                    >
                      <Text style={[styles.dateDayName, toDate === item.display && styles.dateChipTextActive]}>{item.dayName}</Text>
                      <Text style={[styles.dateDay, toDate === item.display && styles.dateChipTextActive]}>{item.date.getDate()}</Text>
                      <Text style={[styles.dateMonth, toDate === item.display && styles.dateChipTextActive]}>{item.date.toLocaleDateString('en-US', { month: 'short' })}</Text>
                    </TouchableOpacity>
                  ))}
                </ScrollView>
              </View>
            )}

            {/* Reason */}
            <Input
              label="Reason"
              placeholder="Enter reason for leave..."
              value={reason}
              onChangeText={setReason}
              multiline
              numberOfLines={4}
            />

            {/* Submit Button */}
            <Button
              title="Submit Leave Request"
              onPress={handleApplyLeave}
              loading={loading}
              style={styles.submitBtn}
            />
          </View>
        ) : (
          <View style={styles.historyContainer}>
            {leaveHistory.length === 0 ? (
              <View style={styles.emptyHistory}>
                <MaterialCommunityIcons name="calendar-blank-outline" size={48} color={COLORS.gray400} />
                <Text style={styles.emptyHistoryTitle}>No leave requests yet</Text>
                <Text style={styles.emptyHistoryText}>Your leave history will appear here after you apply.</Text>
              </View>
            ) : (
            leaveHistory.map((leave) => (
              <View key={String(leave.id)} style={styles.historyCard}>
                <View style={styles.historyHeader}>
                  <View>
                    <Text style={styles.historyType}>{leave.type}</Text>
                    <Text style={styles.historyDates}>{leave.fromDate} - {leave.toDate}</Text>
                  </View>
                  <View style={[styles.statusBadge, { backgroundColor: getStatusColor(leave.status) + '15' }]}>
                    <Text style={[styles.statusText, { color: getStatusColor(leave.status) }]}>
                      {leave.status.charAt(0).toUpperCase() + leave.status.slice(1)}
                    </Text>
                  </View>
                </View>
                
                <View style={styles.historyDetails}>
                  <View style={styles.historyDetail}>
                    <MaterialCommunityIcons name="calendar-range" size={16} color={COLORS.gray500} />
                    <Text style={styles.historyDetailText}>{leave.days} day(s)</Text>
                  </View>
                  <View style={styles.historyDetail}>
                    <MaterialCommunityIcons name="clock-outline" size={16} color={COLORS.gray500} />
                    <Text style={styles.historyDetailText}>Applied: {leave.appliedOn}</Text>
                  </View>
                </View>
                
                <Text style={styles.historyReason}>Reason: {leave.reason}</Text>
                
                {leave.approvedBy && (
                  <Text style={styles.approvedBy}>Approved by: {leave.approvedBy}</Text>
                )}
              </View>
            ))
            )}
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  scrollContent: { flexGrow: 1, paddingHorizontal: SIZES.md, paddingBottom: SIZES.xxl },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  balanceScroll: { maxHeight: 130, marginTop: SIZES.md },
  balanceContainer: { paddingHorizontal: SIZES.md, gap: SIZES.sm },
  balanceCard: { width: scaleWidth(160), minWidth: scaleWidth(140), backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, borderLeftWidth: 4, ...SHADOWS.small },
  balanceType: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  balanceRow: { flexDirection: 'row', alignItems: 'center' },
  balanceItem: { flex: 1, alignItems: 'center' },
  balanceNumber: { fontSize: FONTS.h3, fontWeight: 'bold', color: COLORS.textPrimary },
  balanceLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary },
  balanceDivider: { width: 1, height: 30, backgroundColor: COLORS.gray200 },
  progressBar: { height: 4, backgroundColor: COLORS.gray200, borderRadius: 2, marginTop: SIZES.sm, overflow: 'hidden' },
  progressFill: { height: '100%', borderRadius: 2 },
  tabContainer: { flexDirection: 'row', marginHorizontal: SIZES.md, marginTop: SIZES.md, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: 4, ...SHADOWS.small },
  tab: { flex: 1, flexDirection: 'row', alignItems: 'center', justifyContent: 'center', paddingVertical: SIZES.sm, borderRadius: SIZES.radiusSm, gap: SIZES.xs },
  tabActive: { backgroundColor: COLORS.primary + '10' },
  tabText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, fontWeight: '500' },
  tabTextActive: { color: COLORS.primary, fontWeight: '600' },
  content: { padding: SIZES.md, flex: 1 },
  formContainer: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, ...SHADOWS.small },
  label: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  typeGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: SIZES.sm, marginBottom: SIZES.lg },
  typeChip: { paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusFull, borderWidth: 1, borderColor: 'transparent' },
  typeChipActive: { backgroundColor: COLORS.primary + '10', borderColor: COLORS.primary },
  typeChipText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  typeChipTextActive: { color: COLORS.primary, fontWeight: '600' },
  dateRow: { flexDirection: 'row', gap: SIZES.md, marginBottom: SIZES.md },
  dateField: { flex: 1 },
  dateInput: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.gray100, paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, borderRadius: SIZES.radiusMd, gap: SIZES.sm },
  dateText: { fontSize: FONTS.body, color: COLORS.gray500, flex: 1 },
  dateInputSelected: { backgroundColor: COLORS.primary + '10', borderColor: COLORS.primary, borderWidth: 1 },
  dateTextSelected: { color: COLORS.primary, fontWeight: '500' },
  datePickerContainer: { backgroundColor: COLORS.gray50, borderRadius: SIZES.radiusMd, padding: SIZES.sm, marginBottom: SIZES.md },
  datePickerHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: SIZES.sm },
  datePickerTitle: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary },
  dateScroll: { marginBottom: SIZES.xs },
  dateChip: { width: 60, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.sm, alignItems: 'center', marginRight: SIZES.sm, borderWidth: 1, borderColor: COLORS.gray200 },
  dateChipActive: { backgroundColor: COLORS.primary, borderColor: COLORS.primary },
  dateDayName: { fontSize: FONTS.tiny, color: COLORS.textSecondary },
  dateDay: { fontSize: FONTS.h4, fontWeight: 'bold', color: COLORS.textPrimary, marginVertical: 2 },
  dateMonth: { fontSize: FONTS.tiny, color: COLORS.textSecondary },
  dateChipTextActive: { color: COLORS.white },
  submitBtn: { marginTop: SIZES.md },
  historyContainer: {},
  historyCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  historyHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: SIZES.sm },
  historyType: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  historyDates: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  statusText: { fontSize: FONTS.caption, fontWeight: '600' },
  historyDetails: { flexDirection: 'row', gap: SIZES.md, marginBottom: SIZES.sm },
  historyDetail: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  historyDetailText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  historyReason: { fontSize: FONTS.caption, color: COLORS.textSecondary, fontStyle: 'italic' },
  approvedBy: { fontSize: FONTS.tiny, color: COLORS.success, marginTop: SIZES.xs },
  emptyHistory: { alignItems: 'center', justifyContent: 'center', paddingVertical: SIZES.xxl },
  emptyHistoryTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginTop: SIZES.md },
  emptyHistoryText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
});

export default LeaveRequestScreen;
