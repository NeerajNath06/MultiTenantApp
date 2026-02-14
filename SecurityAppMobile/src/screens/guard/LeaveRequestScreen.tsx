import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
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

      // Load leave balance
      const balanceResult = await leaveService.getLeaveBalance(user.id);
      if (balanceResult.success && balanceResult.data) {
        const balance = balanceResult.data;
        setLeaveBalance([
          { type: 'Casual Leave', total: balance.totalLeaves || 12, used: balance.usedLeaves || 0, remaining: balance.remainingLeaves || 0, color: COLORS.primaryBlue },
          { type: 'Sick Leave', total: 10, used: 0, remaining: 10, color: COLORS.error },
          { type: 'Earned Leave', total: 15, used: 0, remaining: 15, color: COLORS.success },
          { type: 'Comp Off', total: 4, used: 0, remaining: 4, color: COLORS.warning },
        ]);
      }

      // Load leave history
      const historyResult = await leaveService.getLeaveRequests({
        guardId: user.id,
        pageSize: 50,
      });

      if (historyResult.success && historyResult.data) {
        const requests = Array.isArray(historyResult.data)
          ? historyResult.data
          : (historyResult.data.data || []);

        const mappedHistory = requests.map((req: any) => ({
          id: req.id,
          type: req.leaveType || 'Casual Leave',
          fromDate: new Date(req.startDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }),
          toDate: new Date(req.endDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }),
          days: Math.ceil((new Date(req.endDate).getTime() - new Date(req.startDate).getTime()) / (1000 * 60 * 60 * 24)) + 1,
          reason: req.reason || '',
          status: req.status?.toLowerCase() || 'pending',
          appliedOn: new Date(req.createdAt || req.createdOn).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }),
          approvedBy: req.approvedBy || undefined,
        }));

        setLeaveHistory(mappedHistory);
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

  const handleApplyLeave = async () => {
    if (!leaveType) {
      Alert.alert('Error', 'Please select leave type');
      return;
    }
    if (!fromDate || !toDate) {
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

      // Note: The API doesn't have a create leave request endpoint in the controller
      // This would need to be added to the backend
      // For now, we'll show a message
      Alert.alert(
        'Info',
        'Leave request submission will be available once the backend endpoint is implemented.',
        [
          { text: 'OK', onPress: () => {
            setLeaveType('');
            setFromDate('');
            setToDate('');
            setReason('');
            setActiveTab('history');
            loadLeaveData();
          }}
        ]
      );
    } catch (error) {
      console.error('Error applying leave:', error);
      Alert.alert('Error', 'Failed to submit leave request');
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
        contentContainerStyle={styles.content}
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
            {leaveHistory.map((leave) => (
              <View key={leave.id} style={styles.historyCard}>
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
            ))}
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  balanceScroll: { maxHeight: 130, marginTop: SIZES.md },
  balanceContainer: { paddingHorizontal: SIZES.md, gap: SIZES.sm },
  balanceCard: { width: 160, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, borderLeftWidth: 4, ...SHADOWS.small },
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
  content: { padding: SIZES.md },
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
});

export default LeaveRequestScreen;
