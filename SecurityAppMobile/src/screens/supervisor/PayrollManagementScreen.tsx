import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, Dimensions, RefreshControl, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { payrollService, type WageItem } from '../../services/payrollService';

const { width } = Dimensions.get('window');

function PayrollManagementScreen({ navigation }: any) {
  const [selectedMonth, setSelectedMonth] = useState(() => {
    const d = new Date();
    return `${d.toLocaleString('default', { month: 'long' })} ${d.getFullYear()}`;
  });
  const [wages, setWages] = useState<WageItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const getPeriod = useCallback((monthLabel: string) => {
    const [monthName, yearStr] = monthLabel.split(' ');
    const year = parseInt(yearStr || String(new Date().getFullYear()), 10);
    const monthIndex = new Date(Date.parse(monthName + ' 1, ' + year)).getMonth();
    const start = new Date(year, monthIndex, 1);
    const end = new Date(year, monthIndex + 1, 0);
    return { start: start.toISOString().slice(0, 10), end: end.toISOString().slice(0, 10) };
  }, []);

  const load = useCallback(async () => {
    const { start, end } = getPeriod(selectedMonth);
    const res = await payrollService.getWages({ periodStart: start, periodEnd: end, pageSize: 100 });
    if (res.success && res.data?.items) {
      setWages(res.data.items);
    } else {
      setWages([]);
    }
  }, [selectedMonth, getPeriod]);

  useEffect(() => {
    load().finally(() => setLoading(false));
  }, [load]);

  const onRefresh = async () => {
    setRefreshing(true);
    await load();
    setRefreshing(false);
  };

  const summary = {
    totalGuards: wages.length,
    totalBaseSalary: wages.reduce((s, w) => s + (w.totalWages ?? 0), 0),
    totalOvertime: 0,
    totalDeductions: 0,
    totalNetPayable: wages.reduce((s, w) => s + (w.netAmount ?? 0), 0),
    processed: wages.filter(w => (w.status || '').toLowerCase() === 'approved' || (w.status || '').toLowerCase() === 'processed').length,
    pending: wages.filter(w => (w.status || '').toLowerCase() === 'pending' || (w.status || '').toLowerCase() === 'draft').length,
    paid: wages.filter(w => (w.status || '').toLowerCase() === 'paid').length,
  };

  const getStatusColor = (status: string) => {
    const s = (status || '').toLowerCase();
    switch (s) {
      case 'paid': return COLORS.success;
      case 'processed':
      case 'approved': return COLORS.primaryBlue;
      case 'pending':
      case 'draft': return COLORS.warning;
      default: return COLORS.gray500;
    }
  };

  const handleProcessPayroll = (entry: WageItem) => {
    Alert.alert(
      'Process Payroll',
      `Process wage sheet ${entry.wageSheetNumber}?\n\nNet Amount: ₹${Number(entry.netAmount || 0).toLocaleString()}`,
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Process', onPress: () => Alert.alert('Success', 'Payroll processed successfully') }
      ]
    );
  };

  const handleBulkProcess = () => {
    Alert.alert(
      'Bulk Process',
      `Process payroll for all ${summary.pending} pending entries?`,
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Process All', onPress: () => Alert.alert('Success', 'All payrolls processed') }
      ]
    );
  };

  const goPrevMonth = () => {
    const [monthName, yearStr] = selectedMonth.split(' ');
    const year = parseInt(yearStr || '2024', 10);
    const d = new Date(Date.parse(monthName + ' 1, ' + year));
    d.setMonth(d.getMonth() - 1);
    setSelectedMonth(`${d.toLocaleString('default', { month: 'long' })} ${d.getFullYear()}`);
  };

  const goNextMonth = () => {
    const [monthName, yearStr] = selectedMonth.split(' ');
    const year = parseInt(yearStr || '2024', 10);
    const d = new Date(Date.parse(monthName + ' 1, ' + year));
    d.setMonth(d.getMonth() + 1);
    setSelectedMonth(`${d.toLocaleString('default', { month: 'long' })} ${d.getFullYear()}`);
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Payroll Management</Text>
        <TouchableOpacity style={styles.exportBtn}>
          <MaterialCommunityIcons name="download" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        {/* Month Selector */}
        <View style={styles.monthSelector}>
          <TouchableOpacity onPress={goPrevMonth}>
            <MaterialCommunityIcons name="chevron-left" size={24} color={COLORS.primary} />
          </TouchableOpacity>
          <View style={styles.monthInfo}>
            <MaterialCommunityIcons name="calendar" size={20} color={COLORS.primary} />
            <Text style={styles.monthText}>{selectedMonth}</Text>
          </View>
          <TouchableOpacity onPress={goNextMonth}>
            <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.primary} />
          </TouchableOpacity>
        </View>

        {loading ? (
          <View style={styles.loadingWrap}>
            <ActivityIndicator size="large" color={COLORS.primary} />
          </View>
        ) : (
        <>

        {/* Summary Card */}
        <View style={styles.summaryCard}>
          <Text style={styles.summaryTitle}>Payroll Summary</Text>
          <View style={styles.summaryRow}>
            <View style={styles.summaryItem}>
              <Text style={styles.summaryLabel}>Wage Sheets</Text>
              <Text style={styles.summaryValue}>{summary.totalGuards}</Text>
            </View>
            <View style={styles.summaryDivider} />
            <View style={styles.summaryItem}>
              <Text style={styles.summaryLabel}>Total Wages</Text>
              <Text style={styles.summaryValue}>₹{(summary.totalBaseSalary >= 100000 ? (summary.totalBaseSalary / 100000).toFixed(1) + 'L' : summary.totalBaseSalary.toLocaleString())}</Text>
            </View>
            <View style={styles.summaryDivider} />
            <View style={styles.summaryItem}>
              <Text style={styles.summaryLabel}>Net Payable</Text>
              <Text style={[styles.summaryValue, { color: COLORS.primary }]}>{summary.totalNetPayable >= 100000 ? '₹' + (summary.totalNetPayable / 100000).toFixed(1) + 'L' : '₹' + summary.totalNetPayable.toLocaleString()}</Text>
            </View>
          </View>
          <View style={styles.netPayableRow}>
            <Text style={styles.netPayableLabel}>Net Payable</Text>
            <Text style={styles.netPayableValue}>₹{summary.totalNetPayable.toLocaleString()}</Text>
          </View>
        </View>

        {/* Status Overview */}
        <View style={styles.statusRow}>
          <View style={[styles.statusCard, { backgroundColor: COLORS.success + '15' }]}>
            <Text style={[styles.statusValue, { color: COLORS.success }]}>{summary.paid}</Text>
            <Text style={styles.statusLabel}>Paid</Text>
          </View>
          <View style={[styles.statusCard, { backgroundColor: COLORS.primaryBlue + '15' }]}>
            <Text style={[styles.statusValue, { color: COLORS.primaryBlue }]}>{summary.processed}</Text>
            <Text style={styles.statusLabel}>Processed</Text>
          </View>
          <View style={[styles.statusCard, { backgroundColor: COLORS.warning + '15' }]}>
            <Text style={[styles.statusValue, { color: COLORS.warning }]}>{summary.pending}</Text>
            <Text style={styles.statusLabel}>Pending</Text>
          </View>
        </View>

        {/* Bulk Action */}
        {summary.pending > 0 && (
          <TouchableOpacity style={styles.bulkBtn} onPress={handleBulkProcess}>
            <MaterialCommunityIcons name="check-all" size={20} color={COLORS.white} />
            <Text style={styles.bulkBtnText}>Process All Pending ({summary.pending})</Text>
          </TouchableOpacity>
        )}

        {/* Payroll List - Wage sheets */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Wage Sheets</Text>
          {wages.length === 0 ? (
            <View style={styles.emptyWrap}>
              <Text style={styles.emptyText}>No wage data for this period</Text>
            </View>
          ) : (
          wages.map((entry) => (
            <View key={entry.id} style={styles.payrollCard}>
              <View style={styles.payrollHeader}>
                <View style={styles.guardInfo}>
                  <View style={styles.guardAvatar}>
                    <Text style={styles.avatarText}>{entry.wageSheetNumber.slice(0, 2).toUpperCase()}</Text>
                  </View>
                  <View>
                    <Text style={styles.guardName}>{entry.wageSheetNumber}</Text>
                    <Text style={styles.guardId}>{entry.wagePeriodStart?.slice(0, 10)} – {entry.wagePeriodEnd?.slice(0, 10)}</Text>
                  </View>
                </View>
                <View style={[styles.statusBadge, { backgroundColor: getStatusColor(entry.status) + '15' }]}>
                  <Text style={[styles.statusText, { color: getStatusColor(entry.status) }]}>
                    {(entry.status || 'Draft').charAt(0).toUpperCase() + (entry.status || 'Draft').slice(1)}
                  </Text>
                </View>
              </View>

              <View style={styles.payrollDetails}>
                <View style={styles.payrollRow}>
                  <Text style={styles.payrollLabel}>Total Wages</Text>
                  <Text style={styles.payrollValue}>₹{Number(entry.totalWages || 0).toLocaleString()}</Text>
                </View>
                <View style={[styles.payrollRow, styles.netRow]}>
                  <Text style={styles.netLabel}>Net Amount</Text>
                  <Text style={styles.netValue}>₹{Number(entry.netAmount || 0).toLocaleString()}</Text>
                </View>
              </View>

              {(entry.status || '').toLowerCase() === 'pending' || (entry.status || '').toLowerCase() === 'draft' ? (
                <TouchableOpacity style={styles.processBtn} onPress={() => handleProcessPayroll(entry)}>
                  <Text style={styles.processBtnText}>Process</Text>
                </TouchableOpacity>
              ) : null}
            </View>
          ))
          )}
        </View>
        </>
        )}

        <View style={{ height: 50 }} />
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.primary },
  backBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.white },
  exportBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  monthSelector: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.lg, paddingVertical: SIZES.md, backgroundColor: COLORS.white, marginBottom: 1 },
  monthInfo: { flexDirection: 'row', alignItems: 'center', gap: SIZES.sm },
  monthText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  summaryCard: { marginHorizontal: SIZES.md, marginTop: SIZES.md, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, ...SHADOWS.small },
  summaryTitle: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textSecondary, marginBottom: SIZES.md },
  summaryRow: { flexDirection: 'row', alignItems: 'center' },
  summaryItem: { flex: 1, alignItems: 'center' },
  summaryLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary },
  summaryValue: { fontSize: FONTS.h4, fontWeight: 'bold', color: COLORS.textPrimary, marginTop: 2 },
  summaryDivider: { width: 1, height: 40, backgroundColor: COLORS.gray200 },
  netPayableRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: SIZES.md, paddingTop: SIZES.md, borderTopWidth: 1, borderTopColor: COLORS.gray200 },
  netPayableLabel: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  netPayableValue: { fontSize: FONTS.h3, fontWeight: 'bold', color: COLORS.primary },
  statusRow: { flexDirection: 'row', paddingHorizontal: SIZES.md, marginTop: SIZES.md, gap: SIZES.sm },
  statusCard: { flex: 1, alignItems: 'center', padding: SIZES.md, borderRadius: SIZES.radiusMd },
  statusValue: { fontSize: FONTS.h3, fontWeight: 'bold' },
  statusLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 2 },
  bulkBtn: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.success, marginHorizontal: SIZES.md, marginTop: SIZES.md, paddingVertical: SIZES.md, borderRadius: SIZES.radiusMd, gap: SIZES.sm },
  bulkBtnText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.white },
  section: { padding: SIZES.md },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  payrollCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  payrollHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: SIZES.md },
  guardInfo: { flexDirection: 'row', alignItems: 'center' },
  guardAvatar: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center', marginRight: SIZES.sm },
  avatarText: { fontSize: FONTS.bodySmall, fontWeight: 'bold', color: COLORS.white },
  guardName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  guardId: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  statusText: { fontSize: FONTS.caption, fontWeight: '600' },
  payrollDetails: { backgroundColor: COLORS.gray50, borderRadius: SIZES.radiusSm, padding: SIZES.sm },
  payrollRow: { flexDirection: 'row', justifyContent: 'space-between', paddingVertical: SIZES.xs },
  payrollLabel: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  payrollValue: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary },
  netRow: { borderTopWidth: 1, borderTopColor: COLORS.gray200, marginTop: SIZES.xs, paddingTop: SIZES.sm },
  netLabel: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  netValue: { fontSize: FONTS.body, fontWeight: 'bold', color: COLORS.primary },
  processBtn: { backgroundColor: COLORS.primary, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusSm, alignItems: 'center', marginTop: SIZES.sm },
  processBtnText: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.white },
  loadingWrap: { paddingVertical: SIZES.xl * 2, alignItems: 'center' },
  emptyWrap: { paddingVertical: SIZES.lg, alignItems: 'center' },
  emptyText: { fontSize: FONTS.body, color: COLORS.textSecondary },
});

export default PayrollManagementScreen;
