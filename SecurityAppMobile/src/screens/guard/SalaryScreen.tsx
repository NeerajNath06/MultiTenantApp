import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, Dimensions, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { payrollService, GuardPayslipItem } from '../../services/payrollService';
import { authService } from '../../services/authService';

const { width } = Dimensions.get('window');

interface PayslipLineItem {
  label: string;
  amount: number;
  type: 'earning' | 'deduction';
}

interface PayslipHistoryItem {
  id: string;
  month: string;
  year: number;
  grossSalary: number;
  netSalary: number;
  status: 'paid' | 'pending' | 'processing';
  paidOn?: string;
  raw: GuardPayslipItem;
}

function SalaryScreen({ navigation }: any) {
  const [loading, setLoading] = useState(true);
  const [payslips, setPayslips] = useState<GuardPayslipItem[]>([]);
  const [payslipHistory, setPayslipHistory] = useState<PayslipHistoryItem[]>([]);
  const [selectedPayslip, setSelectedPayslip] = useState<GuardPayslipItem | null>(null);

  useEffect(() => {
    (async () => {
      try {
        const user = await authService.getStoredUser();
        const guardId = (user as { guardId?: string })?.guardId || user?.id;
        if (!guardId) return;
        const result = await payrollService.getGuardPayslips(guardId, { pageSize: 24 });
        if (result.success && result.data?.items?.length) {
          const items = result.data.items as GuardPayslipItem[];
          setPayslips(items);
          setSelectedPayslip(items[0] ?? null);
          const history: PayslipHistoryItem[] = items.map((p) => {
            const start = p.wagePeriodStart ? new Date(p.wagePeriodStart) : new Date();
            const paidOn = p.paymentDate ? new Date(p.paymentDate).toLocaleDateString('en-IN', { month: 'short', day: 'numeric', year: 'numeric' }) : undefined;
            const statusMap: Record<string, 'paid' | 'pending' | 'processing'> = { Paid: 'paid', Draft: 'pending', Approved: 'processing', Cancelled: 'pending' };
            const status = statusMap[p.status] ?? 'pending';
            return {
              id: p.id,
              month: start.toLocaleString('default', { month: 'long' }),
              year: start.getFullYear(),
              grossSalary: Number(p.grossAmount ?? 0),
              netSalary: Number(p.netAmount ?? 0),
              status,
              paidOn,
              raw: p,
            };
          });
          setPayslipHistory(history);
        }
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const currentPayslip: PayslipLineItem[] = selectedPayslip
    ? [
        ...(Number(selectedPayslip.basicAmount) > 0 ? [{ label: 'Basic Salary', amount: Number(selectedPayslip.basicAmount), type: 'earning' as const }] : []),
        ...(Number(selectedPayslip.overtimeAmount) > 0 ? [{ label: 'Overtime Pay', amount: Number(selectedPayslip.overtimeAmount), type: 'earning' as const }] : []),
        ...(Number(selectedPayslip.allowances) > 0 ? [{ label: 'Allowances', amount: Number(selectedPayslip.allowances), type: 'earning' as const }] : []),
        ...(Number(selectedPayslip.deductions) > 0 ? [{ label: 'Deductions', amount: Number(selectedPayslip.deductions), type: 'deduction' as const }] : []),
      ]
    : [];

  const selectedMonth = selectedPayslip?.wagePeriodStart
    ? `${new Date(selectedPayslip.wagePeriodStart).toLocaleString('default', { month: 'long' })} ${new Date(selectedPayslip.wagePeriodStart).getFullYear()}`
    : '—';

  const totalEarnings = currentPayslip.filter(item => item.type === 'earning').reduce((sum, item) => sum + item.amount, 0);
  const totalDeductions = currentPayslip.filter(item => item.type === 'deduction').reduce((sum, item) => sum + item.amount, 0);
  const netSalary = selectedPayslip ? Number(selectedPayslip.netAmount) : totalEarnings - totalDeductions;

  const handleDownloadPayslip = (payslip: PayslipHistoryItem) => {
    Alert.alert('Download', `Downloading payslip for ${payslip.month} ${payslip.year}...`);
  };

  const handleSelectPayslip = (p: PayslipHistoryItem) => {
    setSelectedPayslip(p.raw);
  };

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Salary & Payslips</Text>
          <View style={styles.helpBtn} />
        </View>
        <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
          <ActivityIndicator size="large" color={COLORS.primary} />
        </View>
      </SafeAreaView>
    );
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'paid': return COLORS.success;
      case 'processing': return COLORS.warning;
      case 'pending': return COLORS.error;
      default: return COLORS.gray500;
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Salary & Payslips</Text>
        <TouchableOpacity style={styles.helpBtn}>
          <MaterialCommunityIcons name="help-circle-outline" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      <ScrollView showsVerticalScrollIndicator={false}>
        {/* Salary Summary Card */}
        <View style={styles.summaryCard}>
          <Text style={styles.summaryTitle}>Net Salary</Text>
          <Text style={styles.salaryAmount}>₹{netSalary.toLocaleString()}</Text>
          <Text style={styles.summaryMonth}>{selectedMonth}</Text>
          
          <View style={styles.summaryStats}>
            <View style={styles.summaryStatItem}>
              <View style={[styles.statDot, { backgroundColor: COLORS.success }]} />
              <View>
                <Text style={styles.statLabel}>Earnings</Text>
                <Text style={[styles.statValue, { color: COLORS.success }]}>₹{totalEarnings.toLocaleString()}</Text>
              </View>
            </View>
            <View style={styles.summaryStatItem}>
              <View style={[styles.statDot, { backgroundColor: COLORS.error }]} />
              <View>
                <Text style={styles.statLabel}>Deductions</Text>
                <Text style={[styles.statValue, { color: COLORS.error }]}>₹{totalDeductions.toLocaleString()}</Text>
              </View>
            </View>
          </View>
        </View>

        {/* Payslip Breakdown */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Payslip Breakdown</Text>
          <View style={styles.breakdownCard}>
            {/* Earnings */}
            <View style={styles.breakdownSection}>
              <View style={styles.breakdownHeader}>
                <MaterialCommunityIcons name="plus-circle" size={20} color={COLORS.success} />
                <Text style={styles.breakdownTitle}>Earnings</Text>
              </View>
              {(currentPayslip.length ? currentPayslip.filter(item => item.type === 'earning') : [{ label: 'Basic Salary', amount: 0, type: 'earning' as const }]).map((item, index) => (
                <View key={index} style={styles.breakdownItem}>
                  <Text style={styles.breakdownLabel}>{item.label}</Text>
                  <Text style={styles.breakdownAmount}>₹{item.amount.toLocaleString()}</Text>
                </View>
              ))}
              <View style={[styles.breakdownItem, styles.totalRow]}>
                <Text style={styles.totalLabel}>Total Earnings</Text>
                <Text style={[styles.totalAmount, { color: COLORS.success }]}>₹{totalEarnings.toLocaleString()}</Text>
              </View>
            </View>

            <View style={styles.divider} />

            {/* Deductions */}
            <View style={styles.breakdownSection}>
              <View style={styles.breakdownHeader}>
                <MaterialCommunityIcons name="minus-circle" size={20} color={COLORS.error} />
                <Text style={styles.breakdownTitle}>Deductions</Text>
              </View>
              {(currentPayslip.length ? currentPayslip.filter(item => item.type === 'deduction') : [{ label: 'Deductions', amount: 0, type: 'deduction' as const }]).map((item, index) => (
                <View key={index} style={styles.breakdownItem}>
                  <Text style={styles.breakdownLabel}>{item.label}</Text>
                  <Text style={[styles.breakdownAmount, { color: COLORS.error }]}>-₹{item.amount.toLocaleString()}</Text>
                </View>
              ))}
              <View style={[styles.breakdownItem, styles.totalRow]}>
                <Text style={styles.totalLabel}>Total Deductions</Text>
                <Text style={[styles.totalAmount, { color: COLORS.error }]}>-₹{totalDeductions.toLocaleString()}</Text>
              </View>
            </View>

            <View style={styles.netRow}>
              <Text style={styles.netLabel}>Net Payable</Text>
              <Text style={styles.netAmount}>₹{netSalary.toLocaleString()}</Text>
            </View>
          </View>
        </View>

        {/* Payslip History */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Payslip History</Text>
          {(payslipHistory.length ? payslipHistory : []).map((payslip) => (
            <TouchableOpacity
              key={payslip.id}
              style={styles.historyCard}
              onPress={() => handleSelectPayslip(payslip)}
            >
              <View style={styles.historyIcon}>
                <MaterialCommunityIcons name="file-document" size={24} color={COLORS.primaryBlue} />
              </View>
              <View style={styles.historyInfo}>
                <Text style={styles.historyMonth}>{payslip.month} {payslip.year}</Text>
                <Text style={styles.historyAmount}>Net: ₹{payslip.netSalary.toLocaleString()}</Text>
                {payslip.paidOn && (
                  <Text style={styles.historyPaidOn}>Paid on {payslip.paidOn}</Text>
                )}
              </View>
              <View style={styles.historyRight}>
                <View style={[styles.statusBadge, { backgroundColor: getStatusColor(payslip.status) + '15' }]}>
                  <Text style={[styles.statusText, { color: getStatusColor(payslip.status) }]}>
                    {payslip.status.charAt(0).toUpperCase() + payslip.status.slice(1)}
                  </Text>
                </View>
                <TouchableOpacity onPress={() => handleDownloadPayslip(payslip)}><MaterialCommunityIcons name="download" size={20} color={COLORS.primaryBlue} /></TouchableOpacity>
              </View>
            </TouchableOpacity>
          ))}
        </View>

        {/* Quick Actions */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Quick Actions</Text>
          <View style={styles.actionsRow}>
            <TouchableOpacity style={styles.actionCard} onPress={() => Alert.alert('Tax Statement', 'Download annual tax statement')}>
              <MaterialCommunityIcons name="file-certificate" size={28} color={COLORS.secondary} />
              <Text style={styles.actionLabel}>Tax Statement</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionCard} onPress={() => Alert.alert('Salary Certificate', 'Request salary certificate')}>
              <MaterialCommunityIcons name="certificate" size={28} color={COLORS.warning} />
              <Text style={styles.actionLabel}>Salary Certificate</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionCard} onPress={() => navigation.navigate('Support')}>
              <MaterialCommunityIcons name="help-circle" size={28} color={COLORS.info} />
              <Text style={styles.actionLabel}>Help</Text>
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
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.primary },
  backBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.white },
  helpBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  summaryCard: { marginHorizontal: SIZES.md, marginTop: -20, backgroundColor: COLORS.white, borderRadius: SIZES.radiusLg, padding: SIZES.lg, alignItems: 'center', ...SHADOWS.large },
  summaryTitle: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  salaryAmount: { fontSize: 42, fontWeight: 'bold', color: COLORS.primary, marginTop: SIZES.xs },
  summaryMonth: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
  summaryStats: { flexDirection: 'row', marginTop: SIZES.lg, width: '100%', justifyContent: 'space-around' },
  summaryStatItem: { flexDirection: 'row', alignItems: 'center' },
  statDot: { width: 10, height: 10, borderRadius: 5, marginRight: SIZES.sm },
  statLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  statValue: { fontSize: FONTS.body, fontWeight: '600' },
  section: { paddingHorizontal: SIZES.md, marginTop: SIZES.lg },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  breakdownCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, ...SHADOWS.small },
  breakdownSection: { marginBottom: SIZES.sm },
  breakdownHeader: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.sm },
  breakdownTitle: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary, marginLeft: SIZES.xs },
  breakdownItem: { flexDirection: 'row', justifyContent: 'space-between', paddingVertical: SIZES.xs },
  breakdownLabel: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  breakdownAmount: { fontSize: FONTS.bodySmall, color: COLORS.textPrimary, fontWeight: '500' },
  totalRow: { borderTopWidth: 1, borderTopColor: COLORS.gray200, marginTop: SIZES.xs, paddingTop: SIZES.sm },
  totalLabel: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary },
  totalAmount: { fontSize: FONTS.body, fontWeight: '600' },
  divider: { height: 1, backgroundColor: COLORS.gray200, marginVertical: SIZES.md },
  netRow: { flexDirection: 'row', justifyContent: 'space-between', backgroundColor: COLORS.primary + '10', padding: SIZES.md, borderRadius: SIZES.radiusSm, marginTop: SIZES.sm },
  netLabel: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.primary },
  netAmount: { fontSize: FONTS.h4, fontWeight: 'bold', color: COLORS.primary },
  historyCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  historyIcon: { width: 48, height: 48, borderRadius: 24, backgroundColor: COLORS.primaryBlue + '15', justifyContent: 'center', alignItems: 'center' },
  historyInfo: { flex: 1, marginLeft: SIZES.sm },
  historyMonth: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  historyAmount: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  historyPaidOn: { fontSize: FONTS.tiny, color: COLORS.success, marginTop: 2 },
  historyRight: { alignItems: 'flex-end', gap: SIZES.xs },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  statusText: { fontSize: FONTS.tiny, fontWeight: '600' },
  actionsRow: { flexDirection: 'row', gap: SIZES.sm },
  actionCard: { flex: 1, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, alignItems: 'center', ...SHADOWS.small },
  actionLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs, textAlign: 'center' },
});

export default SalaryScreen;
