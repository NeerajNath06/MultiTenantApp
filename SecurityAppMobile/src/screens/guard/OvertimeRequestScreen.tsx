import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, TextInput } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Button from '../../components/common/Button';

interface OvertimeRequest {
  id: number;
  date: string;
  startTime: string;
  endTime: string;
  hours: number;
  reason: string;
  status: 'pending' | 'approved' | 'rejected';
  approvedBy?: string;
  rate: number;
  amount: number;
}

function OvertimeRequestScreen({ navigation }: any) {
  const [activeTab, setActiveTab] = useState<'request' | 'history'>('request');
  const [selectedDate, setSelectedDate] = useState('');
  const [startTime, setStartTime] = useState('');
  const [endTime, setEndTime] = useState('');
  const [reason, setReason] = useState('');
  const [loading, setLoading] = useState(false);

  const overtimeHistory: OvertimeRequest[] = [
    { id: 1, date: 'Dec 15, 2024', startTime: '06:00 PM', endTime: '10:00 PM', hours: 4, reason: 'Event security coverage', status: 'approved', approvedBy: 'Mr. Sharma', rate: 200, amount: 800 },
    { id: 2, date: 'Dec 12, 2024', startTime: '06:00 PM', endTime: '09:00 PM', hours: 3, reason: 'Staff shortage', status: 'approved', approvedBy: 'Mr. Sharma', rate: 200, amount: 600 },
    { id: 3, date: 'Dec 20, 2024', startTime: '06:00 PM', endTime: '11:00 PM', hours: 5, reason: 'Emergency coverage', status: 'pending', rate: 200, amount: 1000 },
    { id: 4, date: 'Dec 8, 2024', startTime: '06:00 PM', endTime: '08:00 PM', hours: 2, reason: 'Training completion', status: 'rejected', rate: 200, amount: 400 },
  ];

  const thisMonthStats = {
    totalHours: 14,
    totalAmount: 2800,
    pendingHours: 5,
    approvedHours: 9,
  };

  const handleSubmitRequest = () => {
    if (!selectedDate || !startTime || !endTime || !reason.trim()) {
      Alert.alert('Error', 'Please fill in all fields');
      return;
    }

    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      Alert.alert('Success', 'Overtime request submitted successfully. Awaiting approval.', [
        { text: 'OK', onPress: () => setActiveTab('history') }
      ]);
      setSelectedDate('');
      setStartTime('');
      setEndTime('');
      setReason('');
    }, 1500);
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
        <Text style={styles.headerTitle}>Overtime Request</Text>
        <View style={styles.placeholder} />
      </View>

      {/* Monthly Stats */}
      <View style={styles.statsCard}>
        <Text style={styles.statsTitle}>This Month</Text>
        <View style={styles.statsRow}>
          <View style={styles.statItem}>
            <Text style={styles.statValue}>{thisMonthStats.totalHours}h</Text>
            <Text style={styles.statLabel}>Total OT</Text>
          </View>
          <View style={styles.statDivider} />
          <View style={styles.statItem}>
            <Text style={[styles.statValue, { color: COLORS.success }]}>₹{thisMonthStats.totalAmount}</Text>
            <Text style={styles.statLabel}>Earned</Text>
          </View>
          <View style={styles.statDivider} />
          <View style={styles.statItem}>
            <Text style={[styles.statValue, { color: COLORS.warning }]}>{thisMonthStats.pendingHours}h</Text>
            <Text style={styles.statLabel}>Pending</Text>
          </View>
        </View>
      </View>

      {/* Tabs */}
      <View style={styles.tabContainer}>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'request' && styles.tabActive]}
          onPress={() => setActiveTab('request')}
        >
          <MaterialCommunityIcons name="plus-circle" size={20} color={activeTab === 'request' ? COLORS.primary : COLORS.gray500} />
          <Text style={[styles.tabText, activeTab === 'request' && styles.tabTextActive]}>New Request</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'history' && styles.tabActive]}
          onPress={() => setActiveTab('history')}
        >
          <MaterialCommunityIcons name="history" size={20} color={activeTab === 'history' ? COLORS.primary : COLORS.gray500} />
          <Text style={[styles.tabText, activeTab === 'history' && styles.tabTextActive]}>History</Text>
        </TouchableOpacity>
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {activeTab === 'request' ? (
          <View style={styles.formCard}>
            {/* Date Selection */}
            <Text style={styles.label}>Date</Text>
            <TouchableOpacity style={styles.dateInput}>
              <MaterialCommunityIcons name="calendar" size={20} color={COLORS.gray500} />
              <Text style={styles.dateText}>{selectedDate || 'Select Date'}</Text>
            </TouchableOpacity>

            {/* Time Selection */}
            <View style={styles.timeRow}>
              <View style={styles.timeField}>
                <Text style={styles.label}>Start Time</Text>
                <TouchableOpacity style={styles.timeInput}>
                  <MaterialCommunityIcons name="clock-outline" size={20} color={COLORS.gray500} />
                  <Text style={styles.timeText}>{startTime || '06:00 PM'}</Text>
                </TouchableOpacity>
              </View>
              <View style={styles.timeField}>
                <Text style={styles.label}>End Time</Text>
                <TouchableOpacity style={styles.timeInput}>
                  <MaterialCommunityIcons name="clock-outline" size={20} color={COLORS.gray500} />
                  <Text style={styles.timeText}>{endTime || '10:00 PM'}</Text>
                </TouchableOpacity>
              </View>
            </View>

            {/* Estimated Hours */}
            <View style={styles.estimateCard}>
              <View style={styles.estimateRow}>
                <Text style={styles.estimateLabel}>Estimated Hours</Text>
                <Text style={styles.estimateValue}>4 hours</Text>
              </View>
              <View style={styles.estimateRow}>
                <Text style={styles.estimateLabel}>Rate per Hour</Text>
                <Text style={styles.estimateValue}>₹200</Text>
              </View>
              <View style={[styles.estimateRow, styles.estimateTotal]}>
                <Text style={styles.totalLabel}>Estimated Amount</Text>
                <Text style={styles.totalValue}>₹800</Text>
              </View>
            </View>

            {/* Reason */}
            <Text style={styles.label}>Reason for Overtime</Text>
            <TextInput
              style={styles.reasonInput}
              placeholder="Explain why overtime is required..."
              value={reason}
              onChangeText={setReason}
              multiline
              numberOfLines={3}
              placeholderTextColor={COLORS.gray400}
            />

            {/* Submit Button */}
            <Button
              title="Submit Request"
              onPress={handleSubmitRequest}
              loading={loading}
              style={styles.submitBtn}
            />
          </View>
        ) : (
          <View style={styles.historyContainer}>
            {overtimeHistory.map((item) => (
              <View key={item.id} style={styles.historyCard}>
                <View style={styles.historyHeader}>
                  <View>
                    <Text style={styles.historyDate}>{item.date}</Text>
                    <Text style={styles.historyTime}>{item.startTime} - {item.endTime}</Text>
                  </View>
                  <View style={[styles.statusBadge, { backgroundColor: getStatusColor(item.status) + '15' }]}>
                    <Text style={[styles.statusText, { color: getStatusColor(item.status) }]}>
                      {item.status.charAt(0).toUpperCase() + item.status.slice(1)}
                    </Text>
                  </View>
                </View>
                
                <Text style={styles.historyReason}>{item.reason}</Text>
                
                <View style={styles.historyFooter}>
                  <View style={styles.historyDetail}>
                    <MaterialCommunityIcons name="clock-outline" size={16} color={COLORS.gray500} />
                    <Text style={styles.historyDetailText}>{item.hours} hours</Text>
                  </View>
                  <View style={styles.historyDetail}>
                    <MaterialCommunityIcons name="currency-inr" size={16} color={COLORS.success} />
                    <Text style={[styles.historyDetailText, { color: COLORS.success }]}>₹{item.amount}</Text>
                  </View>
                </View>
                
                {item.approvedBy && (
                  <Text style={styles.approvedBy}>Approved by: {item.approvedBy}</Text>
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
  statsCard: { marginHorizontal: SIZES.md, marginTop: SIZES.md, backgroundColor: COLORS.primary, borderRadius: SIZES.radiusMd, padding: SIZES.md },
  statsTitle: { fontSize: FONTS.caption, color: COLORS.white, opacity: 0.8, marginBottom: SIZES.sm },
  statsRow: { flexDirection: 'row', alignItems: 'center' },
  statItem: { flex: 1, alignItems: 'center' },
  statValue: { fontSize: FONTS.h3, fontWeight: 'bold', color: COLORS.white },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.white, opacity: 0.8, marginTop: 2 },
  statDivider: { width: 1, height: 40, backgroundColor: 'rgba(255,255,255,0.2)' },
  tabContainer: { flexDirection: 'row', marginHorizontal: SIZES.md, marginTop: SIZES.md, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: 4, ...SHADOWS.small },
  tab: { flex: 1, flexDirection: 'row', alignItems: 'center', justifyContent: 'center', paddingVertical: SIZES.sm, borderRadius: SIZES.radiusSm, gap: SIZES.xs },
  tabActive: { backgroundColor: COLORS.primary + '10' },
  tabText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, fontWeight: '500' },
  tabTextActive: { color: COLORS.primary, fontWeight: '600' },
  content: { padding: SIZES.md },
  formCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, ...SHADOWS.small },
  label: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm, marginTop: SIZES.md },
  dateInput: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.gray100, padding: SIZES.md, borderRadius: SIZES.radiusMd, gap: SIZES.sm },
  dateText: { fontSize: FONTS.body, color: COLORS.gray500 },
  timeRow: { flexDirection: 'row', gap: SIZES.md },
  timeField: { flex: 1 },
  timeInput: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.gray100, padding: SIZES.md, borderRadius: SIZES.radiusMd, gap: SIZES.sm },
  timeText: { fontSize: FONTS.body, color: COLORS.gray500 },
  estimateCard: { backgroundColor: COLORS.gray50, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginTop: SIZES.md },
  estimateRow: { flexDirection: 'row', justifyContent: 'space-between', paddingVertical: SIZES.xs },
  estimateLabel: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  estimateValue: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary },
  estimateTotal: { borderTopWidth: 1, borderTopColor: COLORS.gray200, marginTop: SIZES.xs, paddingTop: SIZES.sm },
  totalLabel: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  totalValue: { fontSize: FONTS.body, fontWeight: 'bold', color: COLORS.success },
  reasonInput: { backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusMd, padding: SIZES.md, fontSize: FONTS.body, color: COLORS.textPrimary, minHeight: 80, textAlignVertical: 'top' },
  submitBtn: { marginTop: SIZES.lg },
  historyContainer: {},
  historyCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  historyHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: SIZES.sm },
  historyDate: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  historyTime: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  statusText: { fontSize: FONTS.caption, fontWeight: '600' },
  historyReason: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginBottom: SIZES.sm },
  historyFooter: { flexDirection: 'row', gap: SIZES.md },
  historyDetail: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  historyDetailText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  approvedBy: { fontSize: FONTS.tiny, color: COLORS.success, marginTop: SIZES.xs },
});

export default OvertimeRequestScreen;
